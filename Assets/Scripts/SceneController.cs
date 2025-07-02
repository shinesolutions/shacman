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
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Pacman", LoadSceneMode.Single);
    }

    public void LoadHiScoreScene()
    {
        SceneManager.LoadScene("HiScore", LoadSceneMode.Single);
    }
}
