using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{

    [SerializeField] private GameObject _anchor;
    [SerializeField] private GameObject _terrainTile;

    public const float MaxDistance = 60;
    private const int LimitCoef = 16;

    private Dictionary<Vector3, GameObject> _spawnedTiles;
    private Vector3 _currentTilePos;

    private Vector2 _offset;
    private Vector3 _heroOffset;
    private bool _isInitialized;

    void Awake()
    {
        _spawnedTiles = new Dictionary<Vector3, GameObject>();
        GameController.GetInstance().OnPlayerWounded += PlayerWoundedHandler;
        GameController.GetInstance().OnHeroPositionLoaded += Init;
        GameController.GetInstance().OnGameRestarted += PlayerWoundedHandler;
    }

    private void Init()
    {
        //load terrain offset
        _offset = DataManager.GetInstance().GetTerrainOffset();
        transform.position = new Vector3(_anchor.transform.position.x - _offset.x, 0, _anchor.transform.position.z - _offset.y);
        _heroOffset = transform.position;
        InitTiles(false);
        _isInitialized = true;
    }

    private void PlayerWoundedHandler()
    {
        //reinitialize tiles and place hero to start point
        InitTiles(true);
    }

    private void InitTiles(bool changeHeroPosition)
    {
        if(changeHeroPosition)
            _anchor.transform.position = new Vector3(0, Hero.HeroY, 0);
        _currentTilePos = new Vector3();
        Vector3[] newTileCoords = CalculateNeighbors();

        if (_spawnedTiles.ContainsKey(_currentTilePos))
            _spawnedTiles[_currentTilePos].SetActive(true);
        else
        {
            AddTile(_currentTilePos);
        }
        foreach (var newTileCoord in newTileCoords)
        {
            if (!_spawnedTiles.ContainsKey(newTileCoord))
                AddTile(newTileCoord);
        }
    }

    void OnDestroy()
    {
        //saving terrain info
        DataManager.GetInstance().SaveTerrainOffset(_offset);
        GameController.GetInstance().OnPlayerWounded -= PlayerWoundedHandler;
        GameController.GetInstance().OnHeroPositionLoaded -= Init;
        GameController.GetInstance().OnGameRestarted -= PlayerWoundedHandler;
    }

	void Update () 
    {
        if(!_isInitialized)
            return;
        //if hero is far away from current tile, predict next tile and draw its neighbors
        Vector3 delta = (_anchor.transform.position - _heroOffset) - _currentTilePos;
        _offset = new Vector2(delta.x, delta.z);
        if ((Mathf.Abs(delta.x) > MaxDistance / LimitCoef) || (Mathf.Abs(delta.z) > MaxDistance / LimitCoef))
        {
            _currentTilePos = CalcultateNextAxisPoint(delta);
            List<Vector3> newTileCoords = new List<Vector3>();
            newTileCoords.Add(_currentTilePos);
            newTileCoords.AddRange(CalculateNeighbors());
            foreach (var newTileCoord in newTileCoords)
            {
                if (_spawnedTiles.ContainsKey(newTileCoord))
                    _spawnedTiles[newTileCoord].SetActive(true);
                else
                {
                    AddTile(newTileCoord);
                }
            }
            CalculateInvisibleTiles();
        }
    }

    public Vector3 GetCurrentTilePos()
    {
        return _currentTilePos;
    }

    private void CalculateInvisibleTiles()
    {
        //if tile too far away from camera, set it inactive
        foreach (var spawnedTile in _spawnedTiles)
        {
            Vector3 dist = spawnedTile.Key - _currentTilePos;
            if (dist.magnitude > MaxDistance*LimitCoef)
            {
                spawnedTile.Value.SetActive(false);
            }
        }
    }

    private void AddTile(Vector3 position)
    {
        //spawn new tile
        GameObject spawnedTile = Instantiate(_terrainTile);
        spawnedTile.transform.parent = transform;
        spawnedTile.transform.localPosition = position;
        spawnedTile.transform.localRotation = Quaternion.identity;
        _spawnedTiles.Add(position, spawnedTile);
    }

    private Vector3[] CalculateNeighbors()
    {
        //get tile neighbors
        Vector3[] neighbors = new Vector3[8];
        //north
        neighbors[0] = new Vector3(_currentTilePos.x, 0, _currentTilePos.z + MaxDistance);
        //south
        neighbors[1] = new Vector3(_currentTilePos.x, 0, _currentTilePos.z - MaxDistance);
        //east
        neighbors[2] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z);
        //west
        neighbors[3] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z);
        //north-east
        neighbors[4] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z + MaxDistance);
        //north-west
        neighbors[5] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z + MaxDistance);
        //south-east
        neighbors[6] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z - MaxDistance);
        //south-west
        neighbors[7] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z - MaxDistance);
        return neighbors;
    }

    private Vector3 CalcultateNextAxisPoint(Vector3 delta)
    {
        //returns predicted next tile position
        Vector3[] neighbors = CalculateNeighbors();
        if (delta.z > MaxDistance / LimitCoef && Mathf.Abs(delta.x) < MaxDistance / LimitCoef)
        {
            return neighbors[0];
        }
        if (delta.x > MaxDistance / LimitCoef && Mathf.Abs(delta.z) < MaxDistance / LimitCoef)
        {
            return neighbors[2];
        }
        if (delta.z < -MaxDistance / LimitCoef && Mathf.Abs(delta.x) < MaxDistance / LimitCoef)
        {
            return neighbors[1];
        }
        if (delta.x < -MaxDistance / LimitCoef && Mathf.Abs(delta.z) < MaxDistance / LimitCoef)
        {
            return neighbors[3];
        }

        if (delta.z > MaxDistance / LimitCoef && delta.x > MaxDistance / LimitCoef)
        {
            return neighbors[4];
        }
        if (delta.x > MaxDistance / LimitCoef && delta.z < -MaxDistance / LimitCoef)
        {
            return neighbors[6];
        }
        if (delta.z < -MaxDistance / LimitCoef && delta.x < -MaxDistance / LimitCoef)
        {
            return neighbors[7];
        }
        if (delta.x < -MaxDistance / LimitCoef && delta.z > MaxDistance / LimitCoef)
        {
            return neighbors[5];
        }
        return new Vector3();
    }
}
