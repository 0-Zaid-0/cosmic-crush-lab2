using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Score UI and win/lose states.
/// Use AddScore, PlayerDied, PlayerWon from absorb logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] Text scoreText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] Text gameOverScoreText;
    [SerializeField] Text winScoreText;

    [Header("Debug hotkeys")]
    [SerializeField] bool enableTestHotkeys = true;

    float score;
    public bool IsGameOver { get; private set; }
    public float Score => score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsGameOver = false;
        Time.timeScale = 1f;
        SetScore(0f);
        HideEndPanels();
    }

    void Update()
    {
        if (!enableTestHotkeys)
            return;

        // J = +score, L = lose, K = win, R = restart after game over
        if (Input.GetKeyDown(KeyCode.L))
            PlayerDied();
        if (Input.GetKeyDown(KeyCode.K))
            PlayerWon();
        if (Input.GetKeyDown(KeyCode.J))
            AddScore(10f);
        if (Input.GetKeyDown(KeyCode.R) && IsGameOver)
            RestartGame();
    }

    public void AddScore(float amount)
    {
        if (IsGameOver || amount <= 0f)
            return;

        SetScore(score + amount);
    }

    public void PlayerDied()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        Time.timeScale = 0f;

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score: {score:0}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void PlayerWon()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;
        Time.timeScale = 0f;

        if (winScoreText != null)
            winScoreText.text = $"Score: {score:0}";

        if (winPanel != null)
            winPanel.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void SetScore(float value)
    {
        score = value;
        if (scoreText != null)
            scoreText.text = $"Score: {score:0}";
    }

    void HideEndPanels()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    // Wired from UI Buttons in the scene setup
    public void OnRestartButton() => RestartGame();
}
