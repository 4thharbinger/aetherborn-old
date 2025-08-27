using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float[] timeSteps;

    public float currentTime;

    public float dampeningTime;

    public float physicsUpdateThreshold = 10f;

    public int current;

    public static bool physicsUpdate = false;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        InputSystem.actions.FindAction("Speed Up").performed += SpeedUp;
        InputSystem.actions.FindAction("Slow Down").performed += SlowDown;
    }

    private void OnDestroy()
    {
        InputSystem.actions.FindAction("Speed Up").performed -= SpeedUp;
        InputSystem.actions.FindAction("Slow Down").performed -= SlowDown;
    }

    private void SpeedUp(InputAction.CallbackContext obj)
    {
        if (current < timeSteps.Length - 1)
        current++;
    }

    private void SlowDown(InputAction.CallbackContext obj)
    {
        if (current > 0)
        current--;
    }

    private void FixedUpdate()
    {
        Physics.simulationMode = SimulationMode.FixedUpdate;
        var targetTime = timeSteps[current];

        if (paused)
        {
            Time.timeScale = 0;
            text.text = $"PAUSED";
        }
        else
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, targetTime, ref dampeningTime, 0.1f, float.PositiveInfinity, Time.fixedUnscaledDeltaTime);
            text.text = $"{Time.timeScale:F3}x";
        }

        physicsUpdate = Time.timeScale < physicsUpdateThreshold;


        if (paused || !physicsUpdate)
        {
            Physics.simulationMode = SimulationMode.Script;
        } else
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }
    }

    public bool paused;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            paused = !paused;
            if (!paused)
            {
                Time.timeScale = timeSteps[current];
            }
        }
    }
}
