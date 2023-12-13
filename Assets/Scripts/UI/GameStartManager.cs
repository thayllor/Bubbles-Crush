using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject levelSelectManagerObject;
    private LevelSelectManager levelSelectManager;
    private GameData gameData;
    private bool gameAlreadyRunning;
    // Start is called before the first frame update

    void Awake()
    {
        levelSelectManager = levelSelectManagerObject.GetComponent<LevelSelectManager>();
        gameData = FindAnyObjectByType<GameData>();
        gameAlreadyRunning = gameData.alreadyRunning;
    }

    private void Start()
    {
        if (gameAlreadyRunning)
        {
            PlayGame();
        }
        else
        {
            Home();
        }
    }
    // Update is called once per frame
    public void PlayGame()
    {
        startPanel.SetActive(false);
        levelSelectManager.ActiveLevelPanels(true);
    }
    public void Home()
    {
        startPanel.SetActive(true);
        levelSelectManager.ActiveLevelPanels(false);
    }
}
