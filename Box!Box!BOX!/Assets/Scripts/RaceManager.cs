using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CarPartCondition
{
    public string partName;

    [Range(0, 100)] public int condition = 100;
    [Range(0, 100)] public int minWear;
    [Range(0, 100)] public int maxWear;
    [Min(0f)] public float maxPenaltySeconds = 5f;

    public float PenaltyMultiplier => condition >= 70 ? 0f
        : condition >= 40 ? 0.25f : condition >= 20 ? 0.6f : 1f;
    public float PenaltySeconds => Mathf.Max(0f, maxPenaltySeconds) * PenaltyMultiplier;

    public CarPartCondition(string partName, int minWear = 0, int maxWear = 0, float maxPenaltySeconds = 5f)
    {
        this.partName = partName;
        this.minWear = minWear;
        this.maxWear = maxWear;
        this.maxPenaltySeconds = maxPenaltySeconds;
    }

    public void ApplyWear()
    {
        int min = Mathf.Min(minWear, maxWear);
        int max = Mathf.Max(minWear, maxWear);

        int wear = Random.Range(min, max + 1);

        condition = Mathf.Max(0, condition - wear);
    }
}

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    public event System.Action<int, int> OnPositionChanged;
    public event System.Action RaceFinished;
    public event System.Action RaceReset;
    public event System.Action PartsChanged;

    [Header("Notification Settings")]
    [SerializeField] private RaceNotificationsUI notificationsPrefab;

    [Header("Race Settings")]
    [SerializeField] private int totalLaps = 4;
    [Tooltip("Total number of participants, including the player.")]
    [SerializeField] private int competitors = 32;
    [SerializeField] private int playerPosition = 16;

    [Header("Drive Timing Settings")]
    [SerializeField] private float targetTime = 60f; 
    [SerializeField] private float secondsPerPosition = 10f;

    [Header("Route Settings")]
    [SerializeField] private int length = 1000;

    private Transform mechCar;
    private float routeStartZ;

    [Header("Car Parts Settings")]
    [SerializeField] private CarPartCondition[] carParts =
    {
        new("Tyres", 20, 40, 8f),
        new("Front Wing", 4, 16, 4f),
        new("Rear Wing", 3, 12, 3f),
        new("Brakes", 12, 25),
        new("Seat", 1, 4, 1f),
        new("Steering Wheel", 2, 6, 2f),
        new("Pedals", 3, 8, 2f),
        new("Gearbox", 8, 18),
        new("Engine", 10, 22, 7f),
        new("Suspension", 6, 15, 4f),
        new("Helmet", 1, 3, 1f),
        new("Battery", 8, 20, 3f)
    };

    public CarPartCondition[] CarParts => carParts;
        

    [Header("Component Deterioration Settings")]
    public UnityEvent onLapCompleted = new();

    public int CompletedLaps { get; private set; } 
    public int Position { get; private set; }
    public float DriveTime { get; private set; }
    public bool IsDriving { get; private set; }
    public float LastPenaltySeconds { get; private set; }
    public float CurrentPenaltySeconds
    {
        get
        {
            float total = 0f;
            foreach (CarPartCondition part in carParts) total += part.PenaltySeconds;
            return total;
        }
    }
    public int PredictedFinalPosition => Mathf.Clamp(Position +
        Mathf.FloorToInt(CurrentPenaltySeconds / Mathf.Max(1f, secondsPerPosition)), 1, Competitors);

    public int TotalLaps => totalLaps;
    public int Competitors => competitors;
    public int CurrentLap => Mathf.Min(CompletedLaps + 1, totalLaps);
    public bool IsRaceFinished => CompletedLaps >= totalLaps;
    public bool IsFinalPit => CurrentLap == TotalLaps && !IsRaceFinished;
    public float RunTime => IsDriving ? Time.time - runStartTime : DriveTime;

    private float runStartTime;
    private float departurePenaltySeconds;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        ResetRace();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (notificationsPrefab != null)
        {
            RaceNotificationsUI notifications = Instantiate(notificationsPrefab);
            DontDestroyOnLoad(notifications.gameObject);
            notifications.Initialize(this);
        }
        else
        {
            Debug.LogWarning("Assign Notifications Prefab on RaceManager to display notifications.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    public void ResetRace()
    {
        totalLaps = Mathf.Max(1, totalLaps);
        competitors = Mathf.Max(1, competitors);
        targetTime = Mathf.Max(1f, targetTime);
        secondsPerPosition = Mathf.Max(1f, secondsPerPosition);

        CompletedLaps = 0;
        Position = Mathf.Clamp(playerPosition, 1, competitors);

        DriveTime = 0f;
        runStartTime = 0f;
        IsDriving = false;
        departurePenaltySeconds = 0f;
        LastPenaltySeconds = 0f;

        foreach (CarPartCondition part in carParts)
        {
            part.condition = 100;
        }

        RaceReset?.Invoke();
        PartsChanged?.Invoke();
    }

    // Call after a replacement has actually been fitted; consume its inventory item only on success.
    public bool TryRepairPart(string partName)
    {
        if (string.IsNullOrWhiteSpace(partName) || IsDriving || IsRaceFinished ||
            SceneManager.GetActiveScene().name != "PitScene" ||
            (SceneFader.Instance != null && SceneFader.Instance.IsTransitioning)) return false;

        string normalized = partName.Replace('_', ' ').Trim();
        if (string.Equals(normalized, "Brake Calipers", System.StringComparison.OrdinalIgnoreCase))
            normalized = "Brakes";
        foreach (CarPartCondition part in carParts)
        {
            if (!string.Equals(part.partName, normalized, System.StringComparison.OrdinalIgnoreCase)) continue;
            if (part.condition >= 100) return false;
            part.condition = 100;
            PartsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void FinishRaceFromPit()
    {
        if (!IsFinalPit || IsDriving ||
            SceneManager.GetActiveScene().name != "PitScene" ||
            (SceneFader.Instance != null && SceneFader.Instance.IsTransitioning))
            return;

        LastPenaltySeconds = CurrentPenaltySeconds;
        // No supply trip on the last lap: only the remaining component penalty applies.
        UpdateRacePosition(targetTime + LastPenaltySeconds);
        CompletedLaps = totalLaps;
        IsDriving = false;
        RaceFinished?.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ShopScene" && IsDriving)
        {
            CompleteSupplyRun();
        }

        IsDriving = scene.name == "DrivingScene" && !IsRaceFinished && !IsFinalPit;

        mechCar = null;

        if (IsDriving)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("Player object not found in the scene.");
                IsDriving = false;
                return;
            }

            mechCar = player.transform;
            routeStartZ = mechCar.position.z;
            runStartTime = Time.time;
            DriveTime = 0f;
            departurePenaltySeconds = CurrentPenaltySeconds;
        }
    }

    private bool CompleteSupplyRun()
    {
        if (!IsDriving || IsRaceFinished || IsFinalPit) return false;

        DriveTime = Time.time - runStartTime;
        IsDriving = false;

        int previousPosition = Position;

        LastPenaltySeconds = departurePenaltySeconds;
        UpdateRacePosition(DriveTime + LastPenaltySeconds);
        CompletedLaps++;

        foreach (CarPartCondition part in carParts)
        {
            part.ApplyWear();
        }

        onLapCompleted.Invoke();
        PartsChanged?.Invoke();

        OnPositionChanged?.Invoke(previousPosition, Position);
        return true;
    }

    private void UpdateRacePosition(float evaluatedTime)
    {
        float timeDifference = evaluatedTime - targetTime;
        int positionChange = Mathf.FloorToInt(timeDifference / secondsPerPosition);

        Position += positionChange;
        Position = Mathf.Clamp(Position, 1, competitors);
    }

    private void Update()
    {
        if (!IsDriving || mechCar == null) return;

        float distance = mechCar.position.z - routeStartZ;

        if (distance < length) return;

        if (CompleteSupplyRun() && !IsRaceFinished)
        {
            SceneFader.Instance.LoadScene("ShopScene");
        }
    }
}
