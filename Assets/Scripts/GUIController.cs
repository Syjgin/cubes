using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    [SerializeField] private Text _statusMessage;
	[SerializeField] private Text _heroLifesCount;
    [SerializeField] private Text _aliveEnemyCountText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Hero _hero;
    [SerializeField] private Text _timer;
    [SerializeField] private Text _killedEnemyCountText;

    private const string Defeat = "Defeat!";
    private const string Victory = "Victory!";
    private const string Pause = "Pause";
    private const string Wounded = "{0} lifes left";
    private const string KilledCount = "{0} red cubes killed";
    private const float MinimalPauseInterval = 1;

    private int _aliveEnemyCount;
    private float _pauseMenuShowTime;
    private float _timerValue;

	void Start ()
	{
        //load previously opened window data from data manager
        _aliveEnemyCount = DataManager.GetInstance().GetAliveEnemyCount();
        _heroLifesCount.text = _hero.GetLifesCount().ToString();
	    _aliveEnemyCountText.text = _aliveEnemyCount.ToString();
        GameController.GetInstance().OnPlayerWounded += ShowWoundWindow;
        GameController.GetInstance().OnEnemyDestroyed += EnemyDestroyedHandler;
        GameController.GetInstance().OnPlayerDefeat += ShowDefeatWindow;
        GameController.GetInstance().OnPlayerVictory += ShowVictoryWindow;
        
        _restartButton.onClick.AddListener(Restart);
        _playButton.onClick.AddListener(Play);
        _pauseMenuShowTime = Time.unscaledTime - MinimalPauseInterval;
	    if (DataManager.GetInstance().IsDefeatWindowEnabled())
	    {
            ShowDefeatWindow();   
	    }
	    if (DataManager.GetInstance().IsVictoryWindowEnabled())
	    {
            ShowVictoryWindow();
	    }
	    if (DataManager.GetInstance().IsPauseWindowEnabled())
	    {
            ShowPauseWindow();
	    }
        if (DataManager.GetInstance().IsWoundWindowEnabled())
            ShowWoundWindow();
	    _timerValue = DataManager.GetInstance().GetTimerValue();
	}

    private void ShowPauseWindow()
    {
        Time.timeScale = 0;
        _playButton.gameObject.SetActive(true);
        GameController.GetInstance().SetGameRunning(false);
        DataManager.GetInstance().SetPauseWindowEnabled(true);
        ShowWindow(Pause, false);
    }

    private void ShowDefeatWindow()
    {
        GameController.GetInstance().SetGameFinished(true);
        _playButton.gameObject.SetActive(false);
        DataManager.GetInstance().SetDefeatWindowEnabled(true);
        _heroLifesCount.text = "0";
        ShowWindow(Defeat, true);
    }

    private void ShowVictoryWindow()
    {
        GameController.GetInstance().SetGameFinished(true);
        _playButton.gameObject.SetActive(false);
        _killedEnemyCountText.gameObject.SetActive(true);
        _killedEnemyCountText.text = string.Format(KilledCount, DataManager.GetInstance().GetMaxEnemiesCount());
        DataManager.GetInstance().SetVictoryWindowEnabled(true);
        ShowWindow(Victory, true);
    }

    private void ShowWindow(string message, bool reinitializeEnemySpawner)
    {
        if(reinitializeEnemySpawner)
            DataManager.GetInstance().SetEnemySpawnerInitialized(false);
        GameController.GetInstance().SetGameRunning(false);
        _statusMessage.text = message;
        _statusMessage.gameObject.SetActive(true);
        _restartButton.gameObject.SetActive(true);
    }

    private void EnemyDestroyedHandler(Enemy enemy)
    {
        //decrease enemy alive count
        _aliveEnemyCount--;
        _aliveEnemyCountText.text = _aliveEnemyCount.ToString();
    }

    private void OnDestroy()
    {
        DataManager.GetInstance().SetAliveEnemyCount(_aliveEnemyCount);
        DataManager.GetInstance().SetTimerValue(_timerValue);
        GameController.GetInstance().OnPlayerWounded -= ShowWoundWindow;
        GameController.GetInstance().OnEnemyDestroyed -= EnemyDestroyedHandler;
        GameController.GetInstance().OnPlayerDefeat -= ShowDefeatWindow;
        GameController.GetInstance().OnPlayerVictory -= ShowVictoryWindow;
        _restartButton.onClick.RemoveListener(Restart);
        _playButton.onClick.RemoveListener(Play);
    }

    private void ShowWoundWindow()
    {
        Time.timeScale = 0;
        _playButton.gameObject.SetActive(true);
        _heroLifesCount.text = _hero.GetLifesCount().ToString();
        GameController.GetInstance().SetGameRunning(false);
        DataManager.GetInstance().SetWoundWindowEnabled(true);
        string message = string.Format(Wounded, _hero.GetLifesCount());
        ShowWindow(message, false);
    }

    void Update ()
    {
        if (GameController.GetInstance().IsGameRunning)
            _timerValue += Time.deltaTime;
        if (Input.GetKey(KeyCode.Escape) && Time.unscaledTime - _pauseMenuShowTime > MinimalPauseInterval)
            OnPauseClick();
        TimeSpan gameTimeInterval = TimeSpan.FromSeconds(_timerValue);
        _timer.text = string.Format("{0:00}:{1:00}:{2:00}", gameTimeInterval.Hours, gameTimeInterval.Minutes,
            gameTimeInterval.Seconds);
    }

    private void OnPauseClick()
    {
        _pauseMenuShowTime = Time.unscaledTime;
        if (!GameController.GetInstance().IsGameRunning)
        {
            if (!GameController.GetInstance().IsGameFinished)
            {
                HideMenuObjects();
                DataManager.GetInstance().SetPauseWindowEnabled(false);
                DataManager.GetInstance().SetWoundWindowEnabled(false);
                GameController.GetInstance().SetGameRunning(true);   
            }
            else
            {
                Restart();
            }
        }
        else
        {
            ShowPauseWindow();  
        }
    }

    private void Restart()
    {
        DataManager.GetInstance().DeleteAll();
        _heroLifesCount.text = DataManager.GetInstance().GetHeroTotalLifes().ToString();
        _aliveEnemyCount = DataManager.GetInstance().GetMaxEnemiesCount();
        _aliveEnemyCountText.text = _aliveEnemyCount.ToString();
        _timerValue = 0;
        GameController.GetInstance().SendGameRestartedEvent();
        HideMenuObjects();
    }

    private void Play()
    {
        DataManager.GetInstance().SetWoundWindowEnabled(false);
        HideMenuObjects();
        DataManager.GetInstance().SetPauseWindowEnabled(false);
        GameController.GetInstance().SetGameRunning(true);
    }

    private void HideMenuObjects()
    {
        Time.timeScale = 1;
        _statusMessage.gameObject.SetActive(false);
        _restartButton.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _killedEnemyCountText.gameObject.SetActive(false);
    }
}
