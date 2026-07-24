using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverKO : MonoBehaviour
{
    [Header("QTE")]
    [SerializeField] private Quick_Time_Event qteToWatch;

    private bool hasEnded;
    private bool wasQteActive = true;

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
        Debug.Log("Game over");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }
}
