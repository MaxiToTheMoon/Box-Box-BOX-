using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    [SerializeField] CarHandler carHandler;

    void Update()
    {
        if (Time.timeScale == 0f || SceneFader.Instance.IsTransitioning ||
            (RaceManager.Instance != null && RaceManager.Instance.IsRaceFinished))
        {
            carHandler.SetInput(Vector2.zero);
            return;
        }

        Vector2 input = Vector2.zero;

        //Keyboard input
        if (Keyboard.current != null) {
            if (Keyboard.current.aKey.isPressed)
                input.x -= 1;
            if (Keyboard.current.dKey.isPressed)
                input.x += 1;

            if (Keyboard.current.rKey.wasPressedThisFrame)
                SceneFader.Instance.LoadScene(SceneManager.GetActiveScene().name);

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    carHandler.ShiftUp();
                    
                if (Mouse.current.rightButton.isPressed)
                    input.y -= 1;
            }
        }

        carHandler.SetInput(input);
    }
}
