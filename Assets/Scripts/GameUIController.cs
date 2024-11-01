using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : UIController
{
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image[] lifeImages;
    [SerializeField] private TextMeshProUGUI scaredCountdownText;
    [SerializeField] private GameObject startCountdownObject;
    [SerializeField] private TextMeshProUGUI startCountdownText;
    [SerializeField] private GameObject gameOverObject;

    public void ExitButton()
    {
        LoadManager.LoadScene("StartScene");
    }

    public void Update()
    {
        if (LoadManager.isLoading) return;
        scoreText.text = GameManager.Game.score.ToString();
        
        timeText.text = FormattedTime(GameManager.Game.time);

        for (var i = 0; i < lifeImages.Length; i++) 
            lifeImages[i].enabled = i + 1 <= GameManager.Game.lives;

        var scaredCountdown = GameManager.Game.scaredLength - (Time.time - GameManager.Game.scaredTime);
        scaredCountdown = GameManager.Game.scaredTime == 0 ? 0 : scaredCountdown;
        scaredCountdownText.enabled = scaredCountdown > 0;
        var scaredSeconds = Mathf.FloorToInt(scaredCountdown % 60f) + 1;
        scaredCountdownText.text = scaredSeconds.ToString();
        
        var startCountdown = GameManager.Game.countdownLength - (Time.time - GameManager.Game.countdownTime);
        var startSeconds = Mathf.FloorToInt(startCountdown % 60f);
        startCountdownText.text = startSeconds == 0 ? "GO" : startSeconds.ToString();
    }

    public void ShowCountdown() { startCountdownObject.SetActive(true); }
    public void HideCountdown() { startCountdownObject.SetActive(false); }
    
    public void ShowGameOver() { gameOverObject.SetActive(true); }
    public void HideGameOver() { gameOverObject.SetActive(false); }
}
