using System.Threading.Tasks;
using UnityEngine;

public class screenFader : MonoBehaviour
{



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static screenFader Instance { get; private set; }
    private Animator animator;
    [SerializeField] private float fadeDuration = 0.5f;//Duration of the fade effect   
    [SerializeField] CanvasGroup canvasGroup; // Reference to the CanvasGroup component for fading

    void Awake()
    {
        // Implementing the singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep the fader across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    async Task fade(float targetTransparency)
    {
        float startTransparency = canvasGroup.alpha;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startTransparency, targetTransparency, elapsedTime / fadeDuration);
            await Task.Yield(); // Wait for the next frame
        }
        canvasGroup.alpha = targetTransparency; // Ensure it ends at the target transparency
    }

    public async Task Fadein()
    {
        await fade(1f); // Fade to black
    }

    public async Task Fadeout() 
    {
        await fade(0f); // Fade to transparent
    }
}

