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

    public CarPartCondition(string partName)
    {
        this.partName = partName;
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

    [Header("Race Settings")]
    [SerializeField] private int totalLaps = 4;
    [Tooltip("Numero totale di partecipanti, incluso il giocatore.")]
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
        new("Tyres"),
        new("Front Wing"),
        new("Rear Wing"),
        new("Brakes"),
        new("Seat"),
        new("Steering Wheel"),
        new("Pedals"),
        new("Gearbox"),
        new("Engine"),
        new("Suspension"),
        new("Helmet"),
        new("Battery")
    };

    public CarPartCondition[] CarParts => carParts;
        

    [Header("Component Deterioration Settings")]
    public UnityEvent onLapCompleted = new();

    public int CompletedLaps { get; private set; } 
    public int Position { get; private set; }
    public float DriveTime { get; private set; }
    public bool IsDriving { get; private set; }

    public int TotalLaps => totalLaps;
    public int Competitors => competitors;
    public int CurrentLap => Mathf.Min(CompletedLaps + 1, totalLaps);
    public bool IsRaceFinished => CompletedLaps >= totalLaps;
    public float RunTime => IsDriving ? Time.time - runStartTime : DriveTime;

    private float runStartTime;
    
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

        foreach (CarPartCondition part in carParts)
        {
            part.condition = 100;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ShopScene" && IsDriving)
        {
            CompleteSupplyRun();
        }

        IsDriving = scene.name == "DrivingScene" && !IsRaceFinished;

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
        }
    }

    private bool CompleteSupplyRun()
    {
        if (!IsDriving) return false;

        DriveTime = Time.time - runStartTime;
        IsDriving = false;

        UpdateRacePosition(DriveTime);

        CompletedLaps++;

        foreach (CarPartCondition part in carParts)
        {
            part.ApplyWear();
        }

        onLapCompleted.Invoke();

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

        if (distance >= length)
        {
            if (CompleteSupplyRun())
            {
                SceneFader.Instance.LoadScene("ShopScene");
            }
        }
    }
}
