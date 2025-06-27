using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartController : MonoBehaviour
{
    [SerializeField] private Text readyText;
    public float displayTime = 3f;
    public static bool GameIsReady { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        readyText.gameObject.SetActive(true);
        GameIsReady = false;
        StartCoroutine(ShowReadyMessage());
    }

    IEnumerator ShowReadyMessage()
    {
        Debug.Log("Countdown Started.");
        yield return new WaitForSeconds(displayTime);
        readyText.gameObject.SetActive(false);

        GameIsReady = true;
        Debug.Log("Countdown Finished.");
    }
}
