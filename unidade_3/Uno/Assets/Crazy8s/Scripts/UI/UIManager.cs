using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public static UIManager instance;
    private List<IHUD> screens = new List<IHUD>();
    private List<GameObject> childrenToTurnOff = new List<GameObject>();

    public void Awake()
    {
        instance = this;
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.activeSelf == false)
                childrenToTurnOff.Add(child);
            child.SetActive(true);
            IHUD screen = child.GetComponent<IHUD>();
            if (screen != null)
                screens.Add(screen);
        }
    }

    public void Start()
    {
        Application.runInBackground = true;
        TurnOffAllScreens();
        for (int i = 0; i < childrenToTurnOff.Count; i++)
        {
            childrenToTurnOff[i].SetActive(false);
        }

    }

    private void TurnOffAllScreens()
    {
        for (int i = 0; i < screens.Count; i++)
        {
            screens[i].SetActive(false);
        }
    }

    public void CloseAllWindows()
    {
        WindowBehaviour[] windows = FindObjectsOfType<WindowBehaviour>();
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].CloseWindow();
        }
    }

    public void BackToMainMenu()
    {
        TurnOffAllScreens();
    }
}
