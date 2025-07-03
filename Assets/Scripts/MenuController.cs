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

    // This method is called by the PlayerInput component when the "Submit" action is triggered.
    private void OnSubmit()
    {
        SceneController.instance.LoadGameScene();
    }
}