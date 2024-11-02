using System;
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
        LoadManager.LoadScene("InnovationScene");
    }

    public void MusicButton()
    {
        var newMusicOption = (LoadManager.LoadMusicOption() + 1) % 4;
        Debug.Log(newMusicOption);
        AudioManager.PlayMusicLoopNow(GetMenuMusic(newMusicOption));
        LoadManager.SaveMusicOption(newMusicOption);
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
        AudioManager.PlayMusicLoopNextBar(GetMenuMusic(LoadManager.LoadMusicOption()));
        LoadManager.SaveHighScore(10, 1000);
        highScoreText.text = $"{LoadManager.LoadHighScore()}, {FormattedTime(LoadManager.LoadBestTime())}";
    }

    private static AudioClipTempoTuple GetMenuMusic(int musicOption) => musicOption switch
    {
        0 => AudioManager.Audio.musicIntro,
        1 => AudioManager.Audio.musicMenu,
        2 => AudioManager.Audio.musicIntermission,
        3 => AudioManager.Audio.musicGhost,
        _ => AudioManager.Audio.musicIntro
    };
}
