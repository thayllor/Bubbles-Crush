using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum GameType
{
    Moves,
    Time
}
[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int couunterValue;
}
public class EndGameManager : MonoBehaviour
{

    private float timerSeconds;
    private Board board;

    public GameObject youWinPanel;
    public GameObject tryAgainPanel;
    public GameObject movesLabel;
    public GameObject timesLabel;
    public TMP_Text counter;
    public EndGameRequirements requirements;
    public int currentCounterValue;

    // Start is called before the first frame update
    void Start()
    {
        board = FindAnyObjectByType<Board>();
        SetGameType();
        SetupGame();
    }

    void SetGameType()
    {
        if(board.world != null)
        {
            if (board.level < board.world.levels.Length)
            { 
                if (board.world.levels[board.level] != null)
                {
                    requirements = board.world.levels[board.level].endGameRequirements;
                }
            }
        }
    }

    void SetupGame()
    {
        currentCounterValue = requirements.couunterValue;
        if(requirements.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
            timesLabel.SetActive(false);
        }
        else
        {
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timesLabel.SetActive(true);
        }
        counter.text = "" + currentCounterValue;
    }

    public void DecreaseCounterValue( )
    {
        if(board.currentState != GameState.pause)
        {
            currentCounterValue --;
            counter.text = "" + currentCounterValue;
            if(currentCounterValue == 0)
            {
                LoseGame();
            }
        } 
    }

    public void WinGame()
    {
        youWinPanel.SetActive(true);
        board.currentState = GameState.win;
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindAnyObjectByType<FadePanelController>();
        fade.GameOver();
    }

    public void LoseGame()
    {
        tryAgainPanel.SetActive(true);
        board.currentState = GameState.lose;
        Debug.Log("You Lose");
        currentCounterValue = 0;
        counter.text = "" + currentCounterValue;
        FadePanelController fade = FindAnyObjectByType<FadePanelController>();
        fade.GameOver();
    }
    // Update is called once per frame
    void Update()
    {
        if(requirements.gameType == GameType.Time && currentCounterValue > 0)
        {
            timerSeconds -= Time.deltaTime;
            if(timerSeconds <= 0)
            {
                DecreaseCounterValue();
                timerSeconds = 1;
            }
        }
    }
}
