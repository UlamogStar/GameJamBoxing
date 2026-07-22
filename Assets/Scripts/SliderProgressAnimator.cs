using UnityEngine;
using UnityEngine.UI;

public class SliderProgressAnimator : MonoBehaviour
{
    public Slider slider;
    public Animator animator;
    public string progressParam = "Progress";

    void Update()
    {
        if (slider == null || animator == null)
            return;

        float normalizedProgress = Mathf.Clamp01(slider.value / Mathf.Max(slider.maxValue, 0.0001f));
        animator.SetFloat(progressParam, normalizedProgress);
    }
}