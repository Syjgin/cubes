using System;
using UnityEngine;
using System.Collections;

public class Hero : MonoBehaviour {

    [SerializeField] private Bullet _bulletPrefab;

    private float _moveDelta;
    private float _rotateDelta;
    private float _jumpVerticalSpeed;
    private float _jumpHorizontalSpeed;
    private Rigidbody _rigidBody;
    private bool _jumpStarted;
    private double _jumpStartTime;
    private float _currentRotation;
    private float _shotInterval;
    private float _shotStartTime;
    private bool _shotStarted;
    private Vector2 _heroPosition;
    private int _lifesCount;

    private const double JumpInterval = 1;
    public const float HeroY = 0.5f;

    void Start ()
	{
        //load static and saved data from data manager
        GameController.GetInstance().OnGameRestarted += GameRestartHandler;
	    _lifesCount = DataManager.GetInstance().GetHeroLifesCount();
	    _moveDelta = DataManager.GetInstance().GetHeroVelocity();
	    _rotateDelta = DataManager.GetInstance().GetHeroAngularVelocity();
	    _jumpVerticalSpeed = DataManager.GetInstance().GetHeroJumpVerticalSpeed();
        _jumpHorizontalSpeed = DataManager.GetInstance().GetHeroJumpHorizontalSpeed();
	    _shotInterval = DataManager.GetInstance().GetHeroShotInterval();
	    _rigidBody = GetComponent<Rigidbody>();
	    _jumpStartTime = Time.time;
        _heroPosition = DataManager.GetInstance().GetHeroPosition();
        transform.position = new Vector3(_heroPosition.x, HeroY, _heroPosition.y);
        _currentRotation = DataManager.GetInstance().GetHeroRotation();
        transform.Rotate(Vector3.up, _currentRotation);
        GameController.GetInstance().SendHeroPositionLoadedEvent();
	}

    private void GameRestartHandler()
    {
        //on restart restore position and lifes count
        _lifesCount = DataManager.GetInstance().GetHeroTotalLifes();
        transform.position = new Vector3(0, HeroY, 0);
    }

    void Update ()
	{
        if (!GameController.GetInstance().IsGameRunning)
            return;
        //if due by physics collision we lost vertical align, assume we are start jumping to restore it
        if (!_jumpStarted && (Math.Abs(transform.position.y - HeroY) > 0.001f ||
                              Math.Abs(transform.rotation.eulerAngles.x) > 1 ||
                              Math.Abs(transform.rotation.eulerAngles.z) > 1))
        {
            _jumpStarted = true;
            _jumpStartTime = Time.time;
        }
        _heroPosition = new Vector2(transform.position.x, transform.position.z);
        _currentRotation = transform.rotation.eulerAngles.y;
	    float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        float currentMoveDelta = _jumpStarted ? verticalMove*_jumpHorizontalSpeed : verticalMove*_moveDelta;
        transform.Translate(new Vector3(horizontalMove*_moveDelta, 0, currentMoveDelta));
        transform.Rotate(Vector3.up, _rotateDelta * Input.GetAxis("Mouse X"));
        if (Input.GetAxis("Fire1") > 0)
        {
            if (!_shotStarted)
            {
                _shotStarted = true;
                Bullet bullet = Instantiate(_bulletPrefab);
                bullet.gameObject.SetActive(false);
                bullet.transform.position = transform.position;
                bullet.transform.rotation = transform.rotation;
                bullet.SetSource(gameObject.transform.position);
                bullet.gameObject.SetActive(true);
                _shotStartTime = Time.time;
            }
        }
        //jumping
	    if (Input.GetAxis("Jump") > 0 && !_jumpStarted)
	    {
	        _jumpStartTime = Time.time;
	        _jumpStarted = true;
            _rigidBody.AddForce(Vector3.up * _jumpVerticalSpeed);   
	    }
        //restore vertical align, if we are on wrong side
        if (Math.Abs(transform.position.y - HeroY) < 0.001f && Time.time - _jumpStartTime > JumpInterval && _jumpStarted)
	    {
	        _jumpStarted = false;
            transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
	    }
	    _currentRotation = transform.rotation.eulerAngles.y;
	    if (Time.time - _shotStartTime > _shotInterval && _shotStarted)
	        _shotStarted = false;
	}

    private void OnDestroy()
    {
        DataManager.GetInstance().SaveHeroRotation(_currentRotation);
        DataManager.GetInstance().SaveHeroPosition(_heroPosition);
        DataManager.GetInstance().SetHeroLifesCount(_lifesCount);
        GameController.GetInstance().OnGameRestarted -= GameRestartHandler;
    }

    private void HandleEnemyAttack()
    {
        //decrease lifes count on enemy attack
        _lifesCount--;
        if(_lifesCount > 0)
            GameController.GetInstance().SendPlayerWoundedEvent();
        else
        {
            GameController.GetInstance().SendPlayerDefeatEvent();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(!GameController.GetInstance().IsGameRunning)
            return;
        Enemy enemy = col.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            HandleEnemyAttack();
        }
    }

    public int GetLifesCount()
    {
        return _lifesCount;
    }
}
