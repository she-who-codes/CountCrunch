using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SceneDropdown] // Uses your clean dropdown selector!
    [SerializeField] private string gameSceneName;

    private bool isTransitioning = false;

    void Update()
    {
        // Listen for the spacebar press, but block it if already transitioning
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (GameSceneManager.Instance != null && !string.IsNullOrEmpty(gameSceneName))
        {
            isTransitioning = true;
            GameSceneManager.Instance.SwitchScene(gameSceneName);
        }
        else if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Main Menu: No game scene name has been selected in the Inspector!");
        }
        else
        {
            Debug.LogError("Main Menu: GameSceneManager instance not found in this scene!");
        }
    }
}
