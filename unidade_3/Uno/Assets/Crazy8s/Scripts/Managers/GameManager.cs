using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Opponent data model
/// </summary>
public class GamePlayerData
{
    public string opponentID { get; private set; }
    public string opponentName { get; set; }
    public int opponentAvatarID { get; set; }
    public int opponentLevel { get; private set; }
    public int opponentTurnOrder { get; private set; }
    public bool opponentIsBot { get; private set; }

    public GamePlayerData(string opponentID, string opponentName, int opponentAvatarID, int opponentLevel, int opponentTurnOrder, bool opponentIsBot)
    {
        this.opponentID = opponentID;
        this.opponentName = opponentName;
        this.opponentAvatarID = opponentAvatarID;
        this.opponentLevel = opponentLevel;
        this.opponentTurnOrder = opponentTurnOrder;
        this.opponentIsBot = opponentIsBot;
    }

}

/// <summary>
/// Game data model
/// </summary>
public class GameData
{
    public int gameRandomSeed { get; private set; }
    public int myTurnOrder { get; private set; }
    public GamePlayerData[] opponents { get; private set; }


    public GameData(int gameRandomSeed, int myTurnOrder, GamePlayerData[] opponents)
    {
        this.gameRandomSeed = gameRandomSeed;
        this.myTurnOrder = myTurnOrder;
        this.opponents = opponents;
    }

    
    public void IncreaseSeed()
    {
        gameRandomSeed += Constants.SEED_INCREASE_STEP;
    }
    
}

public class GameManager : MonoBehaviour {

    public Action OnGamePhaseChangeCallback;

    private GamePhase _gamePhase;
    public GamePhase gamePhase {
        get { return _gamePhase; }
        private set
        {
            Debug.Log("Następna tura: " + value.ToString());
            _gamePhase = value;
            passBtn.SetActive(IsValidTimeToPass());
            takeBtn.SetActive(IsValidTimeToTakePenalties());
            OnGamePhaseChangeCallback.RunAction();
        }
    }

    

    public Player currentPlayer    { get;  private set; }
    public static Action OnGameFinishedCB;

 

    public static GameManager instance;
    public GameObject passBtn;
    public GameObject takeBtn;

    [Header("Cards hands")]
    public Hand[] playersHands;


    private List<Player> playerList = new List<Player>();
    private Deck deck;
    private DiscardPile discardPile;
    private Player thisGamePlayer;

    private int indexOfCurrentPlayer;

    private int _playerTurnDirection;

   
    protected int playerTurnDirection
    {
        get
        {
            return _playerTurnDirection;
        }
        private set
        {
            _playerTurnDirection = value;
            turnOrderIndicator.SetLocalScaleY(_playerTurnDirection);
        }
    }
    private Transform turnOrderIndicator;
    private int penaltyCardsToTake;

    private Queue<GamePhase> turnPhases = new Queue<GamePhase>();
    public GameData gameData { get; private set; }
    private Bid bid;

    private List<IResettable> resettables = new List<IResettable>();


