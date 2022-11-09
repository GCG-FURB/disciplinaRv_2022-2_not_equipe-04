using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TurnTimer : MonoBehaviour {

	public Action OnTimerFinishedCB;
	
	private CanvasGroup canvasGroup;
	private Slider slider;
	private Image sliderFill;
	private float timer;
	private bool isTimerRunning;
	private Color fillColor;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		slider = GetComponent<Slider>();
		slider.maxValue = Constants.TIME_PER_TURN;
		sliderFill = transform.Find("Fill Area/Fill").GetComponent<Image>();
		fillColor = transform.Find ("Fill Area/Fill").GetComponent<Image> ().color;
		canvasGroup.alpha = 0;
		GameManager.OnGameFinishedCB += StopTimer;
	}

	public void StartTimer()
	{
        ResetTimer();
		isTimerRunning = true;
		canvasGroup.DOFade(1, Constants.QUICK_ANIM_TIME);
	}

    public void ResetTimer()
    {
        timer = Constants.TIME_PER_TURN;
    }

    private void Update()
	{
		if(isTimerRunning)
		{
			timer -= Time.deltaTime;

			if(timer <= 0)
			{
				timer = 0;
				StopTimer();
				OnTimerFinishedCB.RunAction();
			}
			sliderFill.color = Color.Lerp(Color.red, fillColor, timer / Constants.TIME_PER_TURN);
			slider.value = timer;
		}
	}

	public void StopTimer()
	{
		isTimerRunning = false;
		canvasGroup.DOFade(0, Constants.QUICK_ANIM_TIME);
	}

	public bool TimeIsOver()
	{
		return timer <= 0;
	}
}
