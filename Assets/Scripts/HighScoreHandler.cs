using System.Collections.Generic;
using UnityEngine;

public class HighScoreHandler : MonoBehaviour
{
    List<HighScoreElement> highScoreList = new List<HighScoreElement>();
    [SerializeField] int maxLength = 10;
    [SerializeField] string fileName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.instance.LoadMenuScene();
        }
    }

    private void LoadHighScores()
    {
        highScoreList = FileHandler.ReadFromJSON<HighScoreElement>(fileName);

        while (highScoreList.Count > maxLength)
        {
            highScoreList.RemoveAt(maxLength);
        }
    }

    private void SaveHighScore()
    {
        FileHandler.SaveToJSON<HighScoreElement>(highScoreList, fileName);
    }

    public void AddHighScoreIfPossible(HighScoreElement element)
    {
        for (int i = 0; i < maxLength; i++)
        {
            if (i >= highScoreList.Count || element.score > highScoreList[i].score)
            {
                // Add new high score
                highScoreList.Insert(i, element);

                while (highScoreList.Count > maxLength)
                {
                    highScoreList.RemoveAt(maxLength);
                }

                SaveHighScore();

                break;
            }
        }
    }
}
