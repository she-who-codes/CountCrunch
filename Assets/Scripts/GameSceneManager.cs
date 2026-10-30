using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using static InteractiveElement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("UI Fade Elements")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Audio Elements")]
    [SerializeField] private AudioSource musicSource;

    [Header("Mini Game To Load")]
    public MiniGameType miniGameType;

    private void Awake()
    {
        // Enforce the Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps this manager alive forever

        // Ensure the fade overlay blocks everything at start if needed, or starts completely clear
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void SwitchScene(string sceneName)
    {
        StartCoroutine(TransitionSequence(sceneName));
    }

    private IEnumerator TransitionSequence(string sceneName)
    {
        // 1. Block player inputs by turning on raycast blocking on the persistent UI canvas
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.blocksRaycasts = true;

        // 2. Fade Out Old Scene (Screen goes black, music dips slightly if desired)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = progress;

            yield return null;
        }

        // 3. Begin Asynchronous Loading of the new level scene asset
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait completely until the level data is fully populated in memory
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 4. Fade In New Scene (Screen goes transparent, music retains full volume)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = 1f - progress;

            yield return null;
        }

        // 5. Unblock interactions now that the scene transition is safely complete
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}
