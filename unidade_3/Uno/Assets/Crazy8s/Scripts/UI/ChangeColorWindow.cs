using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorWindow : WindowBehaviour {

    public static ChangeColorWindow instance;
    private Player colorChanger;


    protected override void Awake()
    {
        instance = this;
        base.Awake();
    }

    public void ShowChangeColorWindow(Player colorChanger)
    {
        this.colorChanger = colorChanger;
        ShowWindow();
    }

    public void ChangeColor(int id)
    {
        if(colorChanger != null)
        {
            colorChanger.ChangeTopCardColor(id);
            colorChanger = null;
        }
        CloseWindow();
    }
}
