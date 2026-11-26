using TMPro;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class PitActionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private UnityEngine.UI.Image background;

    private RaceManager race;
    private UnityEngine.UI.Button button;
    private Sprite normalSprite;
    private Color normalColor;
    private UnityEngine.UI.Image.Type normalImageType;
    private string normalText;
    private Color normalTextColor;
    private bool normalAutoSizing;
    private float normalFontSize;
    private float normalMinSize;
    private float normalMaxSize;
    private Sprite checkerSprite;
    private Texture2D checkerTexture;
    private GameObject textBacking;

    private void Start()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (background == null) background = button.targetGraphic as UnityEngine.UI.Image;

        race = RaceManager.Instance;
        if (race == null || label == null || background == null)
        {
            Debug.LogError("PitActionButton requires RaceManager, a TMP label and a button image.", this);
            enabled = false;
            return;
        }

        normalSprite = background.sprite;
        normalColor = background.color;
        normalImageType = background.type;
        normalText = label.text;
        normalTextColor = label.color;
        normalAutoSizing = label.enableAutoSizing;
        normalFontSize = label.fontSize;
        normalMinSize = label.fontSizeMin;
        normalMaxSize = label.fontSizeMax;

        race.RaceReset += Refresh;
        race.RaceFinished += Refresh;
        race.PartsChanged += Refresh;
        race.onLapCompleted.AddListener(Refresh);
        Refresh();
    }

    private void Refresh()
    {
        bool finalPit = race.IsFinalPit || race.IsRaceFinished;
        button.interactable = !race.IsRaceFinished;

        if (finalPit)
        {
            if (checkerSprite == null) CreateCheckers();
            background.sprite = checkerSprite;
            background.type = UnityEngine.UI.Image.Type.Simple;
            background.color = Color.white;
            textBacking.SetActive(true);
            label.text = $"FINISH\nTHE RACE!\n<size=50%>Penalty: +{race.CurrentPenaltySeconds:0.##} s</size>";
            label.color = Color.white;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 36f;
        }
        else
        {
            background.sprite = normalSprite;
            background.type = normalImageType;
            background.color = normalColor;
            if (textBacking != null) textBacking.SetActive(false);
            label.text = normalText;
            label.color = normalTextColor;
            label.enableAutoSizing = normalAutoSizing;
            label.fontSize = normalFontSize;
            label.fontSizeMin = normalMinSize;
            label.fontSizeMax = normalMaxSize;
        }
    }

    private void CreateCheckers()
    {
        checkerTexture = new Texture2D(8, 4, TextureFormat.RGBA32, false);
        checkerTexture.name = "FinishButtonCheckers";
        checkerTexture.filterMode = FilterMode.Point;
        checkerTexture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 8; x++)
                checkerTexture.SetPixel(x, y, (x + y) % 2 == 0 ? Color.white : Color.black);

        checkerTexture.Apply();
        checkerSprite = Sprite.Create(checkerTexture, new Rect(0, 0, 8, 4), new Vector2(0.5f, 0.5f));

        textBacking = new GameObject("FinishTextBacking", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rect = (RectTransform)textBacking.transform;
        rect.SetParent(label.transform.parent, false);
        rect.SetSiblingIndex(label.transform.GetSiblingIndex());
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        UnityEngine.UI.Image backing = textBacking.GetComponent<UnityEngine.UI.Image>();
        backing.color = new Color(0f, 0f, 0f, 0.9f);
        backing.raycastTarget = false;
    }

    private void OnDestroy()
    {
        if (race != null)
        {
            race.RaceReset -= Refresh;
            race.RaceFinished -= Refresh;
            race.PartsChanged -= Refresh;
            race.onLapCompleted.RemoveListener(Refresh);
        }

        if (checkerSprite != null) Destroy(checkerSprite);
        if (checkerTexture != null) Destroy(checkerTexture);
        if (textBacking != null) Destroy(textBacking);
    }
}
