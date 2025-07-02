using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class HiScoreManager : MonoBehaviour
{
    public Transform scoreListParent;
    public GameObject scoreEntryPrefab;

    public GameObject nameEntryPanel;
    public Text nameEntryText;

    private HiScoreList hiScoreList;
    private string savePath;

    private const int MaxScores = 10;
    private const string DefaultName = "A         ";
    private char[] currentName;
    private int currentNameCharIndex = 0;
    private int newScore;

    private static readonly string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXZYZ0123456789_-";

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "hiscores.json");
        LoadScores();

        // Check if a new high score was passed from the game scene.
        // We use PlayerPrefs as a simple way to pass data between scenes.
        int potentialNewScore = PlayerPrefs.GetInt("LastScore", 0);
        Debug.Log(potentialNewScore.ToString());
        if (potentialNewScore > 0 && IsHiScore(potentialNewScore))
        {
            Debug.Log("PotentialNewScore Qualifies");
            newScore = potentialNewScore;
            PlayerPrefs.SetInt("LastScore", 0); // Clear it so we don't trigger again
            StartNameEntry();
        }
        else
        {
            DisplayScores();
            nameEntryPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Only process input if the name entry panel is active.
        if (nameEntryPanel.activeSelf)
        {
            HandleNameEntryInput();
        }
    }

    #region Data Handling (Load/Save)

    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            hiScoreList = JsonUtility.FromJson<HiScoreList>(json);

            // Sort the list just in case it wasn't saved correctly.
            hiScoreList.scores = hiScoreList.scores.OrderByDescending(s => s.score).ToArray();
        }
        else
        {
            // Create a default list if no save file exists.
            hiScoreList = new HiScoreList { scores = new ScoreEntry[0] };
            CreateDefaultScores();
        }
    }

    private void SaveScores()
    {
        string json = JsonUtility.ToJson(hiScoreList, true);
        File.WriteAllText(savePath, json);
    }

    private void CreateDefaultScores()
    {
        List<ScoreEntry> defaultScores = new List<ScoreEntry>();

        for (int i = 0; i < MaxScores; i++)
        {
            defaultScores.Add(new ScoreEntry("CPU", (MaxScores - i) * 1000));
        }

        hiScoreList.scores = defaultScores.ToArray();
        SaveScores();
    }

    #endregion

    #region Score Display

    private void DisplayScores()
    {
        // Clear any existing score entries in the UI.
        foreach (Transform child in scoreListParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < hiScoreList.scores.Length; i++)
        {
            GameObject entryGO = Instantiate(scoreEntryPrefab, scoreListParent);
            Text[] texts = entryGO.GetComponentsInChildren<Text>();
            texts[0].text = (i + 1).ToString("D2"); // Rank
            texts[1].text = hiScoreList.scores[i].playerName; // Name
            texts[2].text = hiScoreList.scores[i].score.ToString(); // Score
        }

        Invoke("NavigateToMenu", 5f);
    }

    private void NavigateToMenu()
    {
        SceneController.instance.LoadMenuScene();
    }

    #endregion

    #region Name Entry Logic

    private bool IsHiScore(int score)
    {
        // A score qualifies if the list has space, or if the score is higher than the lowest score.
        return hiScoreList.scores.Length < MaxScores || score > hiScoreList.scores.Last().score;
    }

    private void StartNameEntry()
    {
        Debug.Log("StartingNameEntry");
        nameEntryPanel.SetActive(true);
        currentName = DefaultName.ToCharArray();
        currentNameCharIndex = 0;
        UpdateNameDisplayText();
    }

    private void HandleNameEntryInput()
    {
        // Move to the next character
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentNameCharIndex = (currentNameCharIndex + 1) % currentName.Length;
        }
        // Move to the previous character
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentNameCharIndex = (currentNameCharIndex - 1 + currentName.Length) % currentName.Length;
        }
        // Decrement the current character
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeCharacter(-1);
        }
        // Increment the current character
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeCharacter(1);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            FinalizeAndSaveScore();
        }

        UpdateNameDisplayText();
    }

    private void ChangeCharacter(int direction)
    {
        char currentChar = currentName[currentNameCharIndex];
        int alphabetIndex = Alphabet.IndexOf(currentChar);

        alphabetIndex = (alphabetIndex + direction + Alphabet.Length) % Alphabet.Length;
        currentName[currentNameCharIndex] = Alphabet[alphabetIndex];
    }

    private void UpdateNameDisplayText()
    {
        // Add an underline or highlight to show which character is selected
        string displayName = "";

        for (int i = 0; i < currentName.Length; i++)
        {
            if (i == currentNameCharIndex)
            {
                displayName += "<color=white>" + currentName[i] + "</color>";
            }
            else
            {
                displayName += currentName[i];
            }
        }

        nameEntryText.text = displayName;
    }

    private void FinalizeAndSaveScore()
    {
        nameEntryPanel.SetActive(false);

        // Add the new score and sort the list.
        List<ScoreEntry> tempList = hiScoreList.scores.ToList();
        tempList.Add(new ScoreEntry(new string(currentName), newScore));
        hiScoreList.scores = tempList.OrderByDescending(s => s.score).ToArray();

        // Trim the list if it's too long.
        if (hiScoreList.scores.Length > MaxScores)
        {
            hiScoreList.scores = hiScoreList.scores.Take(MaxScores).ToArray();
        }

        SaveScores();
        DisplayScores();
    }

    #endregion
}