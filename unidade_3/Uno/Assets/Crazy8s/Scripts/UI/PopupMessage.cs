using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessage : WindowBehaviour
{
	struct Message
	{
		public string message;
		public Action closePopupAction;
		public string btnTxt;

		public Message(string message, string btnTxt, Action closePopupAction)
		{
			this.message = message;
			this.closePopupAction = closePopupAction;
			this.btnTxt = btnTxt;
		}
	}

	public static PopupMessage instance;
	private Action closePopupAction;
	private Text txt;
	private Text btnText;
	List<Message> messages = new List<Message>();

	protected override void Awake()
	{
		instance = this;
		Init();
	}

	protected void Init()
	{
		txt = transform.Find("Panel/Text").GetComponent<Text>();
		Transform btnTextT = transform.Find("Panel/Close/Text");
		if(btnTextT != null)
		   btnText = btnTextT.GetComponent<Text>();
		base.Awake();
	}

	public void Show (string message,string buttonTxt = "Okay" ,Action closePopupAction = null) 
	{
		if (isOpen)
		{
			messages.Add(new Message(message, buttonTxt, closePopupAction));
			return;
		}
		this.closePopupAction = closePopupAction;
		txt.text = message;
		if(btnText != null)
			btnText.text = buttonTxt;
		base.ShowWindow();	
	}


	public override void CloseWindow()
	{
		base.CloseWindow();
		closePopupAction.RunAction();
		closePopupAction = null;
		if(messages.Count > 0)
		{
			Show(messages[0].message, messages[0].btnTxt, messages[0].closePopupAction);
			messages.RemoveAt(0);
		}
	}

}
