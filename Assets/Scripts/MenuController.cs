using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Text hiScoreText;

    void Start()
    {
        // Load and display the high score when the menu starts.
        if (hiScoreText != null)
        {
            hiScoreText.text = HiScoreUtil.GetTopScore().ToString();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneController.instance.LoadGameScene();
        }
    }
}