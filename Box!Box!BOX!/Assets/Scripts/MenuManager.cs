using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        if (SceneFader.Instance.IsTransitioning) return;

        Time.timeScale = 1f;
        if (RaceManager.Instance != null)
            RaceManager.Instance.ResetRace();

        SceneFader.Instance.LoadScene("PitScene");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
