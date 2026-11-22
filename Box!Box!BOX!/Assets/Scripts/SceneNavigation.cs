using UnityEngine;

public class SceneNavigation : MonoBehaviour
{
    public void GoToDriving()
    {
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
