using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI distanceTravelledText;
    [SerializeField] TextMeshProUGUI gearText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI timerText;

    CarHandler playerCarHandler;

    void Awake()
    {
        playerCarHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<CarHandler>();
    }

    private void UpdateTimer()
    {
        if (timerText == null || RaceManager.Instance == null) return;

        float time = RaceManager.Instance.RunTime;

        int centiseconds = Mathf.FloorToInt(time * 100f);
        int minutes = centiseconds / 6000;
        int seconds = centiseconds / 100 % 60;
        int fraction = centiseconds % 100;

        timerText.text = $"{minutes:00}:{seconds:00}:{fraction:00}";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
        
        distanceTravelledText.text = playerCarHandler.DistanceTravelled.ToString("000000");

        if (playerCarHandler.currentGear == 0)
            gearText.text = "F";
        else
            gearText.text = playerCarHandler.currentGear.ToString();

        speedText.text = Mathf.RoundToInt(playerCarHandler.CurrentSpeed).ToString();
    }
}
