using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BotEasy : AIPlayer {

    public BotEasy(Deck deck) : base(deck)
    {
        SayBotDecision("Gram z botem izi");
    }

    public override void Discard()
    {
            cardOnAction = GetRandomCardFromHand();
            ZoomOrDiscardCard();
    }    
}
