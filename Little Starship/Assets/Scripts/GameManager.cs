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
    public Button reloadButton;

    private void Awake()
    {
        playerReference = GameObject.FindGameObjectWithTag("Player");
        playerLocationReference = playerReference.transform;
        playerHealth = playerReference.GetComponent<PlayerHealth>();
        currentScene = SceneManager.GetActiveScene();
    }

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
        if (playerHealth.isAlive)
        {
            reloadButton.gameObject.SetActive(false);
        }
        else
        {
            reloadButton.gameObject.SetActive(true);
        }


    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(currentScene.name);
    }
}
