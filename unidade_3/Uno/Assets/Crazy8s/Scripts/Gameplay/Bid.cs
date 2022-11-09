using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Bid
{
    public int id { get; private set; }
    public int entry { get; private set; }
    public int win { get; private set; }
    public int bonusPoints { get; private set; }
    public int pointsToWin { get; private set; }

    public Bid(int id, int entry, int win, int bonusPoints, int pointsToWin)
    {
        this.id = id;
        this.entry = entry;
        this.win = win;
        this.bonusPoints = bonusPoints;
        this.pointsToWin = pointsToWin;
    }

    public Bid()
    {
           
    }

    public int GetBalance()
    {
        return entry * 10;
    }

  

}
