using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    private static SceneFader instance;

    public static SceneFader Instance
    {
        get
        {
            if (instance == null)
                new GameObject("SceneFader").AddComponent<SceneFader>();

            return instance;
        }
    }

    [SerializeField] private CanvasGroup overlay;
    [SerializeField, Min(0f)] private float duration = 0.6f;

    private bool isTransitioning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (overlay == null)
            overlay = GetComponent<CanvasGroup>();

        if (overlay == null)
            overlay = CreateOverlay();

        overlay.alpha = 0f;
        overlay.blocksRaycasts = false;
    }

    private CanvasGroup CreateOverlay()
    {
        GameObject canvasObject = new GameObject("FadeCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup),
            typeof(UnityEngine.UI.GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        GameObject imageObject = new GameObject("BlackOverlay",
            typeof(RectTransform), typeof(UnityEngine.UI.Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image image = imageObject.GetComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        return canvasObject.GetComponent<CanvasGroup>();
    }

    public void LoadScene(string sceneName)
    {
        if (isTransitioning) return;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scena non disponibile: {sceneName}");
            return;
        }

        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;
        overlay.blocksRaycasts = true;

        yield return FadeTo(1f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return FadeTo(0f);

        overlay.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator FadeTo(float target)
    {
        float initialAlpha = overlay.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(
                initialAlpha, target, Mathf.Clamp01(elapsed / duration));

            yield return null;
        }

        overlay.alpha = target;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
