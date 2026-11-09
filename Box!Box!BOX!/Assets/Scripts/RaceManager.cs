using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; }

    [Header("Race Settings")]
    [SerializeField] private int totalLaps = 4;
    [SerializeField] private int competitors = 31;
    [SerializeField] private int playerPosition = 16;

    [Header("Drive Timing Settings")]
    [SerializeField] private float targetTime = 60f; 
    [SerializeField] private float secondsPerPosition = 10f;

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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsDriving = scene.name == "DriveScene" && !IsRaceFinished;

        if (IsDriving)
        {
            runStartTime = Time.time;
        }
    }

    private bool CompleteSupplyRun()
    {
        if (!IsDriving) return false;

        DriveTime = Time.time - runStartTime;
        IsDriving = false;

        UpdateRacePosition(DriveTime);

        CompletedLaps++;

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
}
