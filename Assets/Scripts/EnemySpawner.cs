using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private GameObject _hero;
    [SerializeField] private Enemy _enemyPrefab;

    private int _maxEnemiesCount;
    private List<Enemy> _enemies;
    private JSONObject _enemyData;

    private void Awake()
    {
        GameController.GetInstance().OnEnemyDestroyed += EnemyDestroyedHandler;
        GameController.GetInstance().OnGameRestarted += GameRestartHandler;
    }

    private void GameRestartHandler()
    {
        //rearrange enemies on game restart
        foreach (var enemy in _enemies)
        {
            Destroy(enemy.gameObject);
        }
        _enemies.Clear();
        for (int i = 0; i < _maxEnemiesCount; i++)
        {
            SpawnEnemy();
        }
        DataManager.GetInstance().SetEnemySpawnerInitialized(true);
    }

    private void EnemyDestroyedHandler(Enemy enemy)
    {
        _enemies.Remove(enemy);
    }

    void Start ()
    {
        //if enemy spawner was initialized, load enemies info from json. Otherwise spawn appropriate count of enemies on the map
        Random.seed = Convert.ToInt32(DateTime.UtcNow.Ticks % 100);
	    _maxEnemiesCount = DataManager.GetInstance().GetMaxEnemiesCount();
        _enemies = new List<Enemy>();
        string enemyData = DataManager.GetInstance().LoadEnemiesInfo();
        _enemyData = new JSONObject(enemyData);
        if (DataManager.GetInstance().IsEnemySpawnerInitialized())
        {
            if (enemyData != default(string))
            {
                for (int i = 0; i < _maxEnemiesCount; i++)
                {
                    JSONObject enemy = _enemyData.GetField("Enemy" + i);
                    if (enemy != null)
                    {
                        float rotation = 0;
                        if (!enemy.GetField(ref rotation, "Rotation"))
                            Debug.LogError("enemy rotation parse failed");
                        float coordX = 0;
                        if (!enemy.GetField(ref coordX, "XCoord"))
                            Debug.LogError("enemy x coordinate parse failed");
                        float coordY = 0;
                        if (!enemy.GetField(ref coordY, "YCoord"))
                            Debug.LogError("enemy y coordinate parse failed");
                        bool seeking = true;
                        if (!enemy.GetField(ref seeking, "Seeking"))
                            Debug.LogError("enemy seeking parse failed");
                        Enemy instantiated = Instantiate(_enemyPrefab);
                        instantiated.transform.parent = transform;
                        instantiated.transform.position = new Vector3(coordX, Hero.HeroY, coordY);
                        instantiated.transform.Rotate(Vector3.up, rotation);
                        instantiated.Init(_hero, seeking);
                        _enemies.Add(instantiated);
                    }
                }
            }   
        }
        else
        {
            for (int i = 0; i < _maxEnemiesCount; i++)
            {
                SpawnEnemy();
            }
            DataManager.GetInstance().SetEnemySpawnerInitialized(true);
        }
	}
	
	void Update () 
    {
        if(!GameController.GetInstance().IsGameRunning)
            return;
        if(_enemies.Count == 0)
            GameController.GetInstance().SendPlayerVictoryEvent();
	}

    private void OnDestroy()
    {
        //save enemy data to json
        GameController.GetInstance().OnEnemyDestroyed -= EnemyDestroyedHandler;
        GameController.GetInstance().OnGameRestarted -= GameRestartHandler;
        int i = 0;
        foreach (var enemy in _enemies)
        {
            SaveEnemyToJSON(enemy, i);
            i++;
        }
        _enemyData.Bake();
        DataManager.GetInstance().SaveEnemiesInfo(_enemyData.str);
    }

    private void SpawnEnemy()
    {
        //create randomly placed enemy on map
        Enemy instantiated = Instantiate(_enemyPrefab);
        instantiated.transform.parent = transform;
        instantiated.transform.position = new Vector3(
            Random.Range(_hero.transform.position.x - TerrainManager.MaxDistance, _hero.transform.position.x + TerrainManager.MaxDistance), Hero.HeroY,
            Random.Range(_hero.transform.position.z - TerrainManager.MaxDistance, _hero.transform.position.z + TerrainManager.MaxDistance));
        _enemies.Add(instantiated);
        instantiated.Init(_hero, true);
    }

    private void SaveEnemyToJSON(Enemy instantiated, int count)
    {
        JSONObject objToAdd = new JSONObject();
        objToAdd.AddField("Rotation", instantiated.transform.rotation.eulerAngles.y);
        objToAdd.AddField("XCoord", instantiated.transform.position.x);
        objToAdd.AddField("YCoord", instantiated.transform.position.z);
        objToAdd.AddField("Seeking", instantiated.IsSeeking());
        _enemyData.AddField("Enemy" + count, objToAdd);
    }
}
