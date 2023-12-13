using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmPanel : MonoBehaviour
{
    [Header("Level Information")]
    public string levelToLoad;
    public int level;
    private int highScore;
    private int starsActive;
    private GameData gameData;

    [Header("UI Stuf")]
    public Image[] stars;
    public Text highScoreText;
    public Text starText;
    

    // Start is called before the first frame update
    void OnEnable()
    {
        gameData = FindAnyObjectByType<GameData>();
        LoadData();
        ActivateStars();
        SetText();
    }

    void LoadData()
    {
        if(gameData != null)
        {
            starsActive = gameData.saveData.stars[level - 1];
            highScore = gameData.saveData.highScores[level - 1];
        }
    }
    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsActive + "/3";
    }
    void ActivateStars()
    {
        for (int i = 0; i < starsActive; i++)
        {
            stars[i].enabled = true;
        }
    }

    public void Cancel()
    {
        this.gameObject.SetActive(false);
    }

     public void Play()
    {
        PlayerPrefs.SetInt("Current Level",level - 1);
        SceneManager.LoadScene(levelToLoad);
    }
}
