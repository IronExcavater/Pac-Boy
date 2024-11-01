using TMPro;
using UnityEngine;

public class StartUIController : UIController
{
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    public void Level1Button()
    {
        LoadManager.LoadScene("Recreation");
    }

    public void Level2Button()
    {
        
    }

    public void ExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void Start()
    {
        LoadManager.SaveHighScore(10, 1000);
        highScoreText.text = $"{LoadManager.LoadHighScore()}, {FormattedTime(LoadManager.LoadBestTime())}";
    }
}
