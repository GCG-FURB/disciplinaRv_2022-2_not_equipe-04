using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class WindowBehaviour : IHUD
{
    
    protected float animTime = 0.5f;
    protected bool isOpen;

    protected override void Awake()
    {
        base.Awake();
        cg.alpha = 0;
        SwitchCanvasGroup(false);
    }

    private void OnDisable()
    {
        CloseWindow();
    }

    protected virtual void SwitchCanvasGroup(bool state)
    {
        cg.interactable = state;
        cg.blocksRaycasts = state;
    }

    public virtual void ShowWindow()
    {
        if (!isOpen)
        {
            isOpen = true;
            TurnOnScreen();
            SwitchCanvasGroup(true);
            cg.DOKill();
            cg.DOFade(1, animTime);
        }
    }

    public virtual void CloseWindow()
    {
        if (cg != null && isOpen)
        {
            cg.DOKill();
            isOpen = false;
            SwitchCanvasGroup(false);
            cg.DOFade(0, animTime).OnComplete(TurnOffScreen);
        }
    }
}
