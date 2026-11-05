using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("PitScene");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
