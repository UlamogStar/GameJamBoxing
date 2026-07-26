using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Attach this to an object in the scene and configure one or more thresholds.
// When the linked QTE completes a set, this component will fire the matching trigger.
public class NumSetEvent : MonoBehaviour
{
    [Header("QTE Reference")]
    public Quick_Time_Event qte;

    [Header("Set Triggers")]
    public SetTrigger[] triggers = new SetTrigger[0];

    private CanvasGroup fadeCanvasGroup;

    void OnEnable()
    {
        EnsureMonitoring();
    }

    void OnDisable()
    {
        if (qte != null)
            qte.OnSetComplete -= HandleSetComplete;
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

        foreach (SetTrigger trigger in triggers)
        {
            if (trigger == null)
                continue;

            bool shouldTrigger = false;

            if (trigger.triggerOnInterval)
            {
                if (trigger.setInterval > 0 &&
                    completedSets > 0 &&
                    completedSets % trigger.setInterval == 0)
                {
                    if (completedSets != trigger.lastTriggeredAtSet)
                    {
                        shouldTrigger = true;
                        trigger.lastTriggeredAtSet = completedSets;
                    }
                }
            }
            else
            {
                if (trigger.hasTriggered)
                    continue;

                shouldTrigger = trigger.triggerOnExactSetCount
                    ? completedSets == trigger.setsToTrigger
                    : completedSets >= trigger.setsToTrigger;

                if (shouldTrigger)
                    trigger.hasTriggered = true;
            }

            if (shouldTrigger)
            {
                TriggerEvent(trigger);
            }
        }
    }

    void TriggerEvent(SetTrigger trigger)
    {
        if (trigger == null)
            return;

        if (trigger.pauseGameBeforeTrigger)
        {
            StartCoroutine(PauseSequence(trigger));
        }
        else
        {
            if (trigger.animator != null &&
                !string.IsNullOrEmpty(trigger.triggerName))
            {
                trigger.animator.SetTrigger(trigger.triggerName);
            }

            trigger.onTriggered?.Invoke();
        }
    }

    IEnumerator PauseSequence(SetTrigger trigger)
    {
        float originalTimeScale = Time.timeScale;
        AnimatorUpdateMode originalAnimatorUpdateMode = AnimatorUpdateMode.Normal;

        if (qte != null)
            qte.SetPaused(true);

        Time.timeScale = 0f;

        if (trigger.animator != null)
        {
            // Make sure the animator ignores Time.timeScale.
            originalAnimatorUpdateMode = trigger.animator.updateMode;
            trigger.animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            trigger.animator.SetTrigger(trigger.triggerName);

            if (!string.IsNullOrEmpty(trigger.finalStateName))
            {
                yield return WaitForAnimatorSequence(trigger.animator, trigger.finalStateName);
            }

            trigger.animator.updateMode = originalAnimatorUpdateMode;
        }
        else
        {
            // A code-driven fallback means this works without an Animator or
            // any manually-created fade UI in the scene.
            yield return PlayScreenFade(trigger.fadeDuration, trigger.blackScreenDuration);
        }

        Time.timeScale = originalTimeScale;

        if (qte != null)
            qte.SetPaused(false);

        trigger.onTriggered?.Invoke();
    }

    IEnumerator PlayScreenFade(float fadeDuration, float blackScreenDuration)
    {
        CanvasGroup canvasGroup = EnsureFadeCanvas();
        float duration = Mathf.Max(0.01f, fadeDuration);

        yield return FadeCanvas(canvasGroup, 0f, 1f, duration);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, blackScreenDuration));
        yield return FadeCanvas(canvasGroup, 1f, 0f, duration);
    }

    IEnumerator FadeCanvas(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    CanvasGroup EnsureFadeCanvas()
    {
        if (fadeCanvasGroup != null)
            return fadeCanvasGroup;

        GameObject canvasObject = new GameObject("Cutscene Fade Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        GameObject imageObject = new GameObject("Black Fade", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        fadeCanvasGroup = imageObject.GetComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
        return fadeCanvasGroup;
    }

    IEnumerator WaitForAnimatorSequence(Animator animator, string finalStateName)
    {
        // Allow the trigger to be processed.
        yield return null;

        // Wait until the animator enters the final state.
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(finalStateName))
        {
            yield return null;
        }

        // Wait until the final state finishes.
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f ||
               animator.IsInTransition(0))
        {
            yield return null;
        }
    }

    [System.Serializable]
    public class SetTrigger
    {
        [Header("Trigger Threshold")]
        [Tooltip("The number of completed sets required for this trigger to fire.")]
        public int setsToTrigger = 1;

        [Tooltip("If enabled, only fires when the completed set count exactly matches Sets To Trigger.")]
        public bool triggerOnExactSetCount;

        [Header("Interval Trigger")]
        [Tooltip("If enabled, fires every N completed sets.")]
        public bool triggerOnInterval;

        public int setInterval = 6;

        [Header("Pause")]
        public bool pauseGameBeforeTrigger;

        [Tooltip("Time, in real seconds, for each half of the automatic screen fade.")]
        [Min(0f)] public float fadeDuration = 0.3f;

        [Tooltip("Time, in real seconds, to keep the screen black.")]
        [Min(0f)] public float blackScreenDuration = 0.4f;

        [Header("Animator")]
        public Animator animator;

        [Tooltip("Trigger sent to the Animator.")]
        public string triggerName = "Trigger";

        [Tooltip("Name of the final Animator state before gameplay resumes.")]
        public string finalStateName = "Finished";

        [Header("Events")]
        public UnityEvent onTriggered;

        [HideInInspector]
        public bool hasTriggered;

        [HideInInspector]
        public int lastTriggeredAtSet;
    }
}
