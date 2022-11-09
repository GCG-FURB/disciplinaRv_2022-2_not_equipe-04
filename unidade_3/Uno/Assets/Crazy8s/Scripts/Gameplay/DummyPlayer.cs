using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameActionData
{
    public GameAction gameActionType { get; private set; }
    public int gameActionParameter { get; private set; }
    public int[] handCardsState { get; private set; }

    public GameActionData(JSONObject actionData)
    {
        gameActionType = actionData["gameActionType"].str.ToEnum(GameAction.None);
        gameActionParameter = actionData["gameActionParameter"].n;
        if (actionData["handCardsState"].IsNull == false)
        {
            handCardsState = new int[actionData["handCardsState"].Count];
            for (int i = 0; i < handCardsState.Length; i++)
            {
                handCardsState[i] = actionData["handCardsState"][i].n;
            }
        }
        else
        {
            handCardsState = new int[0];
        }
    }
}

public class DummyPlayer : BotPlayer
{

    public DummyPlayer(Deck deck) : base(deck)
    {
    }

    private Queue<Action> opponentActionsQueue = new Queue<Action>();
    private bool isOnAction;

    public void HandleGameActionData(GameActionData gameActionData)
    {
        Action actionToTake = null;

        switch (gameActionData.gameActionType)
        {
            case GameAction.None:
                break;
            case GameAction.Pass:
                break;
            case GameAction.TakeDiscard:
                break;
            case GameAction.TakeStockPile:
                actionToTake = TakeStockPile;
                break;
            case GameAction.Discard:
                actionToTake = () => Discard(gameActionData.gameActionParameter);
                break;
            case GameAction.FinishRound:
                break;
            case GameAction.ShowEmoticon:
                actionToTake = () => ShowEmoticonAction(gameActionData.gameActionParameter);
                break;
            case GameAction.ZoomCard:
                actionToTake = () => ZoomCard(gameActionData.gameActionParameter);
                break;
            default:
                break;
        }

        opponentActionsQueue.Enqueue(
            () =>
            {
                isOnAction = true;
                myHand.ReorderCards(gameActionData.handCardsState, actionToTake);
            });
        RunNextAction();
    }

    
    void RunNextAction()
    {
        if(opponentActionsQueue.Count > 0 && isOnAction == false && gameManager.IsProperPhaseToPerformGameMove())
        {
            opponentActionsQueue.Dequeue().RunAction();
        }
    }

    private void ShowEmoticonAction(int emoticonID)
    {
        FinishAction();
    }

    private void ZoomCard(int gameActionParameter)
    {
        myHand.ZoomCard(gameActionParameter);
        FinishAction();
    }


    public override void PerformAction()
    {
        Debug.Log("Koniec czasu przeciwnika");
    }

    protected override void FinishMove()
    {
        base.FinishMove();
        FinishAction();
    }

    private void FinishAction()
    {
        isOnAction = false;
        RunNextAction();
    }

    public override void ResetState()
    {
        opponentActionsQueue.Clear();
        isOnAction = false;
    }
}
