using UnityEngine;
using System.Collections;

public enum CardColor { red = 0, blue = 1, green = 2, orange = 3, black = 4 };
public enum CardValue {  One = 10, Two = 20, Three = 30, Four = 40, Five = 50, Six = 60, Seven = 70, Eight = 5000, Nine = 90, Ten = 200, Reverse = 500, Skip = 750, Plus2 = 1250, Plus4 = 2500 };

public enum PlayerType { Player, Bot };
public enum GamePhase { CardsDealing,
    TakeOrDiscard,
    ChangeColor,
    PassOrDiscard,
    PassOrDiscardNextSequencedCard,
    OverbidOrTakePenalties,
    RoundEnded,
    GameEnded
}
public enum GameAction { None, Pass, TakeDiscard, TakeStockPile, Discard, FinishRound, ReportRoundWinner, FinishGame, ShowEmoticon, ZoomCard,
    RedrawConfirm
}

public enum SortingMethod { None, Values, Colors};


public class Enums : MonoBehaviour {
   
}
