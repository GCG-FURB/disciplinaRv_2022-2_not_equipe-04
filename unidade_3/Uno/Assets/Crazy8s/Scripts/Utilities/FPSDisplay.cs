using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Shows FPS meter
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    float nextRefresh = 0;
    Text txt;

    void Awake()
    {
        txt = GetComponent<Text>();
#if !TEST_MODE
    //    gameObject.SetActive(false);
#endif
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if(Time.unscaledTime > nextRefresh)
        {
            nextRefresh += 1;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            txt.text = text;
        }
    }

 
}