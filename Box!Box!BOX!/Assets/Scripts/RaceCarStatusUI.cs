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
                "CarStatusUI: manca RaceManager o StatusText.",
                this
            );

            return;
        }

        race.onLapCompleted.AddListener(Refresh);
        Refresh();
    }

    private void OnDestroy()
    {
        if (race != null)
        {
            race.onLapCompleted.RemoveListener(Refresh);
        }
    }

    public void Refresh()
    {
        if (race == null || statusText == null) return;

        StringBuilder text = new();

        text.AppendLine("<size=32><b>RACE CAR STATUS</b></size>");
        text.AppendLine();
        text.AppendLine();
        text.AppendLine("Race car state / Wear per lap");
        text.AppendLine();

        foreach (CarPartCondition part in race.CarParts)
        {
            int minimum = Mathf.Min(part.minWear, part.maxWear);
            int maximum = Mathf.Max(part.minWear, part.maxWear);

            string wear = maximum == 0
                ? "0%"
                : minimum == maximum
                    ? $"-{minimum}%"
                    : $"-{minimum} to -{maximum}%";

            text.AppendLine(
                $"{part.partName} {part.condition}% / {wear}"
            );
            text.AppendLine();
        }

        statusText.text = text.ToString();
    }
}