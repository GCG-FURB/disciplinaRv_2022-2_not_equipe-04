using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

[Serializable]
public class Player {
    public Action OnFinishMoveCB;

    private string _name;
    public string name
    {
        get { return _name; }
        set
        {
            _name = value;
            playerUI.RefreshPlayerName(name);
        }
    }

    public int avatarID { get; protected set; }
    public int turnOrder { get; set; }

    public int GetScore()
    {
        int score = 0;
        List<Card> cards = myHand.GetCardsFromZone();

        for (int i = 0; i < cards.Count; i++)
        {
            score += cards[i].GetCardPointsValue();
        }

        return score;
    }

    protected Hand myHand;
    protected Deck deck;
    protected DiscardPile discardPile;
    protected GameManager gameManager;

    private Card _cardOnAction;
    protected PlayerUI playerUI;
    private bool inAction;
    private Card zoomedCard;

    protected Card cardOnAction
    {
        get
        {
            return _cardOnAction;
        }
        set
        {
            _cardOnAction = value;
            inAction = true;
        }
    }

    public Player(Deck deck)
    {
        this.deck = deck;
        discardPile = GameObject.FindObjectOfType<DiscardPile>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    public virtual void InitPlayerUI(PlayerUI ui)
    {
        playerUI = ui;
        playerUI.ResetUI(PerformAction);
        myHand.OnChildCountChangedCB = playerUI.RefreshRemainingCardsText;
    }

    public void AssignHand(Hand hand)
    {
        myHand = hand;
    }

#region Player_Game_Actions
    public virtual void PerformAction()
    {
        if (inAction == false)
        {
            Action action = gameManager.GetCurrentTurnTimerPassedProperAction(this);
            RunTimerWithAction(action);
        }
    }

    private void RunTimerWithAction(Action action)
    {
        myHand.LockZone();
        new Timer(Constants.AUTOTURN_DELAY, action.RunAction);
    }


    public void TakeStockPile()
    {
        TakeCard(deck);
    }

    public virtual void Discard()
    {
        cardOnAction = GetRandomCardFromHand();
        DiscardSelectedCard();
    }


    public void Discard(int cardID)
    {
        cardOnAction = myHand.GetCardFromHand(cardID);
        if (cardOnAction != null)
            DiscardSelectedCard();
    }


    public void Discard(Card card)
    {
            cardOnAction = card;
            DiscardSelectedCard();
    }

    public void DiscardZoomedCard()
    {
        if (zoomedCard != null && zoomedCard.isCardZoomed)
        {
            Discard(zoomedCard);
        }
    }



    protected void DiscardSelectedCard()
    {
        if (cardOnAction != null)
        {
            myHand.LockZone();
            cardOnAction.transform.DOKill();
            discardPile.OnDrop(cardOnAction);
            cardOnAction.MoveCard(Constants.MOVE_ANIM_TIME);
            cardOnAction.RegisterAction(FinishMove);
            cardOnAction.RegisterAction(myHand.ReorderChildren);
            zoomedCard = null;
            myHand.OnCardDiscard();
        }
        else
        {
            if (gameManager.gamePhase == GamePhase.TakeOrDiscard)
            {
                Debug.Log("Can't discard any card so I take one");
                TakeCard();
            }
            else if(gameManager.gamePhase == GamePhase.PassOrDiscard)
            {
                Debug.Log("Can't discard any card so I pass");
                Pass();
            }
        }
    }


public void TakeCard()
    {
        TakeCard(GetPile());
    }

    public void TakeCard(ITakeCard cardPile)
    {
        cardOnAction = cardPile.TakeCard(myHand);
        if (cardOnAction != null)
        {
            cardOnAction.RegisterAction(CheckTakenCard);
            cardOnAction.RegisterAction(FinishMove);
        }
    }

    private void CheckTakenCard()
    {
        gameManager.CheckTakenCard(cardOnAction);
    }

    


    #endregion

    protected virtual ITakeCard GetPile()
    {
        return deck;
    }

    protected virtual void FinishMove()
    {
        if (cardOnAction != null)
        {
            cardOnAction.UnregisterAction(FinishMove);
            cardOnAction.UnregisterAction(myHand.ReorderChildren);
            cardOnAction.UnregisterAction(CheckTakenCard);
        }
        inAction = false;
        UnlockHand();
        OnFinishMoveCB.RunAction();
    }

  

    private bool CheckIfCardIsFromDeck(ITakeCard cardPile)
    {
        return cardPile is Deck;
    }

    public void RegisterNewCardMove(Card card)
    {
        cardOnAction = card;
        cardOnAction.RegisterAction(FinishMove);
    }

    protected Card GetRandomCardFromHand()
    {
        List<Card> cards = GetChooseCardRule()();


        if (cards.Count == 0)
            return null;
        
        return cards.GetRandomElementFromList(); 
    }

    private Func<List<Card>> GetChooseCardRule()
    {
        if (gameManager.gamePhase == GamePhase.PassOrDiscardNextSequencedCard)
            return myHand.GetAvailableSequencedCard;
        if (gameManager.gamePhase == GamePhase.OverbidOrTakePenalties)
            return myHand.GetAvailableToDefendCards;

        return myHand.GetAvailableCardsFromZone;
    }

    public void RegisterZoomedCard(Card card)
    {
        zoomedCard = card;
    }

    public Sprite GetPlayerAvatar()
    {
        return playerUI.playerAvatar.sprite;
    }

    public void SetPlayerAvatar(int id)
    {
        playerUI.RefreshAvatar(id);
    }

    public void StartTimer()
    {
        playerUI.StartTimer();
    }
    
    public void ResetTimer()
    {
        playerUI.ResetTimer();
    }

    public void StopTimer()
    {
        if (playerUI != null)
            playerUI.StopTimer();
    }

    public void UnlockHand()
    {
        myHand.UnlockZone();
    }

    public bool TimeIsOver()
    {
        return playerUI.TimeIsOver();
    }

    public void WinGame()
    {
        //TODO: Win game
    }

    public void LoseGame()
    {
        //TODO: Lose game
    }

    public virtual void ResetState()
    {

    }

    public void ChangeToRandomColor()
    {
        ChangeTopCardColor(Randomizer.GetRandomNumber(0, Constants.CARDS_COLORS));
    }

    public void ChangeTopCardColor(int color)
    {
        Card card = discardPile.GetTopCardFromDiscardPile();
        if (card != null)
            card.ChangeCardColor((CardColor)color);
        ChangeColorWindow.instance.CloseWindow();
        FinishMove();
    }

    public void Pass()
    {
        FinishMove();
    }

    public bool TryHighlightNextSequenceCard()
    {
        return  myHand.TryHighlightNextSequenceCard();
    }

    public void TakePenaltyCards(int penaltyCardsToTake)
    {
        for (int i = 0; i < penaltyCardsToTake; i++)
        {
            if(i == penaltyCardsToTake -1)
                new Timer(i * Constants.QUICK_ANIM_TIME, 
                    () => 
                    {
                        cardOnAction = deck.TakeCard(myHand);
                        if (cardOnAction != null)
                        {
                            cardOnAction.RegisterAction(CheckTakenCard);
                            cardOnAction.RegisterAction(FinishMove);
                        }
                    });
            else
                new Timer(i * Constants.QUICK_ANIM_TIME, () => deck.TakeCard(myHand));
        }
        
    }

    public bool HasCardsToDefend()
    {
        List<Card> cards = myHand.GetAvailableToDefendCards();
        if(cards.Count == 0)
            return false;
        return true;
    }

    public bool HasAnotherCardToPutInto( )
    {
        List<Card> cards = myHand.GetAvailableCardsFromZone();
        if (cards.Count == 0)
            return false;
        return true;
    }

    public int InHandCardsCount()
    {
        return myHand.InHandCardsCount();
    }
}
