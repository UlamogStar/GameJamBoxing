using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Attach this to an object in the scene and configure one or more thresholds.
// When the linked QTE completes a set, this component will fire the matching trigger.
public class NumSetEvent : MonoBehaviour
{
    [Header("QTE Reference")]
    public Quick_Time_Event qte;

    [Header("Set Triggers")]
    public SetTrigger[] triggers = new SetTrigger[0];

    void OnEnable()
    {
        EnsureMonitoring();
    }

    void OnDisable()
    {
        if (qte != null)
        {
            qte.OnSetComplete -= HandleSetComplete;
        }
    }

    void Start()
    {
        EnsureMonitoring();
    }

    void EnsureMonitoring()
    {
        if (qte == null)
        {
            qte = Object.FindFirstObjectByType<Quick_Time_Event>();
            if (qte == null)
            {
                Debug.LogWarning("NumSetEvent: No Quick_Time_Event found in the scene.");
                return;
            }
        }

        qte.OnSetComplete -= HandleSetComplete;
        qte.OnSetComplete += HandleSetComplete;
    }

    void HandleSetComplete()
    {
        if (qte == null)
            return;

        int completedSets = qte.setsCompleted;

        for (int i = 0; i < triggers.Length; i++)
        {
            SetTrigger trigger = triggers[i];
            if (trigger == null || trigger.hasTriggered)
                continue;

            bool shouldTrigger = trigger.triggerOnExactSetCount
                ? completedSets == trigger.setsToTrigger
                : completedSets >= trigger.setsToTrigger;

            if (shouldTrigger)
            {
                trigger.hasTriggered = true;
                TriggerEvent(trigger);
            }
        }
    }

    void TriggerEvent(SetTrigger trigger)
    {
        if (trigger == null)
            return;

        if (trigger.animator != null && !string.IsNullOrEmpty(trigger.triggerName))
        {
            trigger.animator.SetTrigger(trigger.triggerName);
        }

        if (trigger.onTriggered != null)
        {
            trigger.onTriggered.Invoke();
        }

        if (trigger.pauseGameBeforeTrigger)
        {
            StartCoroutine(PauseThenResume(trigger.pauseDuration));
        }
    }

    IEnumerator PauseThenResume(float pauseDuration)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, pauseDuration));

        Time.timeScale = originalTimeScale;
    }

    [System.Serializable]
    public class SetTrigger
    {
        [Header("Trigger Threshold")]
        [Tooltip("The number of completed sets required for this trigger to fire.")]
        public int setsToTrigger = 1;

        [Tooltip("If enabled, this trigger only fires when the set count is exactly the value above. Otherwise it fires when the count reaches or exceeds it.")]
        public bool triggerOnExactSetCount;

        [Header("Animation")]
        public Animator animator;
        public string triggerName = "Trigger";

        [Header("Pause")]
        [Tooltip("Pause the game briefly before firing the trigger.")]
        public bool pauseGameBeforeTrigger;
        public float pauseDuration = 0.5f;

        [Header("Events")]
        public UnityEvent onTriggered;

        [HideInInspector]
        public bool hasTriggered;
    }
}
