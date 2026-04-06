using UnityEngine;

public class initiateLvl1Music : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicManager.Instance.PlayMusic("hardStyle");
        FadeTransition();

    }

    async void FadeTransition()
    {

    await screenFader.Instance.Fadeout();

    }



}
