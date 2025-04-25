using HLH.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VisualNovel.Mechanics;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _resetGameButton;
    [SerializeField] private Button _easyAttackButton;
    [SerializeField] private List<Transform> _spawnLocations;
    [SerializeField] private Sprite _checkboxCheckSprite;
    [SerializeField] private Sprite _checkboxUncheckSprite;
    private float _transitionColorAlpha = 0f;
    private bool _isGamePaused = false;
    private bool _isGameOver = false;
    public GameState State { get; private set; }

    public static GameManager Instance { get; private set; }

    public Action OnGameEnterBattleState;
    public bool IsEasyAttack = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ReturnToGame();
        State = GameState.Combat;

        int gameProgress = PlayerPrefs.GetInt("GameProgress");
        IsEasyAttack = PlayerPrefs.GetInt("IsEasyAttack") == 1 ? true : false;
        _easyAttackButton.GetComponent<Image>().sprite = IsEasyAttack ? _checkboxCheckSprite : _checkboxUncheckSprite;

        PlayerController.Instance.transform.position = _spawnLocations[gameProgress].transform.position;
    }

    private void Update()
    {
        if (!_isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseGame();
            }
        }
    }

    public void TogglePauseGame()
    {
        if (!_isGamePaused)
        {
            PauseGame();
        }
        else
        {
            ReturnToGame();
        }
    }

    public void ReturnToGame()
    {
        _isGamePaused = false;
        UnfreezeTime();
        _returnButton.gameObject.SetActive(false);
        _restartButton.gameObject.SetActive(false);
        _resetGameButton.gameObject.SetActive(false);
        _easyAttackButton.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        if (!_isGameOver)
        {
            _isGamePaused = true;
            FreezeTime();
            _returnButton.gameObject.SetActive(true);
            _restartButton.gameObject.SetActive(true);
            _resetGameButton.gameObject.SetActive(true);
            _easyAttackButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        _isGameOver = true;
        DialogueSystem.Instance.FadeOutOnGameOver();
    }

    public void FreezeTime()
    {
        Time.timeScale = 0f;
    }

    public void UnfreezeTime()
    {
        Time.timeScale = 1f;
    }

    public void EnterGameState(GameState state)
    {
        State = state;
        if (state == GameState.Combat)
        {
            OnGameEnterBattleState?.Invoke();
        }
    }

    public void ResetGameProcess()
    {
        PlayerPrefs.SetInt("GameProgress", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleEasyAttackButton()
    {
        IsEasyAttack = !IsEasyAttack;
        if (IsEasyAttack)
        {
            _easyAttackButton.GetComponent<Image>().sprite = _checkboxCheckSprite;
            PlayerPrefs.SetInt("IsEasyAttack", 1);
        }
        else
        {
            _easyAttackButton.GetComponent<Image>().sprite = _checkboxUncheckSprite;
            PlayerPrefs.SetInt("IsEasyAttack", 0);
        }
    }
}

public enum GameState
{
    Narration, Combat
}
