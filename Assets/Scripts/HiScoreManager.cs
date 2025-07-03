using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class HiScoreManager : MonoBehaviour
{
    public Transform scoreListParent;
    public GameObject scoreEntryPrefab;

    public GameObject nameEntryPanel;
    public Text nameEntryText;
    public RectTransform underlineCursor; // The underline/cursor image

    private HiScoreList hiScoreList;
    private string savePath;

    private const int MaxScores = 10;
    private const string DefaultName = "A         ";
    private char[] currentName;
    private int currentNameCharIndex = 0;
    private int newScore;

    private const float cursorPositionAdjustment = 5f;

    private static readonly string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXZYZ0123456789._-";

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "hiscores.json");
        LoadScores();

        // Check if a new high score was passed from the game scene.
        // We use PlayerPrefs as a simple way to pass data between scenes.
        int potentialNewScore = PlayerPrefs.GetInt("LastScore", 0);
        if (potentialNewScore > 0 && IsHiScore(potentialNewScore))
        {
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

    private void DisplayScores(ScoreEntry highlightEntry = null)
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

            // Highlight the new score in white, otherwise use yellow.
            if (highlightEntry != null && hiScoreList.scores[i].playerName == highlightEntry.playerName && hiScoreList.scores[i].score == highlightEntry.score)
            {
                foreach (var text in texts) {
                    text.color = Color.white;
                }
            }
        }

        Invoke(nameof(NavigateToMenu), 5f);
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
        nameEntryPanel.SetActive(true);
        currentName = DefaultName.ToCharArray();
        currentNameCharIndex = 0;
        UpdateNameDisplayText();
    }

    private void HandleNameEntryInput()
    {
        // This method is now handled by OnMove and OnSubmit
    }

    private void OnMove(InputValue value)
    {
        if (!nameEntryPanel.activeSelf) return;

        Vector2 input = value.Get<Vector2>().normalized;

        if (input.x > 0.5f) // Right
        {
            currentNameCharIndex = (currentNameCharIndex + 1) % currentName.Length;
        }
        else if (input.x < -0.5f) // Left
        {
            currentNameCharIndex = (currentNameCharIndex - 1 + currentName.Length) % currentName.Length;
        }
        else if (input.y > 0.5f) // Up
        {
            ChangeCharacter(1);
        }
        else if (input.y < -0.5f) // Down
        {
            ChangeCharacter(-1);
        }

        UpdateNameDisplayText();
    }

    private void OnSubmit()
    {
        if (nameEntryPanel.activeSelf)
        {
            FinalizeAndSaveScore();
        }
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
        // Build the string with a color tag for the selected character.
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

        // Calculate the position for the underline cursor within the local space of the canvas
        if (underlineCursor != null)
        {
            // Calculate the width of a single character in UI units.
            TextGenerator textGen = new TextGenerator();
            TextGenerationSettings generationSettings = nameEntryText.GetGenerationSettings(nameEntryText.rectTransform.rect.size);
            float charWidth = textGen.GetPreferredWidth("A", generationSettings); // Use a character for better accuracy

            // Calculate the starting X position of the text block (its left edge).
            float textBlockStartX = nameEntryText.rectTransform.anchoredPosition.x - (nameEntryText.rectTransform.rect.width / 2f);

            // Calculate the local X position for the cursor.
            float cursorX = textBlockStartX + (charWidth * (currentNameCharIndex + 0.5f)) - cursorPositionAdjustment;

            // Set the underline's local position.
            underlineCursor.anchoredPosition = new Vector2(cursorX, underlineCursor.anchoredPosition.y);
        }
    }

    private void FinalizeAndSaveScore()
    {
        nameEntryPanel.SetActive(false);

        ScoreEntry newEntry = new ScoreEntry(new string(currentName), newScore);

        // Add the new score and sort the list.
        List<ScoreEntry> tempList = hiScoreList.scores.ToList();
        tempList.Add(newEntry);
        hiScoreList.scores = tempList.OrderByDescending(s => s.score).ToArray();

        // Trim the list if it's too long.
        if (hiScoreList.scores.Length > MaxScores)
        {
            hiScoreList.scores = hiScoreList.scores.Take(MaxScores).ToArray();
        }

        SaveScores();
        DisplayScores(newEntry);
    }

    #endregion
}