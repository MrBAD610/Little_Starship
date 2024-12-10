using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerReference { get; private set; }
    public Transform playerLocationReference { get; private set; }
    private PlayerHealth playerHealth;
    public Scene currentScene { get; private set; }
    public Button gameOverReloadButton;
    public Button gameWinReloadButton;
    public Button quitButton;

    private void Awake()
    {
        playerReference = GameObject.FindGameObjectWithTag("Player");
        playerLocationReference = playerReference.transform;
        playerHealth = playerReference.GetComponent<PlayerHealth>();
        currentScene = SceneManager.GetActiveScene();
        gameOverReloadButton.gameObject.SetActive(false);
        gameWinReloadButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerHealth.isAlive)
        {
            gameOverReloadButton.gameObject.SetActive(false);
        }
        else
        {
            gameOverReloadButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
        }
    }

    public void ReloadScene()
    {
        Time.timeScale = 1; // Resume the game
        SceneManager.LoadScene(currentScene.name);
    }

    public int GetTotalColonists()
    {
        GameObject[] colonists = GameObject.FindGameObjectsWithTag("Colonist");
        return colonists.Length;
    }

    public void WinState()
    {
        if (GetTotalColonists() == 0)
        {
            gameWinReloadButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
            Time.timeScale = 0; // Pause the game
        }
        else
        {
            gameWinReloadButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
            Debug.LogError("Not all colonists have been saved.");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1; // Resume the game
        Application.Quit();
    }
}
