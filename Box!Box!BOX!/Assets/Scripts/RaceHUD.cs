using UnityEngine;
using TMPro;

public class RaceHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text lapText;

    void Update()
    {
        RaceManager race = RaceManager.Instance;

        if (race==null)
            return;

        positionText.SetText(
            "<size=22>POSITION</size>\n<b>{0}°</b>",
            race.Position
        );

        lapText.SetText(
            "<size=22>LAP</size>\n<b>{0} / {1}</b>",
            race.CurrentLap,
            race.TotalLaps
        );
    }
}
