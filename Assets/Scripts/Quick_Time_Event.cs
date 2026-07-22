using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Quick_Time_Event : MonoBehaviour
{
    [Header("QTE Settings")]
    public InputActionReference[] actions;

    public float maxTime = 5f;
    public float drainSpeed = 1f;
    public float timeAddedPerPress = 0.5f;

    [Header("Button Switching")]
    public int pressesNeededToSwitch = 5;

    [Header("UI")]
    public TextMeshProUGUI promptText;
    public Slider timerSlider;

    private InputActionReference currentAction;

    private float currentTime;
    private int pressCount;
    private bool qteActive;

    void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);

        StartQTE();
    }


    void Update()
    {
        if (!qteActive)
            return;


        // Drain timer
        currentTime -= drainSpeed * Time.deltaTime;


        if (timerSlider != null)
            timerSlider.value = currentTime;


        // Player pressed correct button
        if (currentAction.action.triggered)
        {
            ButtonPressed();
        }


        // Timer empty
        if (currentTime <= 0)
        {
            FailQTE();
        }
    }


    void ButtonPressed()
    {
        // Add time
        currentTime += timeAddedPerPress;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);


        pressCount++;


        // Change button after enough presses
        if (pressCount >= pressesNeededToSwitch)
        {
            ChangeAction();
        }
    }


    void ChangeAction()
    {
        pressCount = 0;


        InputActionReference newAction;

        do
        {
            newAction = actions[Random.Range(0, actions.Length)];

        } while (newAction == currentAction && actions.Length > 1);


        currentAction = newAction;


        UpdatePrompt();
    }


    public void StartQTE()
    {
        if (actions.Length == 0)
        {
            Debug.LogError("No actions assigned!");
            return;
        }


        currentAction = actions[Random.Range(0, actions.Length)];

        currentTime = maxTime;
        pressCount = 0;
        qteActive = true;


        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.maxValue = maxTime;
            timerSlider.value = currentTime;
        }


        if (promptText != null)
            promptText.gameObject.SetActive(true);


        UpdatePrompt();
    }


    void UpdatePrompt()
    {
        if (promptText != null)
        {
            promptText.text =
                $"Mash {currentAction.action.GetBindingDisplayString()}!";
        }
    }


    void FailQTE()
    {
        qteActive = false;

        if (promptText != null)
            promptText.text = "Failed!";

        Invoke(nameof(HideUI), 1f);
    }


    void SuccessQTE()
    {
        qteActive = false;

        if (promptText != null)
            promptText.text = "Success!";

        Invoke(nameof(HideUI), 1f);
    }


    void HideUI()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);
    }
}