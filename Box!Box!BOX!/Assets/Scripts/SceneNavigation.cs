using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void GoToDriving()
    {
        SceneManager.LoadScene("DrivingScene");
    }

    public void GoToShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void GoToPits()
    {
        SceneManager.LoadScene("PitScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
