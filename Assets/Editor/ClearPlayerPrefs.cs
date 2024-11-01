using UnityEngine;
using UnityEditor;

public class ClearPlayerPrefsEditor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared!");
    }
}
