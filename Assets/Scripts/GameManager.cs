using HLH.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _resetGameButton;
    [SerializeField] private List<Transform> _spawnLocations;
    private float _transitionColorAlpha = 0f;
    private bool _isGamePaused = false;
    private bool _isGameOver = false;
    public GameState State { get; private set; }

    public static GameManager Instance { get; private set; }

    public Action OnGameEnterBattleState;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ReturnToGame();
        State = GameState.Combat;

        int gameProgress = PlayerPrefs.GetInt("GameProgress");
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
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        FreezeTime();
        _isGameOver = true;
        _restartButton.gameObject.SetActive(true);
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
}

public enum GameState
{
    Narration, Combat
}
