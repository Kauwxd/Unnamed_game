using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenuButtons : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  public void PlayButton()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OptionsButton()
    {
        //Muligvis ikke nødvendig SceneManager.LoadScene("Options");
    }

    public void QuitButton()
    {
        Application.Quit();
    }

}
