using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject panelPauseScreen;
    public bool paused = false;
    public Button soundButton;
    public Sprite musicOffSprite;
    public Sprite musicOnSprite;


    private Board board;
    private SoundManager soundManager;
    // Start is called before the first frame update
    void Start()
    {
        soundManager = FindAnyObjectByType<SoundManager>();
        board = FindAnyObjectByType<Board>();
        pausePanel.SetActive(false);
        panelPauseScreen.SetActive(false);
        if (PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.image.sprite = musicOffSprite;
            }
            else
            {
                soundButton.image.sprite = musicOnSprite;
            }
        }
        else
        {
            soundButton.image.sprite = musicOnSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (paused && !pausePanel.activeInHierarchy)
        {
            panelPauseScreen.SetActive(true);
            pausePanel.SetActive(true);
            board.currentState = GameState.pause;

        }
        if (!paused && pausePanel.activeInHierarchy)
        {
            panelPauseScreen.SetActive(false);
            pausePanel.SetActive(false);
            board.currentState = GameState.move;

        }

    }
    public void PauseGame()
    {
        paused = !paused;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Splash");
    }

    public void SoundButton()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.image.sprite = musicOnSprite;
                PlayerPrefs.SetInt("Sound", 1);
                soundManager.adjustVolume();
            }
            else
            {
                soundButton.image.sprite = musicOffSprite;
                PlayerPrefs.SetInt("Sound", 0);
                soundManager.adjustVolume();
            }
        }
        else
        {
            soundButton.image.sprite = musicOffSprite;
            PlayerPrefs.SetInt("Sound", 1);
            soundManager.adjustVolume();
        }
    }
}
