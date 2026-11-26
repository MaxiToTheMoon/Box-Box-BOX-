using System.Text;
using TMPro;
using UnityEngine;

public class RaceCarStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private RaceManager race;

    private void Start()
    {
        race = RaceManager.Instance;

        if (race == null || statusText == null)
        {
            Debug.LogError(
                "CarStatusUI: RaceManager or StatusText is missing.",
                this
            );

            return;
        }

        statusText.richText = true;
        race.PartsChanged += Refresh;
        race.RaceFinished += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (race != null)
        {
            race.PartsChanged -= Refresh;
            race.RaceFinished -= Refresh;
        }
    }

    public void Refresh()
    {
        if (race == null || statusText == null) return;

        StringBuilder text = new();

        text.AppendLine("<size=28><b>RACE CAR STATUS</b></size>");
        string caption = race.IsRaceFinished ? "FINAL PENALTY APPLIED"
            : race.IsFinalPit ? "EXPECTED FINAL PENALTY" : "NEXT TRIP PENALTY";
        float penalty = race.IsRaceFinished ? race.LastPenaltySeconds : race.CurrentPenaltySeconds;
        text.AppendLine($"<size=16><color=#B8C0CC>{caption}</color></size>");
        text.AppendLine($"<size=28><b>+{penalty:0.##} s</b></size>");
        if (race.IsFinalPit)
            text.AppendLine($"<size=18>Expected position: {race.PredictedFinalPosition} / {race.Competitors}</size>");
        text.AppendLine("<size=16><color=#B8C0CC>COMPONENT<pos=75%>STATE\n</color></size>");

        foreach (CarPartCondition part in race.CarParts)
        {
            int minimum = Mathf.Min(part.minWear, part.maxWear);
            int maximum = Mathf.Max(part.minWear, part.maxWear);

            string wear = maximum == 0
                ? "0%"
                : minimum == maximum
                    ? $"{minimum}%"
                    : $"{minimum}-{maximum}%";

            float severity = part.PenaltyMultiplier;
            string color = severity == 0f ? "69DE91" : severity == 0.25f ? "F2D35E"
                : severity == 0.6f ? "FFAA55" : "FF6B70";
            text.AppendLine($"<b>{part.partName}<pos=75%><color=#{color}>{part.condition}%</color></b>");
            text.AppendLine($"<size=17><color=#B8C0CC>Wear {wear}/lap<pos=65%>+{part.PenaltySeconds:0.##} s\n</color></size>");
        }

        text.AppendLine("<size=16><color=#69DE91>70-100%</color>  <color=#F2D35E>40-69%</color>  <color=#FFAA55>20-39%</color>  <color=#FF6B70>0-19%</color></size>");

        statusText.text = text.ToString();
    }
}
