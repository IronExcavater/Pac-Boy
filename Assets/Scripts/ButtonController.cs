using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void Level1Button()
    {
        UIManager.UI.LoadScene("Recreation");
    }

    public void Level2Button()
    {
        
    }

    public void ExitButton()
    {
        var activeScene = SceneManager.GetActiveScene();
        switch (activeScene.name)
        {
            case "StartScene":
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
            default:
                UIManager.UI.LoadScene("StartScene");
                break;
        }
    }
}
