using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Load { get; private set; }

    [Header("Load Screen:")]
    public RectTransform loadTransform;
    public static bool isLoading;

    private void Awake()
    {
        if (Load == null)
        {
            Load = this;
            DontDestroyOnLoad(Load);
        }
        else Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
        loadTransform.anchoredPosition = new Vector2(loadTransform.anchoredPosition.x, -loadTransform.sizeDelta.y);
    }

    public static void LoadScene(string sceneName)
    {
        if (isLoading) return;
        Load.StartCoroutine(LoadScreen(sceneName));
    }

    private static IEnumerator LoadScreen(string sceneName)
    {
        isLoading = true;
        var loadRect = Load.loadTransform;
        var tweenIn = AnimationManager.AddTween(loadRect, Vector3.zero, 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenIn));
        SceneManager.LoadSceneAsync(sceneName);
        var tweenOut = AnimationManager.AddTween(loadRect, new Vector3(0, -Screen.height), 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenOut));
        isLoading = false;
    }

    public static void SaveHighScore(float time, int score)
    {
        var prevScore = LoadHighScore();
        if (score > prevScore || score >= prevScore && time < LoadBestTime())
        {
            PlayerPrefs.SetInt("score", score);
            PlayerPrefs.SetFloat("time", time);
        }
    }
    
    public static void SaveMusicOption(int musicOption) { PlayerPrefs.SetInt("musicOption", musicOption); }

    public static float LoadBestTime() { return PlayerPrefs.GetFloat("time", 0); }
    public static int LoadHighScore() { return PlayerPrefs.GetInt("score", 0); }
    public static int LoadMusicOption() { return PlayerPrefs.GetInt("musicOption", 0); }
}
