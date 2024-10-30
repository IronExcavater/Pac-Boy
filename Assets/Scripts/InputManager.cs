using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    public static InputManager Input { get; private set; }

    [Header("Load Screen:")]
    public RectTransform loadTransform;
    public static bool isLoading = false;

    private void Awake()
    {
        if (Input == null)
        {
            Input = this;
            DontDestroyOnLoad(Input);
            Initialize();
        }
        else Destroy(this);
    }

    private void Initialize()
    {
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        loadTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
        loadTransform.anchoredPosition = new Vector2(loadTransform.anchoredPosition.x, -Screen.height);
    }

    public void Level1Button()
    {
        if (isLoading) return;
        StartCoroutine(LoadScene("Recreation"));
    }

    public void Level2Button()
    {
        if (isLoading) return;
    }

    private IEnumerator LoadScene(string sceneName)
    {
        isLoading = true;
        var loadRect = loadTransform;
        var tweenIn = AnimationManager.AddTween(loadRect, Vector3.zero, 2);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenIn));
        SceneManager.LoadSceneAsync(sceneName);
        var tweenOut = AnimationManager.AddTween(loadRect, new Vector3(0, -Screen.height), 2);
        yield return new WaitUntil(() => !AnimationManager.TweenExists(tweenOut));
        isLoading = false;
    }
}
