using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Linq;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private AudioSource sirenAudio;
    public AudioSource siren1Audio;
    public AudioSource siren2Audio;
    public AudioSource siren3Audio;
    public AudioSource siren4Audio;
    public AudioSource siren5Audio;
    public AudioSource munch1Audio;
    public AudioSource munch2Audio;
    public AudioSource eatGhostAudio;
    public AudioSource powerPelletAudio;
    public AudioSource pacmanEatenAudio;
    public AudioSource roundStartAudio;
    public AudioSource roundEndAudio;
    private int currentMunch = 0;

    public bool powerMode = false;
    public bool pacmanEaten = false;
    public static bool isBlocked = false;
    public bool roundFinished = false;
    public int powerPelletsRemaining = 4;

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text readyText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text hiScoreText; // Added for Hi Score
    [SerializeField] private GameObject[] livesIndicators;

    public string playerName { get; private set; }

    public int score { get; private set; } = 0;
    public int hiScore { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private int ghostMultiplier = 1;
    private int startingLives = 1;
    private int startingScore = 3000;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        sirenAudio = siren1Audio;
        LoadHiScore();
        NewGame();
    }

    private void Update()
    {
        if (isBlocked)
        {
            return;
        }

        if (roundFinished)
        {
            sirenAudio.Stop();
            powerPelletAudio.Stop();
            return;
        }

        if (!pacmanEaten)
        {
            if (powerMode)
            {
                if (sirenAudio.isPlaying)
                {
                    sirenAudio.Stop();
                }
                if (!powerPelletAudio.isPlaying)
                {
                    powerPelletAudio.Play();
                }
            }
            else
            {
                if (powerPelletAudio.isPlaying)
                {
                    powerPelletAudio.Stop();
                }
                if (!sirenAudio.isPlaying)
                {
                    sirenAudio.Play();
                }
            }
        }
    }

    private void NewGame()
    {
        SetScore(startingScore);
        SetLives(startingLives);
        StartCoroutine(NewRound());

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetSpeed();
        }
    }

    private IEnumerator NewRound()
    {
        powerPelletsRemaining = 4;
        UpdateSirenAudioSource();
        
        isBlocked = true;
        gameOverText.enabled = false;
        readyText.enabled = true;
        roundFinished = false;

        sirenAudio.Pause();

        roundStartAudio.Play();
        yield return new WaitForSeconds(4f);
        
        readyText.enabled = false;
        sirenAudio.UnPause();
        isBlocked = false;

        ResetRound();
    }

    private void ResetRound()
    {
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].IncreaseSpeed();
        }

        EnableAllCharacters(true);

        ResetState();
        ResetPowerMode();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetState();
        }

        pacmanEaten = false;
        pacman.ResetState();
    }

    private void GameOver()
    {
        gameOverText.enabled = true;
        PlayerPrefs.SetInt("LastScore", this.score);

        EnableAllCharacters(false);

        Invoke(nameof(NavigateToHiScore), 3f);
    }

    private void NavigateToHiScore()
    {
        SceneController.instance.LoadHiScoreScene();
    }

    private void SetLives(int lives)
    {
        this.lives = lives;

        // Loop through all indicators and only activate the ones we have lives for.
        for (int i = 0; i < livesIndicators.Length; i++)
        {
            livesIndicators[i].SetActive(i < this.lives);
        }
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');

        if (this.score > this.hiScore)
        {
            SetHiScore(this.score);
        }
    }

    private void SetHiScore(int score)
    {
        this.hiScore = score;
        hiScoreText.text = score.ToString().PadLeft(2, '0');
    }

    private void LoadHiScore()
    {
        string path = Path.Combine(Application.persistentDataPath, "hiscores.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            HiScoreList list = JsonUtility.FromJson<HiScoreList>(json);

            if (list.scores.Length > 0)
            {
                SetHiScore(list.scores[0].score);
            }
        }
    }

    private void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public void PacmanEaten()
    {
        pacmanEaten = true;
        sirenAudio.Stop();
        powerPelletAudio.Stop();

        pacmanEatenAudio.Play();
        pacman.DeathSequence();

        SetLives(lives - 1);

        if (lives > 0)
        {
            Invoke(nameof(ResetState), 3f);
        }
        else
        {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);
        eatGhostAudio.Play();

        ghostMultiplier *= 2;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!powerMode)
        {
            if (currentMunch == 0)
            {
                munch1Audio.Play();
                currentMunch = 1;
            }
            else if (currentMunch == 1)
            {
                munch2Audio.Play();
                currentMunch = 0;
            }
        }

        if (!HasRemainingPellets())
        {
            roundFinished = true;

            sirenAudio.Stop();
            powerPelletAudio.Stop();

            roundEndAudio.Play();

            EnableAllCharacters(false);

            Invoke(nameof(ShowReady), 2f);
            StartCoroutine(NewRound());
        }
    }

    private void EnableAllCharacters(bool enable)
    {
        pacman.gameObject.SetActive(enable);
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(enable);
        }
    }

    private void ShowReady()
    {
        readyText.enabled = true;
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        powerPelletsRemaining--;
        UpdateSirenAudioSource();

        powerMode = true;
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
        CancelInvoke(nameof(ResetPowerMode));
        Invoke(nameof(ResetPowerMode), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateSirenAudioSource()
    {
        if (sirenAudio.isPlaying)
        {
            sirenAudio.Stop();
        }

        sirenAudio = powerPelletsRemaining switch
        {
            3 => siren2Audio,
            2 => siren3Audio,
            1 => siren4Audio,
            0 => siren5Audio,
            _ => siren1Audio,
        };
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private void ResetPowerMode()
    {
        powerMode = false;
    }

}
