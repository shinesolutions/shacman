using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        instance.LoadMenuScene();
    }

    public void LoadMenuScene()
    {
        Debug.Log("Loading menu scene");
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        Debug.Log("Loading pacman scene");
        SceneManager.LoadScene("Pacman", LoadSceneMode.Single);
    }

    public void LoadHiScoreScene()
    {
        Debug.Log("Loading hiscore scene");
        SceneManager.LoadScene("HiScore", LoadSceneMode.Single);
    }
}
