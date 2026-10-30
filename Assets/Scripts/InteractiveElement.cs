using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractiveElement : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject promptUI; // Drag your UI prompt here

    [Header("Action Type")]
    public InteractionType actionType;

    [SceneDropdown]
    public string sceneToLoad;   // Name of the scene for a door
   
    [Header("Mini Game To Load")]
    public MiniGameType miniGameType;

    public GameObject targetObject; // UI or GameObject for mini-game

    private bool playerInRange = false;

    public enum InteractionType
    {
        LoadScene,
        ToggleGameObject
    }

    public enum MiniGameType
    {
        bobbingGame,
        cardMatchGame,
        catapultGame
    }

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            TriggerAction();
        }
    }

    void TriggerAction()
    {
        switch (actionType)
        {
            case InteractionType.LoadScene:
                
                if (!string.IsNullOrEmpty(sceneToLoad))
                {                    
                    // Redirect through our persistent manager instance
                    if (GameSceneManager.Instance != null)
                    {
                        GameSceneManager.Instance.SwitchScene(sceneToLoad);
                    }
                    else
                    {
                        // Fallback in case you run a scene layout without the manager present
                        SceneManager.LoadScene(sceneToLoad);
                    }

                    GameSceneManager.Instance.miniGameType = miniGameType;
                }
                break;

            case InteractionType.ToggleGameObject:
                if (targetObject != null)
                {
                    bool isActive = targetObject.activeSelf;
                    targetObject.SetActive(!isActive);
                }
                break;
        }
    }
}
