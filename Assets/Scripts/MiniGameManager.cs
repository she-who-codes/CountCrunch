using UnityEngine;
using static InteractiveElement;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }



    public GameObject bobbingGame;
    public GameObject cardMatchGame;
    public GameObject catapultGame;

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

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (GameSceneManager.Instance.miniGameType)
        {
            case MiniGameType.bobbingGame:
                bobbingGame.SetActive(true);
                break;
            case MiniGameType.cardMatchGame:
                cardMatchGame.SetActive(true);
                break;
            case MiniGameType.catapultGame:
                catapultGame.SetActive(true);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
