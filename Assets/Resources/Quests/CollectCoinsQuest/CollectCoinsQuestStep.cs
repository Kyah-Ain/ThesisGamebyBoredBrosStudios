using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//test custom Quest
public class CollectCoinsQuestStep : QuestStep
{
    private int coinsCollected = 0;

    private int coinsToComplete = 5;

    private void OnEnable()
    {
        //GameEventManager.instance.miscEvents.onCoinCollected += CoinCollected;   
    }
    private void OnDisable()
    {
        //GameEventManager.instance.miscEvents.onCoinCollected -= CoinCollected;   
    }

    private void CoinCollected()
    {
        //if (coinsCollected < coinsToComplete)
        //{
            //coinsCollected++;
            //UpdateState();
        //}

        //if (coinsCollected >= coinsToComplete)
        //{
            //FinishQuestStep();
        //}
    }

    private void UpdateState()
    {
        string state = coinsCollected.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        this.coinsCollected = System.Int32.Parse(state);
        UpdateState();
    }
}
