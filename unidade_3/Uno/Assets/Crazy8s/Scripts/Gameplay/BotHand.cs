using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotHand : Hand {


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void ShowCard(Transform child)
    {
#if !TEST_MODE
        Card card = child.GetComponent<Card>();
        if (card != null)
        {
            card.isReversed = true;
        }
#else
        base.ShowCard(child);
#endif
    }


   /* protected override int GetTheBestCardPosition(Card card)
    {
        return transform.childCount;
    }*/

    public override void UnlockZone()
    {
        base.LockZone();
    }

    protected override void LockNotSuitableCards()
    {    
    }

    protected override float GetFanAngle()
    {
        return Constants.BOT_FAN_ANGLE;
    }
    

}
