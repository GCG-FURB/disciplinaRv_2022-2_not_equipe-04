using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BotHard : BotMedium {

    public BotHard(Deck deck) : base(deck)
    {
        SayBotDecision("Gram z botem hard");
    }

  
}
