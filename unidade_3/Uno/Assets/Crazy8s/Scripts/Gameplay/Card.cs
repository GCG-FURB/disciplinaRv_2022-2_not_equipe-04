using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class Card : Draggable, IResettable, IPointerClickHandler {

	public CardColor cardColor;
	public CardValue cardValue;
	public int cardID { get; set; }
	public Action OnReverseAnimationFinishCB;
    public static Action OnColorChanged;

	private Image cardImage;

	private Sprite cardBackSprite;
	private List<Sprite> cardFrontSprite = new List<Sprite>();

	private int currentCardSet;
	private RectTransform rt;

	public bool isReversed
	{
		get
		{
			return _isReversed;
		}
		set
		{
			_isReversed = value;
			RefreshCardGraphic();
		}
	}

	public void RegisterResetable()
	{
		FindObjectOfType<GameManager>().RegisterResettable(this);
	}

	public void ResetState()
	{
		rt.pivot = Constants.vectorHalf;
		rt.anchorMax = Constants.vectorHalf;
		rt.anchorMin = Constants.vectorHalf;
		isReversed = true;
		transform.DOKill();
		gameObject.SetActive(false);
		gameObject.SetActive(true);
		canvasGroup.blocksRaycasts = false;
		previousParent = transform.parent;
		CheckIfItIsEightCard();
		NormalColorCard();
		ResetActions();
	}

	private void CheckIfItIsEightCard()
	{
		if(cardValue == CardValue.Eight)
		{
			ChangeCardColor(CardColor.black);
		}
	}


	public void ChangeCardColor(CardColor color)
	{
		cardColor = color;
		LoadGraphics();
		RefreshCardGraphic();
        OnColorChanged.RunAction();
	}

	private void RefreshCardGraphic()
	{
		if (_isReversed)
		{
			cardImage.sprite = cardBackSprite;
		}
		else
		{
			cardImage.sprite = cardFrontSprite[currentCardSet];
		}
	}

	public int GetCardPointsValue()
	{
		return (int)cardValue;
	}

	public void ResetActions()
	{
		OnAnimationFinishCB = null;
		OnReverseAnimationFinishCB = null;
	}


	public override void Awake()
	{
		base.Awake();
	}


	void Start()
	{ 
		cardImage = GetComponent<Image>();
		rt = (transform as RectTransform);
		LoadGraphics();
		RegisterResetable();
		ResetState();
	}

	private void LoadGraphics()
	{
		string path = "";
		cardFrontSprite.Clear();
		for (int i = 0; i < Constants.CARD_SETS; i++)
		{
			path = GetCardPath(i);

			cardFrontSprite.Add(Resources.Load<Sprite>(path));
		}
		cardImage.sprite = cardFrontSprite[currentCardSet];
		path = "Cards/"+currentCardSet + "/cover/cover";
		cardBackSprite = Resources.Load<Sprite>(path);
	}



	private string GetCardPath(int setID)
	{
		return "Cards/" + setID + "/" + cardColor.ToString() + "/" + cardColor.ToString() + "_" + cardValue.ToString();
	}	

	private void SwitchCardSet(int i)
	{
		currentCardSet = i;
		if(!_isReversed)
			cardImage.sprite = cardFrontSprite[currentCardSet];
	}

 

	public void RotateCard(float time)
	{
		ResetCardPositionAndRotation();
        transform.DOLocalRotate(new Vector3(0, 90, 0), time);
		transform.DOLocalRotate(new Vector3(0, 180, 0), time).SetDelay(time).OnComplete(FinishFlipCardAnim);
	}

	public void FlipCard()
	{
		isReversed = !isReversed;
	}

	private void FinishFlipCardAnim()
	{
		ResetCardPositionAndRotation();
		Invoke("RunOnReverseCallback", Time.deltaTime);
	}

	private void RunOnReverseCallback()
	{
		OnReverseAnimationFinishCB.RunAction();
	}

	private void ResetCardPositionAndRotation()
	{
		transform.localScale = Constants.vectorOne;
		transform.localRotation = Constants.quaternionIdentity;
		canvasGroup.blocksRaycasts = !_isReversed;
	}


	public void OnPointerClick(PointerEventData eventData)
	{
		ZoomCard();
	}

	public void ZoomCard(bool zoomIsForced = false)
	{
		transform.parent.BroadcastMessage("Unzoom", this);
		if (isCardZoomed)
		{
			Unzoom(null);
			GameManager.instance.TryDiscardCard(this);
		}
		else if (zoomIsForced || (GameManager.instance.IsValidTimeToDiscard() && GameManager.instance.IsValidCardToDiscard(this) 
			&& cardIsDisabled == false))
		{
			isCardZoomed = true;
			beforeZoomLocalPos = transform.localPosition;
			transform.AnimateParentScale(Constants.TAP_ZOOM_CARD_SIZE * transform.localScale.x);
			transform.AnimateLocalPosition(transform.localPosition + Vector3.up * Constants.SELECTED_MOVE_CARD_HEIGHT, Constants.QUICK_ANIM_TIME);
			GameManager.instance.TryAddThisCardAsThisPlayerZoomedCard(this);
		}
	}
   

	private void DisableColorCard()
	{
		cardIsDisabled = true;
		UnlockCard(false);
		cardImage.DOColor(Constants.disabledCardColor, Constants.QUICK_ANIM_TIME);
    }


	private void NormalColorCard()
	{
		cardIsDisabled = false;
		UnlockCard(true);
		cardImage.DOColor(Color.white, Constants.QUICK_ANIM_TIME);
	}

	public void SetCardColor(bool state)
	{
		if (state)
			NormalColorCard();
		else
			DisableColorCard();
	}
}
