using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class mainMenu : MonoBehaviour
{
    void Start()
    {
        musicManager.Instance.PlayMusic("MainMenu");
    }

    public void Play()
    {
        LevelManager.Instance.LoadScene("MainMenu");
        musicManager.Instance.PlayMusic("MainMenu");
    }


}
