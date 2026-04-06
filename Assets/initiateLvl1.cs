using UnityEngine;

public class initiateLvl1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicManager.Instance.PlayMusic("hardStyle");
        FadeTransition();
        Debug.Log(screenFader.Instance);
    }

    async void FadeTransition()
    {

    await screenFader.Instance.Fadeout();

    }



}
