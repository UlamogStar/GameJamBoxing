using System;
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
    [Header("Set Settings")]
    public int pressesPerSet = 5;

    [Header("Button Switching")]
    public int pressesNeededToSwitch = 5;

    [Header("UI")]
    public TextMeshProUGUI promptText;
    public Slider timerSlider;

    private InputActionReference currentAction;

    private float currentTime;
    private int pressCount;
    private int switchPressCount;
    private bool qteActive;

    public event Action OnFail;
    public event Action OnStart;
    public event Action OnSetComplete;

    [Header("Tracking")]
    public int setsCompleted; // number of sets (successful press-goals) completed
    public float totalTimeSurvived; // total time accumulated from completed sets and failures

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
        switchPressCount++;

        // Change button after enough presses
        if (switchPressCount >= pressesNeededToSwitch)
        {
            ChangeAction();
        }

        // Set completion
        if (pressCount >= pressesPerSet)
        {
            CompleteSet();
        }
    }


    void ChangeAction()
    {
        switchPressCount = 0;

        InputActionReference newAction;

        do
        {
            newAction = actions[UnityEngine.Random.Range(0, actions.Length)];

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


        currentAction = actions[UnityEngine.Random.Range(0, actions.Length)];

        currentTime = maxTime;
        pressCount = 0;
        qteActive = true;

        OnStart?.Invoke();


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

        // accumulate time survived on fail
        float timeSurvivedOnFail = maxTime - currentTime;
        totalTimeSurvived += timeSurvivedOnFail;

        OnFail?.Invoke();
        Invoke(nameof(HideUI), 1f);
    }


    void CompleteSet()
    {
        pressCount = 0;

        float timeSurvived = maxTime - currentTime;
        totalTimeSurvived += timeSurvived;
        setsCompleted++;
        OnSetComplete?.Invoke();
    }


    void HideUI()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (timerSlider != null)
            timerSlider.gameObject.SetActive(false);
    }
}