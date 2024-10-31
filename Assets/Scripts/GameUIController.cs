using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image[] lifeImages;
    [SerializeField] private TextMeshProUGUI scaredCountdownText;
    
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
        
        var elapsedTime = Time.time - GameManager.Game.time;
        var minutes = Mathf.FloorToInt(elapsedTime / 60f);
        var seconds = Mathf.FloorToInt(elapsedTime % 60f);
        var milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        var timeFormatted = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
        timeText.text = timeFormatted;

        for (var i = 0; i < lifeImages.Length; i++) 
            lifeImages[i].enabled = i + 1 <= GameManager.Game.lives;

        var countdownTime = GameManager.Game.scaredLength - (Time.time - GameManager.Game.scaredTime);
        scaredCountdownText.text = countdownTime < 0 ? "" : $"{Mathf.FloorToInt(countdownTime % 60f) + 1}";
    }
}
