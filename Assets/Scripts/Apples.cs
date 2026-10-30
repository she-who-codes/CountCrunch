using UnityEngine;
using System.Collections;

public class Apples : MonoBehaviour
{
    public bool isGoodApple;
    public string assignedButton { get; private set; }
    public int assignedQuadrant { get; private set; }

    [Header("Component Links")]
    [SerializeField] private SpriteRenderer buttonPromptRenderer; // Link the child SpriteRenderer here

    [Header("Apple Artwork")]
    [SerializeField] private Sprite goodAppleSprite;
    [SerializeField] private Sprite badAppleSprite;

    [Header("Button Prompt Artwork")]
    [SerializeField] private Sprite spriteA;
    [SerializeField] private Sprite spriteB;
    [SerializeField] private Sprite spriteX;
    [SerializeField] private Sprite spriteY;

    private AppleGameManager manager;
    private SpriteRenderer baseAppleRenderer;

    private void Awake()
    {
        baseAppleRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(AppleGameManager gameManager, string button, int quadrant, bool isGood, float lifetime)
    {
        manager = gameManager;
        assignedButton = button;
        assignedQuadrant = quadrant;
        isGoodApple = isGood;

        // Apply correct apple texture
        if (baseAppleRenderer != null)
        {
            baseAppleRenderer.sprite = isGood ? goodAppleSprite : badAppleSprite;
            baseAppleRenderer.color = Color.white;
        }

        // Apply correct button graphic overlay
        if (buttonPromptRenderer != null)
        {
            buttonPromptRenderer.sprite = GetButtonSprite(button);
        }

        StartCoroutine(LifetimeRoutine(lifetime));
    }

    private Sprite GetButtonSprite(string buttonName)
    {
        return buttonName switch
        {
            "A" => spriteA,
            "B" => spriteB,
            "X" => spriteX,
            "Y" => spriteY,
            _ => null
        };
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Despawn();
    }

    public void Despawn()
    {
        manager.ReturnResources(assignedButton, assignedQuadrant, this);
        Destroy(gameObject);
    }
}
