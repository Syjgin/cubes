
public class GameController
{
    public static GameController GetInstance()
    {
        if(_instance == null)
            _instance = new GameController();
        return _instance;
    }

    private static GameController _instance;

    private GameController()
    {
        IsGameRunning = true;
    }

    //restart

    public delegate void GameRestartAction();

    public event GameRestartAction OnGameRestarted;

    public void SendGameRestartedEvent()
    {
        IsGameRunning = true;
        IsGameFinished = false;
        if (OnGameRestarted != null)
            OnGameRestarted();
    }

    //wound

    public delegate void PlayerWoundedAction();

    public event PlayerWoundedAction OnPlayerWounded;

    public void SendPlayerWoundedEvent()
    {
        if (OnPlayerWounded != null)
            OnPlayerWounded();
    }


    //defeat

    public delegate void PlayerDefeatAction();

    public event PlayerDefeatAction OnPlayerDefeat;

    public void SendPlayerDefeatEvent()
    {
        IsGameRunning = false;
        IsGameFinished = true;
        if (OnPlayerDefeat != null)
            OnPlayerDefeat();
    }

    //victory

    public delegate void PlayerVictoryAction();

    public event PlayerVictoryAction OnPlayerVictory;

    public void SendPlayerVictoryEvent()
    {
        IsGameRunning = false;
        IsGameFinished = true;
        if (OnPlayerVictory != null)
            OnPlayerVictory();
    }

    //enemy destroyed

    public delegate void EnemyDestroyedAction(Enemy enemy);

    public event EnemyDestroyedAction OnEnemyDestroyed;

    public void SendEnemyDestroyedEvent(Enemy enemy)
    {
        if (OnEnemyDestroyed != null)
            OnEnemyDestroyed(enemy);
    }

    //hero position loaded

    public delegate void HeroPositionLoadedAction();

    public event HeroPositionLoadedAction OnHeroPositionLoaded;

    public void SendHeroPositionLoadedEvent()
    {
        if (OnHeroPositionLoaded != null)
            OnHeroPositionLoaded();
    }

    //pause

    public void SetGameRunning(bool running)
    {
        IsGameRunning = running;
    }

    //defeat or victory

    public void SetGameFinished(bool finished)
    {
        IsGameFinished = finished;
    }

    public bool IsGameRunning { get; private set; }
    public bool IsGameFinished { get; private set; }
}
