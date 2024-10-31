using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image[] lifeImages;
    [SerializeField] private TextMeshProUGUI scaredCountdownText;
    [SerializeField] private Image startCountdownImage;
    [SerializeField] private TextMeshProUGUI startCountdownText;
    
    public void Level1Button()
    {
        LoadManager.Load.LoadScene("Recreation");
    }

    public void Level2Button()
    {
        
    }

    public void ExitButton()
    {
        LoadManager.Load.LoadScene("StartScene");
    }

    public void Update()
    {
        if (LoadManager.isLoading) return;
        scoreText.text = GameManager.Game.score.ToString();
        
        var elapsedTime = GameManager.ElapsedTime();
        var minutes = Mathf.FloorToInt(elapsedTime / 60f);
        var seconds = Mathf.FloorToInt(elapsedTime % 60f);
        var milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        var timeFormatted = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        timeText.text = timeFormatted;

        for (var i = 0; i < lifeImages.Length; i++) 
            lifeImages[i].enabled = i + 1 <= GameManager.Game.lives;

        var scaredCountdown = GameManager.Game.scaredLength - (Time.time - GameManager.Game.scaredTime);
        scaredCountdownText.enabled = scaredCountdown > 0;
        var scaredSeconds = Mathf.FloorToInt(scaredCountdown % 60f) + 1;
        scaredCountdownText.text = scaredSeconds.ToString();
        
        // TODO: Broken countdown
        var startCountdown = GameManager.Game.countdownLength - (Time.time - GameManager.Game.countdownTime);
        startCountdownImage.enabled = startCountdown > 0;
        startCountdownText.enabled = startCountdown > 0;
        var startSeconds = Mathf.FloorToInt(startCountdown % 60f);
        startCountdownText.text = (startSeconds - 1).ToString();
    }
}
