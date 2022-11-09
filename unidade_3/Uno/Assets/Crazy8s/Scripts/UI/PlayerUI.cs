using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    public Image playerAvatar;
    public Text remainingCardsText;
    public Text playerName;
    private TurnTimer timer;
    private Image UIPanel;
    private CanvasGroup cg;

    void Awake()
    {
        UIPanel = GetComponent<Image>();
        cg = GetComponent<CanvasGroup>();
        timer = GetComponentInChildren<TurnTimer>();
        RefreshRemainingCardsText(0);
    }

    public void SetVisibility(bool state)
    {
        if (state)
            cg.ShowCG(Constants.QUICK_ANIM_TIME);
        else
            cg.HideCG(Constants.QUICK_ANIM_TIME);
    }


    public void RefreshRemainingCardsText(int amount)
    {
        if(remainingCardsText != null)
            remainingCardsText.text = amount.ToString();
    }

    public void RefreshPlayerName(string nickname)
    {
        if(playerName!= null)
        {
            playerName.text = nickname;
        }
    }

    public void ResetUI(Action timerAction)
    {
        if(timer == null)
            timer = GetComponentInChildren<TurnTimer>();
        timer.OnTimerFinishedCB = timerAction;
        RefreshRemainingCardsText(0);
    }


    public void RefreshAvatar(int avatarID)
    {
        playerAvatar.sprite = AvatarLoader.LoadAvatar(avatarID);
    }

    public void StartTimer()
    {
        timer.StartTimer();
    }

    public void StopTimer()
    {
        timer.StopTimer();
    }

    public bool TimeIsOver()
    {
       return timer.TimeIsOver();
    }

    public void ResetTimer()
    {
        timer.ResetTimer();
    }
}
