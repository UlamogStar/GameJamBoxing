using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class SliderProgressEvent : MonoBehaviour
{
    [System.Serializable]
    public class ProgressEvent
    {
        [Range(0f, 1f)]
        public float targetValue = 0.5f;

        [Tooltip("True to fire when progress passes below the target value, false to fire when progress passes above it.")]
        public bool fireOnBelow = true;

        [Tooltip("Optional AudioSystem key to play when this event triggers.")]
        public string audioKey;

        public UnityEvent onTriggered;
    }

    public Slider slider;
    public string progressParam = "Progress";

    [Tooltip("Configure one or more progress events that fire when the slider reaches a target value.")]
    public List<ProgressEvent> progressEvents = new List<ProgressEvent>();

    private float lastProgress = 0f;

    void Start()
    {
        lastProgress = GetNormalizedProgress();
    }

    void Update()
    {
        if (slider == null)
            return;

        float normalizedProgress = GetNormalizedProgress();
        UpdateProgressEvents(normalizedProgress);
        lastProgress = normalizedProgress;
    }

    private float GetNormalizedProgress()
    {
        return Mathf.Clamp01(slider.value / Mathf.Max(slider.maxValue, 0.0001f));
    }

    private void UpdateProgressEvents(float progress)
    {
        for (int i = 0; i < progressEvents.Count; i++)
        {
            var evt = progressEvents[i];
            bool shouldFire = false;

            if (evt.fireOnBelow)
            {
                shouldFire = lastProgress >= evt.targetValue && progress < evt.targetValue;
            }
            else
            {
                shouldFire = lastProgress <= evt.targetValue && progress > evt.targetValue;
            }

            if (shouldFire)
            {
                evt.onTriggered?.Invoke();
                if (!string.IsNullOrEmpty(evt.audioKey) && AudioSystem.Instance != null)
                {
                    AudioSystem.Instance.Play(evt.audioKey);
                }
            }
        }
    }
}
