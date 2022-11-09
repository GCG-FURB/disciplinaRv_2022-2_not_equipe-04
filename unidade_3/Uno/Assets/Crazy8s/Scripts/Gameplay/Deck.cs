using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Deck : MonoBehaviour, ITakeCard, IPointerDownHandler {

    private GameObject cardPrefab;
    private List<GameObject> cardsPool;
    private DiscardPile discardPile;
    private GameManager gameManager;
    private bool lockTakeCard;
    private Text cardsOnDeckText;

    public Action OnCardDealtCB;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        cardPrefab = Resources.Load<GameObject>("CardPrefabs/CardPrefab");
        cardsOnDeckText = transform.parent.GetComponentInChildren<Text>();
        discardPile = FindObjectOfType<DiscardPile>();
        PrepareObjectsPool();
    }

    private void Start()
    {
        TurnOffDeck();
    }

    public void TurnOffDeck()
    {
        gameObject.SetActive(false);
    }

    private void PrepareObjectsPool()
    {
        List<CardValue> valuesAsArray = Enum.GetValues(typeof(CardValue)).Cast<CardValue>().ToList();
        List<CardColor> colorsAsArray = Enum.GetValues(typeof(CardColor)).Cast<CardColor>().ToList();

        cardsPool = new List<GameObject>();
        int colorIndex = -1;
        for (int i = 0; i < Constants.CARD_DECKS ; i++)
        {
            for (int j = 0; j < Constants.CARDS_IN_DECK; j++)
            {
                if (j % Constants.CARDS_IN_COLOR == 0)
                    colorIndex = (colorIndex + 1) % Constants.CARDS_COLORS;
                GameObject cardGO = Instantiate(cardPrefab, transform);
                Card card = cardGO.GetComponent<Card>();
                card.cardID = j + i * Constants.CARDS_IN_DECK;
                card.cardValue = valuesAsArray[j % Constants.CARDS_IN_COLOR];
                card.cardColor = colorsAsArray[colorIndex];
                card.name = card.cardColor + "_" + card.cardValue;
                cardsPool.Add(cardGO);
            }
        }
    }

    public void PrepareDeckToReorder(int gameRandomSeed,Hand[] handsToDeal)
    {
        List<GameObject> cards = GetCardsFromDeck();
        cards = ShuffleFromSeed(gameRandomSeed, cards);
        DealCards(handsToDeal, cards);
    }

    public void ResetState(int randomSeed)
    {
#if TEST_MODE
        if (isTestMode == false)
#endif
            cardsPool = ShuffleFromSeed(randomSeed, cardsPool);
        BackCardsToInitialState();
    }

    private void ShuffleCards( )
    {
        cardsPool.ShuffleList();
        Resources.UnloadUnusedAssets();
    }

    private List<GameObject> ShuffleFromSeed(int seed,List<GameObject> elementsToSort)
    {
        int[] myRandomNumbers = Randomizer.GetRandomDeckFromSeed(seed, elementsToSort.Count);
        return SortCardsByIDArray(myRandomNumbers, elementsToSort);
    }

    private List<GameObject> SortCardsByIDArray(int[] idsArray, List<GameObject> elementsToSort)
    {
        if (cardsPool == null)
            PrepareObjectsPool();
        try
        {
            elementsToSort = elementsToSort.OrderBy(x => x.name).ToList();
            List<GameObject> tempCards = new List<GameObject>();
            for (int i = 0; i < elementsToSort.Count; i++)
            {
                tempCards.Add(elementsToSort[idsArray[i]]);
            }
            return tempCards;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return elementsToSort;
        }
    }

   

    private void BackCardsToInitialState()
    {
        for (int i = 0; i < cardsPool.Count; i++)
        {
            cardsPool[i].transform.SetParent(transform);
            cardsPool[i].transform.localPosition = Vector3.zero;
            cardsPool[i].transform.localScale = Constants.vectorOne;
            cardsPool[i].transform.rotation = transform.rotation;
            cardsPool[i].transform.SetAsLastSibling();
        }
        Resources.UnloadUnusedAssets();
    }

    public void DealCards(Hand[] handsToDeal,List<GameObject> deck = null)
    {
        if(deck == null)
            deck = cardsPool;
        gameObject.SetActive(true);
        RefreshCardsCountText();
        StartCoroutine(DealCardsCoroutine(handsToDeal, deck));
    }

    private IEnumerator DealCardsCoroutine(Hand[] handsToDeal, List<GameObject> deck)
    {
        int index = 0;
        yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);

        for (int i = 0; i < Constants.CARDS_IN_START_HAND; i++)
        {
            for (int j = 0; j < handsToDeal.Length; j++)
            {
                Hand hand = handsToDeal[j];
                Card card = deck[index].GetComponent<Card>();

                float cardRotationAngle = 0;
                Vector3 moveDestination = hand.GetNextAvailablePosition(out cardRotationAngle, card);
                CardDestination cardDestination = new CardDestination(hand.transform, moveDestination, cardRotationAngle);

                card.DealCard(cardDestination);
                card.isReversed = true;
                index++;
                RefreshCardsCountText();
                yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);
            }

            if(handsToDeal.Length <= 1)
                yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);
        }
        PutLastCardToDiscardPile(index,deck);
        RefreshCardsCountText();
        yield return new WaitForSeconds(Constants.DEAL_ANIM_TIME);
        OnCardDealtCB.RunAction();
    }

    private void PutLastCardToDiscardPile(int index, List<GameObject> deck)
    {
        CardDestination cardDestination = new CardDestination(discardPile.transform, discardPile.transform.position, 0);
        Card card;
        do
        {
            card = deck[index++].GetComponent<Card>();
        }
        while (card.cardValue > CardValue.Nine);
        card.DealCard(cardDestination);
    }

    public Card TakeCard(Hand hand)
    {
       // if (gameManager.IsThisGamePlayer())
            //PlayerManager.instance.SendActionToServer(GameAction.TakeStockPile);
        Card card = transform.TakeCard(hand);
        RefreshCardsCountText();
        CheckIfDeckIsEmpty();
        if (card == null)
        {
            Debug.Log("Deck is empty! Reorder it!");
        }
        else
        {
            card.SetCardColor(true);
        }
        return card;
    }

    private void CheckIfDeckIsEmpty()
    {
        int remainingOnDeckCards = GetRemainingCardsNumber();
        if(remainingOnDeckCards == 0)
        {
            ReturnDiscardedCardsToDeck();
        }
        
    }

    private void ReturnDiscardedCardsToDeck()
    {
        List<Card> cards = discardPile.GetComponentsInChildren<Card>().ToList();
        cards.RemoveAt(cards.Count - 1);
        cards.ShuffleList();
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            CardDestination cardDestination = new CardDestination(transform, transform.position, 0);
            card.DealCard(cardDestination);
            if (i == cards.Count - 1)
                RegisterCallbackOnLastCardInDeck(card);
        }
    }

    private void RegisterCallbackOnLastCardInDeck(Card card)
    {
        card.OnReverseAnimationFinishCB = () => 
        {
            RefreshCardsCountText();
            card.OnReverseAnimationFinishCB = null;
        };
    }

    private int GetRemainingCardsNumber()
    {
        return transform.GetComponentsInChildren<Card>().Length;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lockTakeCard)
            return;
        if (gameManager.IsValidTimeToTakeCard())
        {
            gameManager.currentPlayer.TakeCard(this);
            RunLockCooldown();
        }
    }

    private void RunLockCooldown()
    {
        lockTakeCard = true;
        Invoke("UnlockHand", Constants.LOCK_HAND_TIME);
    }

    private void UnlockHand()
    {
        lockTakeCard = false;
    }

    public void RefreshCardsCountText()
    {
        string cardAmountTxt = transform.childCount > 0 ? transform.childCount.ToString() : "";
        cardsOnDeckText.text = cardAmountTxt;
    }

    public void FadeDeckCards(bool fade)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetCardColor(!fade);
        }
    }


    public void Shake()
    {
        transform.parent.DOShakeScale(1, 0.12f, 3, 45);
    }


    public List<GameObject> GetCardsFromDeck()
    {
        List<GameObject> cards = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Card c = transform.GetChild(i).GetComponent<Card>();
            if (c != null)
            {
                cards.Add(c.gameObject);
            }
        }
        return cards;
    }

   
}
