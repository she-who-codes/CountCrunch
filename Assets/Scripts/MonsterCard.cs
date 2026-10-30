using UnityEngine;
using System.Collections;

public class MemoryCard3D : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private GameObject matchViz;
    [SerializeField] private AudioSource flipSound;
    [Header("Settings")]
    [SerializeField] private float scaleSpeed = 15f;
    [SerializeField] private float flipDuration = 0.2f;

    private int cardID;
    private bool isFlipped = false;
    private bool isMatched = false;
    private Vector3 baseScale;
    private Vector3 targetScale;

    public int CardID => cardID;
    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;

    private void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        // Smoothly process thumbstick scale shifts
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void Initialize(int id, Sprite frontSprite)
    {
        cardID = id;
        frontRenderer.sprite = frontSprite;

        // Face down initialization
        backRenderer.enabled = true;
        frontRenderer.enabled = false;

        isFlipped = false;
        isMatched = false;
        targetScale = baseScale;
    }

    public void SetSelectionState(bool isSelected)
    {
        //if (isMatched) return;
        // Enlarge slightly relative to its unique base layout scales
        targetScale = isSelected ? baseScale * 1.2f : baseScale;
    }

    public void FlipCard(bool showFront)
    {
        if (isMatched || isFlipped == showFront) return;
        StartCoroutine(FlipRoutine(showFront));
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        isFlipped = showFront;
        float timer = 0f;
        Vector3 currentTarget = targetScale;

        flipSound.Play();
        // Flatten card horizontally (Y-axis spin simulation)
        while (timer < flipDuration / 2f)
        {
            timer += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(currentTarget.x, 0, timer / (flipDuration / 2f)), currentTarget.y, 1f);
            yield return null;
        }

        backRenderer.enabled = !showFront;
        frontRenderer.enabled = showFront;

        // Expand back outwards
        timer = 0f;
        while (timer < flipDuration / 2f)
        {
            timer += Time.deltaTime;
            transform.localScale = new Vector3(Mathf.Lerp(0, currentTarget.x, timer / (flipDuration / 2f)), currentTarget.y, 1f);
            yield return null;
        }
    }

    public void MarkAsMatched()
    {
        isMatched = true;
        targetScale = baseScale;
        matchViz.SetActive(true);
    }
}
