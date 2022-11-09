using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;

public class Hand : DropZone, IResettable {

    public Action OnCardsPositionRefreshCB;
    public Action<int> OnChildCountChangedCB;
    public PlayerUI playerUI;

    private bool isArch = true;
    private Player _playerOfThisHand;
    private SortingMethod sortingMethod = SortingMethod.Colors;
    private CanvasGroup cg;


    public Player playerOfThisHand
    {
        get { return _playerOfThisHand; }
        set
        {
            _playerOfThisHand = value;
            if (_playerOfThisHand != null)
            {
                _playerOfThisHand.AssignHand(this);
                _playerOfThisHand.InitPlayerUI(playerUI);
            }
        }
    }


    private float handWidth;
    private RectTransform rectTransform;
    protected float cardFitSpeed;

    float spacePerCard;
    float twistPerCard;
    float startTwist;
    bool gameIsStarted;

    protected override void Awake()
    {
        base.Awake();
        cg = GetComponent<CanvasGroup>();
        cardFitSpeed = Constants.QUICK_ANIM_TIME;
        discardPile = FindObjectOfType<DiscardPile>();
        discardPile.OnDiscardPileDrop += 
            () =>
        {
            LockNotSuitableCards();
            OnCardDiscard();
        };
        Card.OnColorChanged += LockNotSuitableCards;
        gameManager.OnGamePhaseChangeCallback += LockNotSuitableCards;
        RegisterResetable();
        deck.OnCardDealtCB += UnlockHand;
    }

    public void SetHandVisibility(bool state)
    {
        if (state)
        {
            cg.ShowCG(Constants.QUICK_ANIM_TIME);
        }else
        {
            cg.HideCG(Constants.QUICK_ANIM_TIME);
        }

        playerUI.SetVisibility(state);
    }

    private void ChangeHandShape(bool value)
    {
        isArch = value;
        if(gameIsStarted)
            ReorderChildren();
    }

    protected void Start()
    {
        GameManager.OnGameFinishedCB += LockHand;
        rectTransform = transform as RectTransform;
        handWidth = rectTransform.sizeDelta.x;
        ResetState();
    }

    public void RegisterResetable()
    {
        gameManager.RegisterResettable(this);
    }

    public void ResetState()
    {
        playerOfThisHand = null;
        LockHand();
        CalculateSpaceAndRotationParameters(Constants.CARDS_IN_START_HAND);
    }

    private void LockHand()
    {
        gameIsStarted = false;
        LockZone();
    }

    private void UnlockHand()
    {
        gameIsStarted = true;
        UnlockZone();
        SortCards();
    }
    
    private void CalculateSpaceAndRotationParameters(int startSize = 0)
    {
        // int cardsCount = startSize == 0 ?  transform.childCount : startSize;
        int cardsCount = transform.childCount > Constants.CARDS_IN_START_HAND ? transform.childCount : Constants.CARDS_IN_START_HAND;

        float childrenCount = Mathf.Max(1, cardsCount);
        spacePerCard = handWidth /  childrenCount;
        twistPerCard = GetFanAngle() / childrenCount;
        startTwist = GetFanAngle() / 2f;
    }

    protected virtual float GetFanAngle()
    {
        return isArch ? Constants.FAN_ANGLE : 0;
    }

    public override void AssignNewChild(Transform child, int cardOrder)
    {
        base.AssignNewChild(child);
        child.SetSiblingIndex(cardOrder);
        SortCards();
    }

    public void ZoomCard(int gameActionParameter)
    {
        Card card = GetCardFromHand(gameActionParameter);
        if(card != null)
            card.ZoomCard(true);
    }

    public override void AssignNewChild(Transform child)
    {
        int pos = GetTheBestCardPosition(child.GetComponent<Card>());
        AssignNewChild(child,pos);
        ShowCard(child);
        OnChildCountChangedCB.RunAction(transform.childCount);
    }

    protected virtual void ShowCard(Transform child)
    {
        Card card = child.GetComponent<Card>();
        if (card != null)
        {
           card.isReversed = false;
        }
    }



    public override void UnparentChild(Transform child, Transform newParent)
    {
        base.UnparentChild(child, newParent);
        RegisterReorderAction(child);
        SortCards();
    }

    public void OnCardDiscard()
    {
        OnChildCountChangedCB.RunAction(transform.childCount);
        if (transform.childCount == 0)
            gameManager.TryFinishTheGame(playerOfThisHand);
    }

    private void RegisterReorderAction(Transform child)
    {
        Card c = child.GetComponent<Card>();
        if (c != null)
            c.OnPlaceHolderChangePos += ReorderChildren;
    }

    

    private void UnregisterReorderAction(Transform child)
    {
        Card c = child.GetComponent<Card>();
        if (c != null)
            c.OnPlaceHolderChangePos -= ReorderChildren;
    }

  
    public void ReorderChildren()
    {
        CalculateSpaceAndRotationParameters();
        int extraCardSpace = CalculateExtraSpace();

        for (int i = extraCardSpace; i < transform.childCount + extraCardSpace; i++)
        {
            Transform cardChild = transform.GetChild(i - extraCardSpace);
       
            float twistForThisCard =  CalculateCardTwist(i);
            Vector3 newRot = new Vector3(0, 0, twistForThisCard);
            cardChild.AnimateLocalRotation(newRot,cardFitSpeed);
            float cardHeight = CalculateCardHeight(twistForThisCard, i);           
            cardChild.AnimateParentScale(GetScaleFactor());
            float cardXsize = i * spacePerCard;
            Vector3 newPos = new Vector3(cardXsize, cardHeight, 0) + Constants.HAND_CARDS_OFFSET;
            cardChild.AnimateLocalPosition(newPos, cardFitSpeed);
        }
        LockNotSuitableCards();
        OnCardsPositionRefreshCB.RunAction();     
    }

