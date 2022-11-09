using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BotPlayer : Player {

    public BotPlayer(Deck deck) : base(deck)
    {
    }
    

    protected void SayBotDecision(string decision)
    {
#if TEST_MODE
        Debug.Log(decision);
#endif
    }
}
