using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using FMODUnity;

public class CarGearAudio : MonoBehaviour
{
    [Header("Assign in order: acc1 (0) to acc5 (4)")]
    public StudioEventEmitter[] accEmitters;

    [Header("Assign in order: decc1 (steady) (0) to decc5 (4)")]
    public StudioEventEmitter[] deccEmitters;

    [Header("Drag your Gear text object here (or leave empty to auto-find)")]
    public TMP_Text gearTMPText;
    public Text gearLegacyText;

    private int lastPlayedGear = -1;
    private bool isAcceleratingMode = false;
    private bool isDeceleratingMode = false;
    private bool lastPlayedWasDecc = false;

    void Start()
    {
        if (gearTMPText == null && gearLegacyText == null)
        {
            GameObject gearCanvas = GameObject.Find("Gear canvas");
            if (gearCanvas != null)
            {
                gearTMPText = gearCanvas.GetComponentInChildren<TMP_Text>();
                if (gearTMPText == null)
                {
                    gearLegacyText = gearCanvas.GetComponentInChildren<Text>();
                }
            }
        }
    }

    void Update()
    {
        if (Mouse.current == null) return;

        bool leftClicked = Mouse.current.leftButton.wasPressedThisFrame;
        bool rightClicked = Mouse.current.rightButton.wasPressedThisFrame;

        // 1. Capture Left/Right Click mode immediately
        if (leftClicked)
        {
            isAcceleratingMode = true;
            isDeceleratingMode = false;
        }
        else if (rightClicked)
        {
            isDeceleratingMode = true;
            isAcceleratingMode = false;
        }

        // Read the current gear number from the UI
        string rawText = "";
        if (gearTMPText != null) rawText = gearTMPText.text;
        else if (gearLegacyText != null) rawText = gearLegacyText.text;

        if (!int.TryParse(rawText.Trim(), out int currentGear)) return;

        // 2. When the car returns to Gear 0 (rest), reset tracking so the next 0 -> 1 launch always plays acc1
        if (currentGear <= 0)
        {
            lastPlayedGear = -1;
            lastPlayedWasDecc = false;
            return;
        }

        currentGear = Mathf.Clamp(currentGear, 1, 5);

        // 3. Play whenever mode switches (Acc <-> Decc) OR gear changes (including every 0 -> 1 launch)
        if (isAcceleratingMode)
        {
            if (currentGear != lastPlayedGear || lastPlayedWasDecc)
            {
                PlayFromArray(accEmitters, currentGear);
                lastPlayedGear = currentGear;
                lastPlayedWasDecc = false;
            }
        }
        else if (isDeceleratingMode)
        {
            if (currentGear != lastPlayedGear || !lastPlayedWasDecc)
            {
                PlayFromArray(deccEmitters, currentGear);
                lastPlayedGear = currentGear;
                lastPlayedWasDecc = true;
            }
        }
    }

    private void PlayFromArray(StudioEventEmitter[] targetArray, int gearNumber)
    {
        StopAllEmitters(accEmitters);
        StopAllEmitters(deccEmitters);

        int index = gearNumber - 1;
        if (targetArray != null && index >= 0 && index < targetArray.Length && targetArray[index] != null)
        {
            targetArray[index].Play();
        }
    }

    private void StopAllEmitters(StudioEventEmitter[] emitters)
    {
        if (emitters == null) return;
        for (int i = 0; i < emitters.Length; i++)
        {
            if (emitters[i] != null && emitters[i].IsPlaying())
            {
                emitters[i].Stop();
            }
        }
    }
}