using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//Called when the slider hits zero so the game will "end" if the player fails.
public class GameOverKO : MonoBehaviour
{
    [Header("QTE")]
    [SerializeField] private Quick_Time_Event qteToWatch;

    [Header("Slider")]
    [SerializeField] private Slider progressSlider;

    [Header("End Screen Fade")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;
    [SerializeField, Min(0f)] private float blackScreenDuration = 0.75f;

    private bool hasEnded;
    private bool wasQteActive = true;
    private bool restartSequenceStarted;
    private CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        if (qteToWatch == null)
        {
            qteToWatch = FindFirstObjectByType<Quick_Time_Event>();
        }
    }

    private void Update()
    {
        if (hasEnded || qteToWatch == null)
            return;

        bool isQteActive = IsQteActive();

        if (wasQteActive && !isQteActive && IsFailureTextVisible())
        {
            EndGame();
        }

        wasQteActive = isQteActive;
    }

    private bool IsQteActive()
    {
        FieldInfo field = typeof(Quick_Time_Event).GetField("qteActive", BindingFlags.Instance | BindingFlags.NonPublic);
        return field != null && (bool)field.GetValue(qteToWatch);
    }

    private bool IsFailureTextVisible()
    {
        return qteToWatch.promptText != null && qteToWatch.promptText.text == "Failed!";
    }

    private void EndGame()
    {
        if (hasEnded)
            return;

        hasEnded = true;
        ReturnToMainCamera();
        Debug.Log("Game over");
        StartCoroutine(FadeAndRestart());
    }

    public void ResetGame()
    {
        if (restartSequenceStarted)
            return;

        ReturnToMainCamera();
        StartCoroutine(FadeAndRestart());
    }

    private static void ReturnToMainCamera()
    {
        MayaSequenceController sequenceController = FindFirstObjectByType<MayaSequenceController>();
        if (sequenceController != null)
            sequenceController.ReturnToMainCamera();
    }

    private IEnumerator FadeAndRestart()
    {
        restartSequenceStarted = true;

        if (qteToWatch != null)
            qteToWatch.SetPaused(true);

        Time.timeScale = 0f;

        CanvasGroup canvasGroup = EnsureFadeCanvas();
        yield return FadeCanvas(canvasGroup, 0f, 1f, Mathf.Max(0.01f, fadeDuration));
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        // Time.timeScale is static, so restore it before loading the scene.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator FadeCanvas(CanvasGroup canvasGroup, float from, float to, float duration)
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

    private CanvasGroup EnsureFadeCanvas()
    {
        if (fadeCanvasGroup != null)
            return fadeCanvasGroup;

        GameObject canvasObject = new GameObject("Game Over Fade Canvas",
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
        return fadeCanvasGroup;
    }
}
