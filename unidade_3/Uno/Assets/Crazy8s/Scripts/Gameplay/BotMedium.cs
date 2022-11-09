using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BotMedium : AIPlayer {

    public BotMedium(Deck deck) : base(deck)
    {
        SayBotDecision("Gram z botem medium");

    }
    
}
