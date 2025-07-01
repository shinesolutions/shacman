using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    void Awake()
    {
        Debug.Log("Awake");
    }

    void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        Debug.Log(mode);
    }

    void Start()
    {
        Debug.Log("Start");
    }

    void Update()
    {
        Debug.Log("updating");
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Escape pressed");
            SceneController.instance.LoadGameScene();
        }
    }
}