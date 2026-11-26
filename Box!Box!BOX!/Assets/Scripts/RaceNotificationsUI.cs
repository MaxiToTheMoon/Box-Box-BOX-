using System.Collections;
using TMPro;
using UnityEngine;

public class RaceNotificationsUI : MonoBehaviour
{
    [Header("Position banner")]
    [SerializeField] private CanvasGroup positionBanner;
    [SerializeField] private TMP_Text positionText;
    [SerializeField, Min(0f)] private float bannerDuration = 2f;

    [Header("Results")]
    [SerializeField] private CanvasGroup resultsPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text finalPositionText;
    [SerializeField] private UnityEngine.UI.Button retryButton;
    [SerializeField] private UnityEngine.UI.Button menuButton;

    private RaceManager race;
    private SceneFader fader;
    private string pendingMessage;
    private Color pendingColor;
    private bool pendingResult;
    private bool pausedByResults;
    private float previousTimeScale;

    public void Initialize(RaceManager raceManager)
    {
        if (race != null) return;

        if (raceManager == null || positionBanner == null || positionText == null ||
            resultsPanel == null || titleText == null || finalPositionText == null ||
            retryButton == null || menuButton == null)
        {
            Debug.LogError("Assign all references on the RaceNotificationsUI prefab.", this);
            Destroy(gameObject);
            return;
        }

        race = raceManager;
        fader = SceneFader.Instance;
        ClearNotifications();

        race.OnPositionChanged += QueuePositionMessage;
        race.RaceFinished += HandleRaceFinished;
        race.RaceReset += ClearNotifications;
        fader.TransitionCompleted += ShowPendingNotification;
        retryButton.onClick.AddListener(Retry);
        menuButton.onClick.AddListener(ReturnToMenu);
    }

    private void QueuePositionMessage(int previous, int current)
    {
        int difference = previous - current;
        int count = Mathf.Abs(difference);

        if (difference == 0)
        {
            pendingMessage = $"POSITION UNCHANGED\nStill in P{current}";
            pendingColor = Color.white;
        }
        else
        {
            if (difference > 0)
            {
                pendingMessage = count == 1 ? "GAINED 1 POSITION" : $"GAINED {count} POSITIONS";
                pendingColor = new Color(0.35f, 1f, 0.55f);
            }
            else
            {
                pendingMessage = count == 1 ? "LOST 1 POSITION" : $"LOST {count} POSITIONS";
                pendingColor = new Color(1f, 0.4f, 0.4f);
            }

            pendingMessage += $"\nP{previous} → P{current}";
        }

        pendingMessage += $"\n<size=75%>Component wear: +{race.LastPenaltySeconds:0.##} s</size>";
        // CompleteSupplyRun runs before the shop loads: wait for the fade-in.
    }

    private void HandleRaceFinished()
    {
        StopAllCoroutines();
        positionBanner.alpha = 0f;
        pendingMessage = null;
        pendingResult = true;

        if (!fader.IsTransitioning)
            ShowPendingNotification();
    }

    private void ShowPendingNotification()
    {
        if (pendingResult)
        {
            pendingResult = false;
            ShowResults();
            return;
        }

        if (string.IsNullOrEmpty(pendingMessage)) return;

        string message = pendingMessage;
        pendingMessage = null;
        StopAllCoroutines();
        StartCoroutine(ShowBanner(message, pendingColor));
    }

    private IEnumerator ShowBanner(string message, Color color)
    {
        positionText.text = message;
        positionText.enableAutoSizing = true;
        positionText.fontSizeMin = 18f;
        positionText.fontSizeMax = 30f;
        positionText.color = color;
        positionBanner.alpha = 0f;

        yield return Fade(positionBanner, 1f, 0.2f);
        yield return new WaitForSecondsRealtime(bannerDuration);
        yield return Fade(positionBanner, 0f, 0.2f);
    }

    private void ShowResults()
    {
        StopAllCoroutines();
        positionBanner.alpha = 0f;
        titleText.text = race.Position == 1 ? "VICTORY!"
            : race.Position <= 3 ? "PODIUM FINISH!" : "RACE FINISHED";
        finalPositionText.text = $"You finished P{race.Position} out of {race.Competitors}";
        finalPositionText.text += $"\n<size=75%>Final component penalty: +{race.LastPenaltySeconds:0.##} s</size>";

        resultsPanel.alpha = 0f;
        resultsPanel.interactable = true;
        resultsPanel.blocksRaycasts = true;

        if (!pausedByResults)
        {
            previousTimeScale = Time.timeScale;
            pausedByResults = true;
        }

        Time.timeScale = 0f;
        StartCoroutine(Fade(resultsPanel, 1f, 0.25f));
    }

    private IEnumerator Fade(CanvasGroup group, float target, float duration)
    {
        float initial = group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(initial, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = target;
    }

    private void Retry() => LeaveResults("PitScene");
    private void ReturnToMenu() => LeaveResults("MainMenu");

    private void LeaveResults(string sceneName)
    {
        if (fader.IsTransitioning) return;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene unavailable: {sceneName}", this);
            return;
        }

        race.ResetRace();
        Time.timeScale = 1f;
        fader.LoadScene(sceneName);
    }

    private void ClearNotifications()
    {
        StopAllCoroutines();
        pendingMessage = null;
        pendingResult = false;
        positionBanner.alpha = 0f;
        positionBanner.interactable = false;
        positionBanner.blocksRaycasts = false;
        resultsPanel.alpha = 0f;
        resultsPanel.interactable = false;
        resultsPanel.blocksRaycasts = false;
        ResumeTime();
    }

    private void ResumeTime()
    {
        if (!pausedByResults) return;
        Time.timeScale = previousTimeScale;
        pausedByResults = false;
    }

    private void OnDestroy()
    {
        if (race != null)
        {
            race.OnPositionChanged -= QueuePositionMessage;
            race.RaceFinished -= HandleRaceFinished;
            race.RaceReset -= ClearNotifications;
        }

        if (fader != null)
            fader.TransitionCompleted -= ShowPendingNotification;
        if (retryButton != null)
            retryButton.onClick.RemoveListener(Retry);
        if (menuButton != null)
            menuButton.onClick.RemoveListener(ReturnToMenu);

        ResumeTime();
    }
}
