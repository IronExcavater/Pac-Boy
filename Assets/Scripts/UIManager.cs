using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager UI { get; private set; }

    [Header("Load Screen:")]
    public RectTransform loadTransform;
    public static bool isLoading = false;

    private void Awake()
    {
        if (UI == null)
        {
            UI = this;
            DontDestroyOnLoad(UI);
            Initialize();
        }
        else Destroy(gameObject);
    }

    private void Initialize()
    {
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width + 12);
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height + 12);
        loadTransform.anchoredPosition = new Vector2(loadTransform.anchoredPosition.x, -loadTransform.sizeDelta.y);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadScreen(sceneName));
    }

    private static IEnumerator LoadScreen(string sceneName)
    {
        isLoading = true;
        var loadRect = UI.loadTransform;
        var tweenIn = AnimationManager.AddTween(loadRect, Vector3.zero, 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenIn));
        SceneManager.LoadSceneAsync(sceneName);
        var tweenOut = AnimationManager.AddTween(loadRect, new Vector3(0, -Screen.height), 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenOut));
        isLoading = false;
    }
}
