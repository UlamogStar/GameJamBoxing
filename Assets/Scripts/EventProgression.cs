using System.Collections;
using UnityEngine;
using TMPro;
//System to track the number of QTE sets completed and adjust difficulty accordingly. 

public class EventProgression : MonoBehaviour
{
    [Header("References")]
    public Quick_Time_Event qte;

    [Header("Progression Settings")]
    public float drainSpeedIncreasePerSet = 0.25f;
    public int pressesPerSetIncreasePerSet = 2; // amount to increment qte.pressesPerSet per set
    public float maxDrainSpeed = 3f; // cap for drainSpeed

    [Header("UI (optional)")]
    public TextMeshProUGUI setsText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI pressesText;


    void OnEnable()
    {
        EnsureAndStartMonitoring();
    }

    void OnDisable()
    {
        if (qte != null)
        {
            qte.OnSetComplete -= HandleSetComplete;
        }

        StopAllCoroutines();
    }

    void EnsureAndStartMonitoring()
    {
        if (qte == null)
        {
            // try to find a QTE in the scene if none assigned
            qte = Object.FindFirstObjectByType<Quick_Time_Event>();
            if (qte == null)
            {
                Debug.LogWarning("EventProgression: No Quick_Time_Event found in scene.");
                return;
            }
        }

        qte.OnSetComplete += HandleSetComplete;

        // initialize UI
        UpdateUI();
    }

    void HandleSetComplete()
    {
        AdvanceSet();
    }

    void AdvanceSet()
    {
        // Increase difficulty on the actual QTE component, clamped
        qte.pressesPerSet += pressesPerSetIncreasePerSet;
        qte.drainSpeed = Mathf.Min(qte.drainSpeed + drainSpeedIncreasePerSet, maxDrainSpeed);

        Debug.Log($"Advanced set. drainSpeed={qte.drainSpeed}, pressesPerSet={qte.pressesPerSet}");

        UpdateUI();
    }

    void UpdateUI()
    {
        if (qte == null)
            return;

        if (setsText != null)
            setsText.text = $"Sets: {qte.setsCompleted}";

        if (speedText != null)
            speedText.text = $"Speed: {qte.drainSpeed:F2}";

        if (pressesText != null)
            pressesText.text = $"Presses to complete: {qte.pressesPerSet}";

    }
}

