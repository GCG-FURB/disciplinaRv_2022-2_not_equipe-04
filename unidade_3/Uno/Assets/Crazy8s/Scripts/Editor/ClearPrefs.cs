using UnityEditor;
using UnityEngine;

public class ClearPrefs : EditorWindow
{

    [MenuItem("Edit/Reset Playerprefs")]

    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

}