    private int CalculateExtraSpace()
    {
        return Mathf.CeilToInt(Mathf.Max(Constants.CARDS_IN_START_HAND - transform.childCount, 0) / 2f);
    }

    protected virtual void LockNotSuitableCards()
    {
        List<Card> cards = GetCardsFromZone();


        Func<Card, Player, bool> cardValidator = GetValidationMethod();
      

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetCardColor(gameManager.IsThisPlayerTurn(playerOfThisHand) &&  cardValidator(cards[i], playerOfThisHand));
        }
    }

    private Func<Card, Player, bool> GetValidationMethod()
    {
        if (gameManager.IsValidGamePhase(GamePhase.PassOrDiscardNextSequencedCard))
            return discardPile.IsNextSequencedCard;

        if (gameManager.IsValidGamePhase(GamePhase.OverbidOrTakePenalties))
            return discardPile.CheckIfCardCardCanRescueFromPenalty;

        return discardPile.CheckIfCardCanBePutOnDiscardPile;
    }

    protected virtual float GetScaleFactor()
    {
        return 1;
    }

    private float CalculateCardTwist(int cardsCount)
    {
        return startTwist - (cardsCount * twistPerCard);
    }

    private float CalculateCardHeight(float cardAngle, int cardIndex)
    {
        int childCount = transform.childCount > Constants.CARDS_IN_START_HAND ? transform.childCount : Constants.CARDS_IN_START_HAND;
        int cardsPerSide = childCount / 2;
        float cardRelativeFromCenterPositionIndex = cardIndex - cardsPerSide;

        return -Mathf.Abs(cardAngle * cardRelativeFromCenterPositionIndex);
    }

    

    public Vector3 GetNextAvailablePosition(out float cardRotation, Card card)
    {
        int cardAvailablePlace = GetTheBestCardPosition(card);
        cardRotation = transform.localScale.y * CalculateCardTwist(cardAvailablePlace);
        float cardHeight = CalculateCardHeight(cardRotation, cardAvailablePlace);
        cardRotation += transform.rotation.eulerAngles.z;
        Vector3 pos = new Vector3(cardAvailablePlace * spacePerCard, cardHeight, 0)  + Constants.HAND_CARDS_OFFSET;
        return transform.TransformPoint(pos);
    }

  

    protected virtual int GetTheBestCardPosition(Card cardToFit)
    {
        if(cardToFit != null && gameIsStarted)
        {
            List<Card> cards = GetCardsFromZone();

            cards.Add(cardToFit);
            cards = cards.SortCardsByColor();
            return cards.IndexOf(cardToFit) + CalculateExtraSpace();
        }

        return transform.childCount;
    }

  

    public override void OnDrop(PointerEventData eventData)
    {
        
    }

    public void ChangeSortingType(SortingMethod method)
    {
        sortingMethod = method;
        SortCards();
    }

    private void SortCards()
    {
        if (gameIsStarted)
        {
            switch (sortingMethod)
            {
                case SortingMethod.None:
                    break;
                case SortingMethod.Values:
                    SortCardsByValue();
                    break;
                case SortingMethod.Colors:
                    SortCardsByColor();
                    break;
                default:
                    break;
            }
        }
    }

    private void SortCardsByValue()
    {
        List<Card> cards = GetCardsFromZone().SortCardsByValue();
        SyncAndReorderCards(cards);
    }

   

    private void SortCardsByColor()
    {
        List<Card> cards = GetCardsFromZone().SortCardsByColor();
        SyncAndReorderCards(cards);
    }

    public void ReorderCards(int[] handCardsState, Action cardAction = null)
    {
        List<Card> cards = new List<Card>();
        List<Card> thisHandCards = GetCardsFromZone();
        for (int i = 0; i < handCardsState.Length; i++)
        {
            Card card = GetCardFromHand(handCardsState[i]);
            if (card == null)
            {
                Debug.LogError("There is no card id: " + handCardsState[i] + " on hand.");
            }
            else
            {
                cards.Add(card);
            }
        }
        SyncAndReorderCards(cards);
        cardAction.RunAction();
    }

    public Card GetCardFromHand(int cardID, List<Card> thisHandCards = null)
    {
        if (thisHandCards == null)
            thisHandCards = GetCardsFromZone();
        Card card = thisHandCards.Find(x => x.cardID == cardID);
        return card;
    }

    private void SyncAndReorderCards(List<Card> cards)
    {
        SyncCardPostion(cards);
        ReorderChildren();
    }

    void SyncCardPostion(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetAsLastSibling();
        }
    }


    public bool TryHighlightNextSequenceCard()
    {
        List<Card> cards = GetAvailableSequencedCard();

        if (cards.Count == 0)
            return false;
        else
            return true;
    }

    public List<Card> GetAvailableSequencedCard()
    {
        List<Card> cards = GetCardsFromZone();
        Card card = discardPile.GetTopCardFromDiscardPile();
        if (card == null)
            return cards;
        return cards.Where(c => c.cardColor == card.cardColor && discardPile.IsNextSequencedCard(c, playerOfThisHand)).ToList();
    }

    public List<Card> GetAvailableCardsFromZone()
    {
        List<Card> cards = GetCardsFromZone();

        return cards.Where(c => discardPile.CheckIfCardCanBePutOnDiscardPile(c, playerOfThisHand) == true).ToList();
    }

    public List<Card> GetAvailableToDefendCards()
    {
        List<Card> cards = GetCardsFromZone();

        return cards.Where(c => discardPile.CheckIfCardCardCanRescueFromPenalty(c, playerOfThisHand) == true).ToList();
    }

    public int InHandCardsCount()
    {
        return transform.childCount;
    }
}
