using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI distanceTravelledText;
    [SerializeField] TextMeshProUGUI gearText;
    [SerializeField] TextMeshProUGUI speedText;

    CarHandler playerCarHandler;

    void Awake()
    {
        playerCarHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<CarHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelledText.text = playerCarHandler.DistanceTravelled.ToString("000000");

        if (playerCarHandler.currentGear == 0)
            gearText.text = "F";
        else
            gearText.text = playerCarHandler.currentGear.ToString();

        speedText.text = Mathf.RoundToInt(playerCarHandler.CurrentSpeed).ToString();
    }
}
