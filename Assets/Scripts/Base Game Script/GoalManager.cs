using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public Goals[] levelGoals;
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();

    private EndGameManager endGame;
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        board = FindAnyObjectByType<Board>();
        endGame = FindAnyObjectByType<EndGameManager>();
        GetGoals();
        SetupGoals();
    }

    void GetGoals()
    {
        if(board != null){
            if (board.world != null) {
                if (board.level < board.world.levels.Length)
                { 
                    if (board.world.levels[board.level] != null)
                    {
                        levelGoals = board.world.levels[board.level].levelGoals;
                        for(int i = 0; i< levelGoals.Length; i++)
                        {
                            levelGoals[i].numberCollected = 0;
                        }
                    }
                
                }
            }
        }
    }

    void SetupGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            //cria o painel Goal na tela pre game
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform);

            //seta a imagem e o texto 
            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].typeGoals.GetSprite(); ;
            panel.thisString = "0/" + levelGoals[i].numberNeeded;

            //cria o painel Goal no score board
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            gameGoal.transform.SetParent(goalGameParent.transform);

            //seta a imagem e o texto 
            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            panel.thisSprite = levelGoals[i].typeGoals.GetSprite();
            panel.thisString = "0/" + levelGoals[i].numberNeeded;
        }
    }
    // Update is called once per frame
    public void UpdateGoals()
    {
        int goalsCompleted = 0;
        for (int i = 0; i < levelGoals.Length; i ++)
        {
            currentGoals[i].thisText.text = "" + levelGoals[i].numberCollected + "/" + levelGoals[i].numberNeeded;
            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded)
            {
                goalsCompleted ++;
                currentGoals[i].thisText.text = "" + levelGoals[i].numberNeeded + "/" + levelGoals[i].numberNeeded;
            }
        }
        if(goalsCompleted >= levelGoals.Length)
        {
           if (endGame != null)
            {
                endGame.WinGame();

            }
        }
    }

    public void CompareGoal(string goalTocompare)
    {
        for (int i = 0; i< levelGoals.Length; i++)
        {
            if (goalTocompare == levelGoals[i].typeGoals.GetTag())
            {
                levelGoals[i].numberCollected++;
            }
        }
    }
}
