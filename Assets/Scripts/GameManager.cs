using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver, gameWin;
    public TextMeshProUGUI scoreText, highScoreText;
    public AudioSource winAudio, loseAudio;
    public GameObject pauseButton;

    private int score;

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver.alpha > 0 || gameWin.alpha > 0)
            pauseButton.SetActive(false);
    }

    public void NewGame()
    {
        Time.timeScale = 1f;

        SetScore(0);
        highScoreText.text = LoadHighScore().ToString();

        gameOver.alpha = 0f;
        gameOver.interactable = false;//ngăn ko cho tương tác dc

        gameWin.alpha = 0f;

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;
        StartCoroutine(Fade(gameOver, loseAudio));
    }

    public void GameWin()
    {
        board.enabled = false;
        StartCoroutine(Fade(gameWin, winAudio));
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        board.enabled = false;
    }

    public void Continue()
    {
        Time.timeScale = 1f;
        board.enabled = true;
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, AudioSource audio)
    {
        yield return new WaitForSeconds(1);
        audio.Play();

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        SaveHighScore();
    }

    private void SaveHighScore()
    {
        //int highScore = LoadHighScore();
        if (score > LoadHighScore())
            PlayerPrefs.SetInt("highScore", score);
    }

    private int LoadHighScore() => PlayerPrefs.GetInt("highScore", 0);
}
