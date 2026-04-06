using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using UnityEngine.InputSystem;

public class pausePanelScript : MonoBehaviour
{
    [SerializeField] GameObject pauseOverlay; // Reference to the pause panel UI
    [SerializeField] GameObject healthBar; // Reference to the health bar UI
    [SerializeField] PlayerMovement playerMovement;
 

    public void Pause()
    {
      
        pauseOverlay.SetActive(true); // Show the pause panel
        healthBar.SetActive(false); // Hide the health bar
        playerMovement.enabled = false;
        Time.timeScale = 0f; // Pause the game
    }

    public void Resume()
    {
        Time.timeScale = 1f; // Resume the game
        playerMovement.enabled = true;
        pauseOverlay.SetActive(false);
        healthBar.SetActive(true); // Show the health bar
    }


    public void QuitToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Load the main menu scene
        Time.timeScale = 1f; // Resume the game
        playerMovement.enabled = true;
    }

    public void Restart()
    {
      
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        Time.timeScale = 1f; // Resume the game
        playerMovement.enabled = true;
        healthBar.SetActive(true); // Show the health bar
    }


}
