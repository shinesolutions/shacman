using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public AudioSource sirenAudio;
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

    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text readyText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    public string playerName { get; private set; }

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private int ghostMultiplier = 1;

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

        // Update this to change screen to home screen
        if (lives <= 0 && Input.GetKeyDown(KeyCode.Escape))
        {
            // check if hi score and change to hi score screen
            NewGame();
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
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        isBlocked = true;
        gameOverText.enabled = false;
        readyText.enabled = true;
        roundFinished = false;

        sirenAudio.Pause();

        // RoundStartAction();
        roundStartAudio.Play();
        
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

        EnableAllCharacters(true);

        ResetState();
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

        EnableAllCharacters(false);

        // Game Over State
        // Move to Highscore UI Panel
        // Call HighScoreHandler.AddHighScoreIfPossible(new HighScoreElement(playerName, score))

    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    private void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }

    public void PacmanEaten()
    {
        pacmanEaten = true;
        sirenAudio.Stop();
        // This should never happen as PacMan should not be able to die in this mode.
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

        ghostMultiplier++;
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
            pacman.gameObject.SetActive(false);

            sirenAudio.Stop();
            powerPelletAudio.Stop();

            roundEndAudio.Play();

            EnableAllCharacters(false);

            Invoke(nameof(ShowReady), 2f);
            Invoke(nameof(NewRound), 5.3f);
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

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

    private void ResetPowerMode()
    {
        powerMode = false;
    }

}
