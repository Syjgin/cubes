using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private const string HeroRotationKey = "HeroRotationKey";
    private const string TerrainOffsetKey = "TerrainOffsetKey";
    private const string EnemiesData = "EnemiesData";
    private const string HeroPositionKey = "HeroPositionKey";
    private const string HeroLifesCountKey = "HeroLifesCountKey";
    private const string AliveEnemyCountKey = "AliveEnemyCountKey";
    private const string SpawnerInitializationFinished = "SpawnerInitializationFinished";
    private const string PauseWindowEnabled = "PauseWindowEnabled";
    private const string VictoryWindowEnabled = "VictoryWindowEnabled";
    private const string DefeatWindowEnabled = "DefeatWindowEnabled";
    private const string WoundWindowEnabled = "WoundWindowEnabled";
    private const string TimerValueKey = "TimerValueKey";
    private const string ConfigName = "config";

    private const int MaxEnemiesCountFallback = 50;
    private const float EnemyVelocityFallback = 0.1f;
    private const float EnemyCourseChangeTimeFallback = 3;
    private const float EnemyStopSeekingTimeFallback = 3;
    private const float EnemyAngularVelocityFallback = 1;
    private const float EnemyDirectAttackDistanceFallback = 10;
    private const float EnemyDirectAttackVelocityFallback = 0.2f;

    private const float HeroAngularVelocityFallback = 1;
    private const float HeroJumpHorizontalSpeedFallback = 150;
    private const float HeroJumpVerticalSpeedFallback = 0.2f;
    private const float HeroShotIntervalFallback = 1;
    private const float BulletSpeedFallback = 5;
    private const float BulletDistanceFallback = 25;
    private const int HeroLifesCountFallback = 3;
    private const float HeroVelocityFallback = 0.1f;

    private int _maxEnemiesCount;
    private float _enemyVelocity;
    private float _enemyCourseChangeTime;
    private float _enemyStopSeekingTime;
    private float _enemyAngularVelocity;
    private float _enemyDirectAttackVelocity;
    private float _enemyDirectAttackDistance;

    private float _heroVelocity;
    private float _heroAngularVelocity;
    private float _heroJumpVerticalSpeed;
    private float _heroJumpHorizontalSpeed;
    private float _heroShotInterval;
    private float _bulletSpeed;
    private float _bulletDistance;
    private int _heroLifesCount;

    private static DataManager _instance = null;

    public static DataManager GetInstance()
    {
        if(_instance == null)
            _instance = new DataManager();
        return _instance;
    }

    private DataManager()
    {
        //config reading
        TextAsset config = Resources.Load(ConfigName) as TextAsset;
        if (config != null)
        {
            string unparsedConfig = config.text;
            try
            {
                JSONObject parsedConfig = new JSONObject(unparsedConfig);
                JSONObject heroData = parsedConfig.GetField("Hero");
                JSONObject enemyData = parsedConfig.GetField("Enemies");
                TryParseConfigValue(enemyData, ref _maxEnemiesCount, MaxEnemiesCountFallback, "MaxCount", "enemy");
                TryParseConfigValue(enemyData, ref _enemyVelocity, EnemyVelocityFallback, "Velocity", "enemy");
                TryParseConfigValue(enemyData, ref _enemyCourseChangeTime, EnemyCourseChangeTimeFallback, "CourseChangeTime", "enemy");
                TryParseConfigValue(enemyData, ref _enemyStopSeekingTime, EnemyStopSeekingTimeFallback, "StopSeekingTime", "enemy");
                TryParseConfigValue(enemyData, ref _enemyAngularVelocity, EnemyAngularVelocityFallback, "AngularVelocity", "enemy");
                TryParseConfigValue(enemyData, ref _enemyDirectAttackDistance, EnemyDirectAttackDistanceFallback, "DirectAttackDistance", "enemy");
                TryParseConfigValue(enemyData, ref _enemyDirectAttackVelocity, EnemyDirectAttackVelocityFallback, "DirectAttackVelocity", "enemy");

                TryParseConfigValue(heroData, ref _heroAngularVelocity, HeroAngularVelocityFallback, "AngularVelocity", "hero");
                TryParseConfigValue(heroData, ref _heroJumpVerticalSpeed, HeroJumpVerticalSpeedFallback, "JumpVerticalSpeed", "hero");
                TryParseConfigValue(heroData, ref _heroJumpHorizontalSpeed, HeroJumpHorizontalSpeedFallback, "JumpHorizontalSpeed", "hero");
                TryParseConfigValue(heroData, ref _bulletSpeed, BulletSpeedFallback, "BulletSpeed", "hero");
                TryParseConfigValue(heroData, ref _bulletDistance, BulletDistanceFallback, "BulletDistance", "hero");
                TryParseConfigValue(heroData, ref _heroShotInterval, HeroShotIntervalFallback, "ShotInterval", "hero");
                TryParseConfigValue(heroData, ref _heroLifesCount, HeroLifesCountFallback, "LifesCount", "hero");
                TryParseConfigValue(heroData, ref _heroVelocity, HeroVelocityFallback, "Velocity", "hero"); 
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                WriteFallbacks();
            }
        }
        else
        {
            WriteFallbacks();
        }
    }

    private void WriteFallbacks()
    {
        _maxEnemiesCount = MaxEnemiesCountFallback;
        _enemyVelocity = EnemyVelocityFallback;
        _enemyCourseChangeTime = EnemyCourseChangeTimeFallback;
        _enemyStopSeekingTime = EnemyStopSeekingTimeFallback;
        _enemyAngularVelocity = EnemyAngularVelocityFallback;
        _enemyDirectAttackDistance = EnemyDirectAttackDistanceFallback;
        _enemyDirectAttackVelocity = EnemyDirectAttackVelocityFallback;

        _heroAngularVelocity = HeroAngularVelocityFallback;
        _heroVelocity = HeroVelocityFallback;
        _heroJumpVerticalSpeed = HeroJumpHorizontalSpeedFallback;
        _heroShotInterval = HeroShotIntervalFallback;
        _bulletSpeed = BulletSpeedFallback;
        _heroLifesCount = HeroLifesCountFallback;
        _heroJumpHorizontalSpeed = HeroJumpHorizontalSpeedFallback;
        _bulletDistance = BulletDistanceFallback;
    }

    #region Hero

    public float GetHeroVelocity()
    {
        return _heroVelocity;
    }

    public float GetHeroAngularVelocity()
    {
        return _heroAngularVelocity;
    }

    public float GetHeroJumpVerticalSpeed()
    {
        return _heroJumpVerticalSpeed;
    }

    public float GetHeroJumpHorizontalSpeed()
    {
        return _heroJumpHorizontalSpeed;
    }

    public float GetHeroRotation()
    {
        return PlayerPrefs.GetFloat(HeroRotationKey, 0);
    }

    public void SaveHeroRotation(float rotation)
    {
        PlayerPrefs.SetFloat(HeroRotationKey, rotation);
    }

    public Vector2 GetHeroPosition()
    {
        float offsetX = PlayerPrefs.GetFloat(HeroPositionKey + "x", 0);
        float offsetY = PlayerPrefs.GetFloat(HeroPositionKey + "y", 0);
        return new Vector2(offsetX, offsetY);
    }

    public void SaveHeroPosition(Vector2 position)
    {
        PlayerPrefs.SetFloat(HeroPositionKey + "x", position.x);
        PlayerPrefs.SetFloat(HeroPositionKey + "y", position.y);
    }

    public float GetHeroShotInterval()
    {
        return _heroShotInterval;
    }

    public int GetHeroLifesCount()
    {
        return PlayerPrefs.GetInt(HeroLifesCountKey, _heroLifesCount);
    }

    public void SetHeroLifesCount(int count)
    {
        PlayerPrefs.SetInt(HeroLifesCountKey, count);
    }

    public int GetHeroTotalLifes()
    {
        return _heroLifesCount;
    }

    public float GetBulletSpeed()
    {
        return _bulletSpeed;
    }

    public float GetBulletDistance()
    {
        return _bulletDistance;
    }

    #endregion

    #region Enemy

    public int GetMaxEnemiesCount()
    {
        return _maxEnemiesCount;
    }

    public void SaveEnemiesInfo(string data)
    {
        PlayerPrefs.SetString(EnemiesData, data);
    }

    public string LoadEnemiesInfo()
    {
        return PlayerPrefs.GetString(EnemiesData, "");
    }

    public float GetEnemyVelocity()
    {
        return _enemyVelocity;
    }

    public float GetEnemyCourseChangeTime()
    {
        return _enemyCourseChangeTime;
    }

    public float GetEnemyStopSeekingTime()
    {
        return _enemyStopSeekingTime;
    }

    public float GetEnemyAngularVelocity()
    {
        return _enemyAngularVelocity;
    }

    public void SetAliveEnemyCount(int count)
    {
        PlayerPrefs.SetInt(AliveEnemyCountKey, count);
    }

    public int GetAliveEnemyCount()
    {
        return PlayerPrefs.GetInt(AliveEnemyCountKey, _maxEnemiesCount);
    }

    public bool IsEnemySpawnerInitialized()
    {
        return PlayerPrefs.HasKey(SpawnerInitializationFinished);
    }

    public void SetEnemySpawnerInitialized(bool initialized)
    {
        if (initialized)
            PlayerPrefs.SetInt(SpawnerInitializationFinished, 1);
        else
            PlayerPrefs.DeleteKey(SpawnerInitializationFinished);
    }

    public float GetEnemyDirectAttackDistance()
    {
        return _enemyDirectAttackDistance;
    }

    public float GetEnemyDirectAttackVelocity()
    {
        return _enemyDirectAttackVelocity;
    }

    #endregion

    #region Terrain

    public Vector2 GetTerrainOffset()
    {
        float offsetX = PlayerPrefs.GetFloat(TerrainOffsetKey + "x", 0);
        float offsetY = PlayerPrefs.GetFloat(TerrainOffsetKey + "y", 0);
        return new Vector2(offsetX, offsetY);
    }

    public void SaveTerrainOffset(Vector2 offset)
    {
        PlayerPrefs.SetFloat(TerrainOffsetKey + "x", offset.x);
        PlayerPrefs.SetFloat(TerrainOffsetKey + "y", offset.y);
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    #endregion

    #region GUI

    public void SetPauseWindowEnabled(bool isEnabled)
    {
        if (isEnabled)
            PlayerPrefs.SetInt(PauseWindowEnabled, 1);
        else
            PlayerPrefs.DeleteKey(PauseWindowEnabled);
    }

    public void SetVictoryWindowEnabled(bool isEnabled)
    {
        if (isEnabled)
            PlayerPrefs.SetInt(VictoryWindowEnabled, 1);
        else
            PlayerPrefs.DeleteKey(VictoryWindowEnabled);
    }

    public void SetDefeatWindowEnabled(bool isEnabled)
    {
        if (isEnabled)
            PlayerPrefs.SetInt(DefeatWindowEnabled, 1);
        else
            PlayerPrefs.DeleteKey(DefeatWindowEnabled);
    }

    public void SetWoundWindowEnabled(bool isEnabled)
    {
        if (isEnabled)
            PlayerPrefs.SetInt(WoundWindowEnabled, 1);
        else
            PlayerPrefs.DeleteKey(WoundWindowEnabled);
    }

    public bool IsDefeatWindowEnabled()
    {
        return PlayerPrefs.HasKey(DefeatWindowEnabled);
    }

    public bool IsVictoryWindowEnabled()
    {
        return PlayerPrefs.HasKey(VictoryWindowEnabled);
    }

    public bool IsPauseWindowEnabled()
    {
        return PlayerPrefs.HasKey(PauseWindowEnabled);
    }

    public bool IsWoundWindowEnabled()
    {
        return PlayerPrefs.HasKey(WoundWindowEnabled);
    }


    public float GetTimerValue()
    {
        return PlayerPrefs.GetFloat(TimerValueKey, 0);
    }

    public void SetTimerValue(float timerValue)
    {
        PlayerPrefs.SetFloat(TimerValueKey, timerValue);
    }

#endregion

    private void TryParseConfigValue(JSONObject jsonObject, ref float configValue, float defaultValue, string parameterName, string namespaceName)
    {
        if (!jsonObject.GetField(ref configValue, parameterName, defaultValue))
            Debug.LogError(String.Format("{0} {1} parse failed", namespaceName, parameterName)); 
    }

    private void TryParseConfigValue(JSONObject jsonObject, ref int configValue, int defaultValue, string parameterName, string namespaceName)
    {
        if (!jsonObject.GetField(ref configValue, parameterName, defaultValue))
            Debug.LogError(String.Format("{0} {1} parse failed", namespaceName, parameterName));
    }
}
