using UnityEngine;

public class SceneNavigation : MonoBehaviour
{
    public void GoToDriving()
    {
        if (SceneFader.Instance.IsTransitioning) return;

        RaceManager race = RaceManager.Instance;
        if (race != null)
        {
            if (race.IsRaceFinished) return;
            if (race.IsFinalPit)
            {
                race.FinishRaceFromPit();
                return;
            }
        }

        SceneFader.Instance.LoadScene("DrivingScene");
    }

    public void GoToShop()
    {
        SceneFader.Instance.LoadScene("ShopScene");
    }

    public void GoToPits()
    {
        SceneFader.Instance.LoadScene("PitScene");
    }

    public void GoToMainMenu()
    {
        SceneFader.Instance.LoadScene("MainMenu");
    }
}
