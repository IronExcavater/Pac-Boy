using System.Collections;
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
    [SerializeField] private Image phaseImage;

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
        {
            if (i + 1 <= GameManager.Game.lives != lifeImages[i].enabled)
                StartCoroutine(PopLife(lifeImages[i]));
        }

        var scaredCountdown = GameManager.Game.scaredLength - (Time.time - GameManager.Game.scaredTime);
        scaredCountdown = GameManager.Game.scaredTime == 0 ? 0 : scaredCountdown;
        scaredCountdownText.enabled = scaredCountdown > 0;
        var scaredSeconds = Mathf.FloorToInt(scaredCountdown % 60f) + 1;
        scaredCountdownText.text = scaredSeconds.ToString();
        
        var startCountdown = GameManager.Game.countdownLength - (Time.time - GameManager.Game.countdownTime);
        var startSeconds = Mathf.FloorToInt(startCountdown % 60f);
        var startSecondsText = startSeconds == 0 ? "GO" : startSeconds.ToString();
        if (startCountdownObject.activeInHierarchy && !startCountdownText.text.Equals(startSecondsText)) 
            AudioManager.PlaySfxOneShot(startSecondsText == "GO" ? AudioManager.Audio.select : AudioManager.Audio.coin);
        startCountdownText.text = startSecondsText;
        
        var canPhase = GameManager.PlayerCanPhase();
        phaseImage.color = new Color(1, 1, 1, canPhase ? 1 : 0.3f);
    }

    public void ShowCountdown() { startCountdownObject.SetActive(true); }
    public void HideCountdown() { startCountdownObject.SetActive(false); }
    
    public void ShowGameOver() { gameOverObject.SetActive(true); }
    public void HideGameOver() { gameOverObject.SetActive(false); }

    public IEnumerator PopLife(Image lifeImage)
    {
        var lifeAni = lifeImage.GetComponent<Animator>();
        lifeAni.SetTrigger("Pop");
        yield return new WaitForSeconds(2);
        lifeImage.enabled = false;
    }
}
