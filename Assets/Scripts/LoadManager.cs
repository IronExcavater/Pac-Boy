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
        var loadRect = Load.loadTransform;
        var tweenIn = AnimationManager.AddTween(loadRect, Vector3.zero, 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenIn));
        SceneManager.LoadSceneAsync(sceneName);
        var tweenOut = AnimationManager.AddTween(loadRect, new Vector3(0, -Screen.height), 2, AnimationManager.Easing.EaseInCubic);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenOut));
        isLoading = false;
    }
}
