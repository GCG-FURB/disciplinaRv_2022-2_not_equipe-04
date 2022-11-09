using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DiscardPile : DropZone, IPointerDownHandler {

    bool lockTakeCard;
    public Action OnDiscardPileDrop;
    public Action<Card, Card> OnCardPutOnDiscardPile;

    public override void OnDrop(Card card)
    {
        if (card != null && card.previousParent != transform)
        {
            if (gameManager.IsThisGamePlayer() && gameManager.IsRoundEnded() == false)
            {
               //Send action to server
               // PlayerManager.instance.SendActionToServer(GameAction.Discard, card.cardID);

            }
            float cardRotation = Randomizer.GetRandomNumber(-Constants.DISCARDPILE_CARD_MAX_ANGLE, Constants.DISCARDPILE_CARD_MAX_ANGLE);
            CardDestination cardDestination = new CardDestination(transform, transform.position, cardRotation);
            card.SetReturnPoint(cardDestination);
            card.SetCardHolderAsParent();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lockTakeCard)
            return;
        if (gameManager.IsValidTimeToDiscard())
        {
            gameManager.currentPlayer.DiscardZoomedCard();
        }
    }

    private void RunLockCooldown()
    {
        lockTakeCard = true;
       new Timer (Constants.LOCK_HAND_TIME, UnlockDiscardPile);
    }

    private void UnlockDiscardPile()
    {
        lockTakeCard = false;
    }


    public override void AssignNewChild(Transform child)
    {
        Card prevCard = GetTopCardFromDiscardPile();

        base.AssignNewChild(child);
        OnDiscardPileDrop.RunAction();

        Card card = child.GetComponent<Card>();
        if (card != null)
        {
            OnCardPutOnDiscardPile.RunAction(card, prevCard);
        }
        
    }


    public bool CheckIfCardCanBePutOnDiscardPile(Card card, Player player)
    {
        Card topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard == null)
        {
            Debug.LogError("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
            return false;
        }

        if (card.cardValue == CardValue.Eight)
            return true;

        if (card.cardValue == CardValue.Ten && player.InHandCardsCount() == 1)
        {
            return false;
        }


        if (card.cardValue == topDeckCard.cardValue || card.cardColor == topDeckCard.cardColor)
            return true;
        return false;
    }

    public bool IsNextSequencedCard(Card card, Player player)
    {

        Card topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard == null)
        {
            Debug.LogError("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
            return false;
        }

        if (topDeckCard.cardValue == CardValue.Seven && card.cardValue == CardValue.Eight)
            return true;

        if (card.cardColor != topDeckCard.cardColor)
            return false;

        if (card.cardValue == topDeckCard.cardValue + 10)
            return true;

        

        if (topDeckCard.cardValue == CardValue.Eight && card.cardValue == CardValue.Nine)
            return true;


        if (topDeckCard.cardValue == CardValue.Nine && card.cardValue == CardValue.Ten)
        {
            if(player.InHandCardsCount() == 1)
                return false;

            return true;
        }

        return false;
    }

    public bool CheckIfCardCardCanRescueFromPenalty(Card c, Player playerOfThisHand)
    {
        Card topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard == null)
        {
            Debug.LogError("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
            return false;
        }

        if (c.cardValue == CardValue.Eight)
            return true;

        if(topDeckCard.cardValue == CardValue.Plus2)
        {
            if (c.cardValue == CardValue.Plus2 || c.cardValue == CardValue.Plus4)
                return true;
        }

        if(topDeckCard.cardValue == CardValue.Plus4)
        {
            if (c.cardValue == CardValue.Plus4)
                return true;
        }

        return false;
    }

    public Card GetTopCardFromDiscardPile()
    {
        if (transform.childCount > 0)
            return transform.GetChild(transform.childCount - 1).GetComponent<Card>();
        return null;
    }

    
}
