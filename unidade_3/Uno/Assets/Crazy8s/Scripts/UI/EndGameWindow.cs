using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameWindow : WindowBehaviour {

    public static EndGameWindow instance;

    private Text winTxt;

    override protected void Awake()
    {
        base.Awake();
        instance = this;
        winTxt = transform.Find("Window/Desc").GetComponent<Text>();
    }



    public void ShowEndGameWindow(Player winner, List<Player> players)
    {
        /*
        Player p1 = new Player;
        Player p2 = new Player;
        Player p3 = new Player;
        Player p4 = new Player;

        p1 = players[0];
        */
        ShowWindow();
        int score = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != winner)
                score += players[i].GetScore();
            winTxt.text = "O Vencedor é: " + winner.name + " - Pontuação Final: " + score + "\n\n";
            winTxt.text = "--------------------------------------------";
            winTxt.text = players[0] + "\n\n Pontuação Final: " + score;
            
        }

    }
}