    public void RegisterResettable(IResettable resettable)
    {
        resettables.AddUniqueValue(resettable);            
    }

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        instance = this;
        deck = FindObjectOfType<Deck>();
        turnOrderIndicator = GameObject.FindGameObjectWithTag("TurnOrderIndicator").transform;
        discardPile = FindObjectOfType<DiscardPile>();
        discardPile.OnCardPutOnDiscardPile += CheckForCardSpecialPower;
        deck.OnCardDealtCB += StartGame;
    }


    ///TEST FUNCTIONS

    public void StartTwoPlayersGame()
    {
        GamePlayerData[] opponents = {new GamePlayerData("0","janusz",0,1,1,true)};

        int seed = Randomizer.GetRandomNumber(0, 10000);// 1000;
        GameData gameData = new GameData(seed, 0, opponents);
        StartNewGame(new Bid(1, 2, 3, 4, 5),gameData);
    }

  
    public void StartThreePlayersGame()
    {
        GamePlayerData[] opponents = { new GamePlayerData("0", "janusz", 0, 2, 2, true),
            new GamePlayerData("0", "grzybeusz", 0, 0, 0, true)
        };
        int seed = Randomizer.GetRandomNumber(0, 10000);// 1000;
        GameData gameData = new GameData(seed, 1, opponents);
        StartNewGame(new Bid(1, 2, 3, 4, 5), gameData);
    }


    public void StartFourPlayersGame()
    {
        GamePlayerData[] opponents = { new GamePlayerData("0", "janusz", 0, 2, 3, true),
            new GamePlayerData("0", "grzybeusz", 0, 0, 1, true),
             new GamePlayerData("0", "polemelo", 0, 0, 0, true)
        };
        int seed = Randomizer.GetRandomNumber(0, 10000) ;// 1000;
        GameData gameData = new GameData(seed, 2, opponents);
        StartNewGame(new Bid(1, 2, 3, 4, 5), gameData);
    }


    ///TEST FUNCTIONS


    /// <summary>
    /// Method starts new game
    /// </summary>
    /// <param name="bid">Game stake bid</param>
    /// <param name="gameData">Game data object</param>
    public void StartNewGame(Bid bid, GameData gameData)
    {
        if(!IsGameEnded())
            CloseThisGame();
        gamePhase = GamePhase.CardsDealing;
        playerList.Clear();
        this.bid = bid;
        this.gameData = gameData;
        deck.ResetState(gameData.gameRandomSeed);
        ResetResettables();
        ResetPlayers();
        InitPlayers();
        StartNewMatch();
    }

    public void StartNewMatch()
    {
        RecreatePhasesQueue();
        gameData.IncreaseSeed();
        SetPlayersVisibility();
        Hand[] dealOrder = CreateHandDealOrder();
        deck.DealCards(dealOrder);
    }

    private void SetPlayersVisibility()
    {
        for (int i = 0; i < playersHands.Length; i++)
        {
            playersHands[i].SetHandVisibility(playersHands[i].playerOfThisHand != null);
        }
    }

    private void ResetResettables(bool ignoreCards = false)
    {
        for (int i = 0; i < resettables.Count; i++)
        {
            if (ignoreCards && resettables[i] is Card)
                continue;
            resettables[i].ResetState();
        }
    } 

    private void ResetPlayers()
    {
        penaltyCardsToTake = 0;
        indexOfCurrentPlayer = -1;
        playerTurnDirection = 1;
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].ResetState();
        }
    }

    private void InitPlayers()
    {
        Player p = new Player(deck);
        p.OnFinishMoveCB += StartNextPhase;
        playersHands[0].playerOfThisHand = p;
        p.name = "Player";
        playerList.Add(p);
        int playerTurnOrder = gameData.myTurnOrder;
        p.turnOrder = playerTurnOrder;
        p.SetPlayerAvatar(0);
        thisGamePlayer = p;

        for (int i = 1; i <= gameData.opponents.Length; i++)
        {
            int opponentID = ++playerTurnOrder % (gameData.opponents.Length +1);
            GamePlayerData opponent = gameData.opponents.Where(o => o.opponentTurnOrder == opponentID).FirstOrDefault();

            if(opponent == null)
            {
                Debug.LogError("InitPlayers -- There is no opponent with turnID " + opponentID);
                return;
            }


            Player opponentPlayer;
            if (opponent.opponentIsBot)
            {
                opponentPlayer = AssignBotPlayer();
            }
            else
            {
                opponentPlayer = new DummyPlayer(deck);
            }
            
            opponentPlayer.OnFinishMoveCB += StartNextPhase;
            playersHands[i].playerOfThisHand = opponentPlayer;
            opponentPlayer.name = opponent.opponentName;
            opponentPlayer.SetPlayerAvatar(opponent.opponentAvatarID);

            opponentPlayer.turnOrder = opponent.opponentTurnOrder;
            playerList.Add(opponentPlayer);

        }

        SetupPlayersOrder();
    }

    

    private Player AssignBotPlayer()
    {
        //TODO: Implement various bot types
        return new BotEasy(deck);
    }

    private void SetupPlayersOrder()
    {
        playerList = playerList.OrderBy(p => p.turnOrder).ToList();
    }

    private void ReversePlayersOrder()
    {
        playerTurnDirection = -playerTurnDirection;
    }



    public int GetWinPrize()
    {
        if (bid == null)
            return 0;
        return bid.win;
    }

    public int GetBidID()
    {
        if (bid == null)
            return -1;
        return bid.id;
    }



    private Hand[] CreateHandDealOrder()
    {
        Hand[] handOrder = playersHands.Where(h => h.playerOfThisHand != null).ToArray();

        return handOrder.OrderBy(h => h.playerOfThisHand.turnOrder).ToArray();
    }

    private void NextTurn()
    {
        IncreasePlayerIndex();
        AssignNewPlayer();
        ShowTurnDesc();
        RecreatePhasesQueue();
        StartNextPhase();
    }

    private void IncreasePlayerIndex()
    {
        indexOfCurrentPlayer = (indexOfCurrentPlayer + playerTurnDirection) % playerList.Count;
        if (indexOfCurrentPlayer < 0)
            indexOfCurrentPlayer += playerList.Count;
    }

    private void SkipNextPlayer()
    {
        IncreasePlayerIndex();
    }

    private void AssignNewPlayer()
    {
        if (currentPlayer != null)
            currentPlayer.StopTimer();
        currentPlayer = playerList[indexOfCurrentPlayer];
        currentPlayer.StartTimer();
    }

    private void RecreatePhasesQueue()
    {
        turnPhases.Clear();
        if (ThereArePenaltiesToTake())
            turnPhases.Enqueue(GamePhase.OverbidOrTakePenalties);
        else
            turnPhases.Enqueue(GamePhase.TakeOrDiscard);
    }

    private bool ThereArePenaltiesToTake()
    {
        return penaltyCardsToTake > 0;
    }

    private void AddNewMoveToQueue(GamePhase gamePhase)
    {
        turnPhases.Enqueue(gamePhase);
    }

    private void StartGame()
    {
        NextTurn();
    }

    private void StartNextPhase()
    {
        if (gamePhase == GamePhase.GameEnded)
            return;

        if (turnPhases.Count > 0)
        {
            currentPlayer.ResetTimer();
            gamePhase = turnPhases.Dequeue();
            CheckIfNextPlayerIsBot();
            CheckIfNextPlayerIsForcedToTakeCards();
        }
        else
        {
            NextTurn();
        }
    }

    private void CheckIfNextPlayerIsForcedToTakeCards()
    {

        // Tira a vez do outro jogador - utilizando para poder testar mais rapido
        SkipNextPlayer(); 

        /*
        if (IsValidGamePhase(GamePhase.OverbidOrTakePenalties) && currentPlayer.HasCardsToDefend() == false)
            TakePenaltyCards();
        */
    }

    //REMOVE AÇÃO DAS CATAS ESPECIAIS
    private void CheckForCardSpecialPower(Card card, Card prevCard)
    {
        
        if (IsValidTimeToCheckCardSpecialPower())
        {
            /*
            if (card.cardValue <= CardValue.Nine)
            {
                if (CheckTwinCardsCondition(prevCard, card) && currentPlayer.HasAnotherCardToPutInto())
                {
                    Debug.Log("Twin Card - extra tura!");
                    AddNewMoveToQueue(GamePhase.PassOrDiscard);
                }
                else if (currentPlayer.TryHighlightNextSequenceCard())
                {
                    Debug.Log("Straight - extra tura!");
                    AddNewMoveToQueue(GamePhase.PassOrDiscardNextSequencedCard);
                }
                return;
            }
            
            switch (card.cardValue)
            {
                case CardValue.Eight:
                    Debug.Log("Osemka zmiana koloru");
                    penaltyCardsToTake = 0;

                    if (IsThisGamePlayer())
                        ChangeColorWindow.instance.ShowChangeColorWindow(currentPlayer);
                    AddNewMoveToQueue(GamePhase.ChangeColor);

                    if (currentPlayer.TryHighlightNextSequenceCard())
                        AddNewMoveToQueue(GamePhase.PassOrDiscardNextSequencedCard);
                    break;
                case CardValue.Ten:
                    Debug.Log("Dziesiatka extra ruch");
                    AddNewMoveToQueue(GamePhase.TakeOrDiscard);
                    break;
                case CardValue.Reverse:
                    Debug.Log("Odracamy kolejkę ");
                    if (IsOnlyTwoPlayersGame())
                        SkipNextPlayer();
                    else
                        ReversePlayersOrder();
                    break;
                case CardValue.Skip:
                    Debug.Log("Pomijamy turę gracz");
                    SkipNextPlayer();
                    break;
                case CardValue.Plus2:
                    Debug.Log("Bierz dwie");
                    AddPenaltyCards(2);
                    break;
                case CardValue.Plus4:
                    Debug.Log("Bierz cztery");
                    AddPenaltyCards(4);
                    break;
                default:
                    break;
                
            }*/
        }
    }

    private bool CheckTwinCardsCondition(Card prevCard, Card card)
    {
        if (prevCard == null || card == null)
            return false;

        if (prevCard.cardValue == card.cardValue && prevCard.cardColor == card.cardColor)
            return true;

        return false;
    }

    public void CheckTakenCard(Card card)
    {
        if (gamePhase == GamePhase.TakeOrDiscard && currentPlayer.TimeIsOver() == false && discardPile.CheckIfCardCanBePutOnDiscardPile(card, currentPlayer))
            AddNewMoveToQueue(GamePhase.PassOrDiscard);
    }

    private void AddPenaltyCards(int penaltyCards)
    {
        penaltyCardsToTake += penaltyCards;
    }

    private void DiscardOrTakePenaltyCards()
    {
        if (currentPlayer.HasCardsToDefend() && currentPlayer.TimeIsOver() == false)
            currentPlayer.Discard();
        else
            TakePenaltyCards();
    }

    public void TakePenaltyCards()
    {
        currentPlayer.TakePenaltyCards(penaltyCardsToTake);
        FullscreenTextMessage.instance.ShowText("Take " + penaltyCardsToTake + " cards!");
        penaltyCardsToTake = 0;
        takeBtn.SetActive(false);
    }

    

    private void ShowTurnDesc()
    {
        FullscreenTextMessage.instance.ShowText(currentPlayer.name + " turn.");
    }

    

    Action botAction;

    private void CheckIfNextPlayerIsBot()
    {

        if (currentPlayer.TimeIsOver())
        {
            currentPlayer.PerformAction();
        }
        else if (currentPlayer is AIPlayer)
        {
            AIPlayer bot = currentPlayer as AIPlayer;
            botAction = GetCurrentTurnProperAction(bot);
            float decisionTime = Randomizer.GetRandomNumber(Constants.BOT_ACTION_MIN_DELAY, Constants.BOT_ACTION_MAX_DELAY);
#if TEST_MODE
    decisionTime = 1;
#endif
            if (gamePhase == GamePhase.OverbidOrTakePenalties && currentPlayer.HasCardsToDefend() == false)
                decisionTime = Constants.QUICK_ANIM_TIME;
            new Timer(decisionTime, DelayedBotAction);
        }
    }


    /// <summary>
    /// Handling server action data
    /// </summary>
    /// <param name="data"></param>
    public void HandleGameActionData(JSONObject data)
    {
       /* GameActionData gameActionData = new GameActionData(data);
        if (gamePhase != GamePhase.GameEnded && otherPlayerHand.playerOfThisHand is DummyPlayer)
        {
            (otherPlayerHand.playerOfThisHand as DummyPlayer).HandleGameActionData(gameActionData);
        }*/
    }

    public Action GetCurrentTurnProperAction(Player player)
    {

        Action action = null;
        switch (gamePhase)
        {
            case GamePhase.TakeOrDiscard:
                action = player.Discard;
                break;
            case GamePhase.PassOrDiscard:
                action = player.Discard;
                break;
            case GamePhase.PassOrDiscardNextSequencedCard:
                action = player.Discard;
                break;
            case GamePhase.ChangeColor:
                action = player.ChangeToRandomColor;
                break;
            case GamePhase.OverbidOrTakePenalties:
                action = DiscardOrTakePenaltyCards;
                break;
            default:
                break;
        }
       
        return action;
    }

    public Action GetCurrentTurnTimerPassedProperAction(Player player)
    {

        Action action = null;
        switch (gamePhase)
        {
            case GamePhase.TakeOrDiscard: 
                action = player.Pass;
                break;
            case GamePhase.PassOrDiscard:
                action = player.Pass;
                break;
            case GamePhase.PassOrDiscardNextSequencedCard:
                action = player.Pass;
                break;
            case GamePhase.ChangeColor:
                action = player.ChangeToRandomColor;
                break;case GamePhase.OverbidOrTakePenalties:
                action = DiscardOrTakePenaltyCards;
                break;
            default:
                break;
        }

        return action;
    }

    void DelayedBotAction()
    {
        if(IsProperPhaseToPerformGameMove())
            botAction.RunAction();
    }

    public void Pass()
    {
        if (currentPlayer != null)
            currentPlayer.Pass();
    }

    public void TryDiscardCard(Card card)
    {
        if (IsValidTimeToDiscard())
            currentPlayer.Discard(card);
    }

    public bool IsValidCardToDiscard(Card card)
    {
        if (card.previousParent.GetComponent<Hand>() == null)
            return false;
        return true;
    }

    public bool IsValidTimeToDiscard()
    {
        return IsValidGamePhase(GamePhase.TakeOrDiscard) || IsValidGamePhase(GamePhase.PassOrDiscard) 
            || IsValidGamePhase(GamePhase.PassOrDiscardNextSequencedCard) || IsValidGamePhase(GamePhase.OverbidOrTakePenalties);
    }

    private bool IsValidTimeToCheckCardSpecialPower()
    {
        return (gamePhase == GamePhase.TakeOrDiscard || gamePhase == GamePhase.PassOrDiscard
             || gamePhase == GamePhase.PassOrDiscardNextSequencedCard || gamePhase == GamePhase.OverbidOrTakePenalties )
             && currentPlayer.TimeIsOver() == false;
    }

    public bool IsValidTimeToTakeCard()
    {

        return IsValidGamePhase(GamePhase.TakeOrDiscard);
    }

    public bool IsValidTimeToPass()
    {
        return IsValidGamePhase(GamePhase.PassOrDiscard) || IsValidGamePhase(GamePhase.PassOrDiscardNextSequencedCard);
    }

    private bool IsValidTimeToTakePenalties()
    {
        return IsValidGamePhase(GamePhase.OverbidOrTakePenalties);
    }


    private bool IsOnlyTwoPlayersGame()
    {
        return playerList.Count == 2;
    }


    public bool IsValidGamePhase(GamePhase gPhase)
    {
        return gamePhase == gPhase && IsThisGamePlayerAndTimeIsNotOver();
    }

    public bool IsThisGamePlayerAndTimeIsNotOver()
    {
        if (currentPlayer == null)
            return false;

        return IsThisGamePlayer() &&  currentPlayer.TimeIsOver() == false;
    }

    public bool IsThisGamePlayer()
    {
        return !(currentPlayer is BotPlayer);
    }

    public bool IsThisGamePlayer(Player player)
    {
        return !(player is BotPlayer);
    }

    public bool IsThisPlayerTurn(Player player)
    {
        return currentPlayer == player;
    }

    public void TryAddThisCardAsThisPlayerZoomedCard(Card card)
    {
        if (IsThisPlayerTurn(thisGamePlayer) && card != null)
        {
            currentPlayer.RegisterZoomedCard(card);
        }
    }


    public bool IsRoundEnded()
    {
        return gamePhase == GamePhase.RoundEnded;
    }

    public bool IsGameEnded()
    {
        return gamePhase == GamePhase.GameEnded;
    }

    public bool IsGameDealingCards()
    {
        return gamePhase == GamePhase.CardsDealing;
    }

    public bool IsProperPhaseToPerformGameMove()
    {
        return !(IsGameEnded() || IsGameDealingCards() || IsRoundEnded());
    }

    internal void TryFinishTheGame(Player winner)
    {
       if(IsProperPhaseToPerformGameMove() && IsThisPlayerTurn(winner))
        {
            CloseThisGame();
            EndGameWindow.instance.ShowEndGameWindow(winner, playerList);
        }
    }

    public void CloseThisGame()
    {
        OnGameFinishedCB.RunAction();
        deck.TurnOffDeck();
        gamePhase = GamePhase.GameEnded;
    }

    public void OnLeaveGame()
    {
        DecisionPopup.instance.Show("Tem certeza que quer Sair? Você irá perder toda a pontuação!", ConfirmLeaveGame, null);
    }

    private void ConfirmLeaveGame()
    { 
        CloseThisGame();
    }
}
