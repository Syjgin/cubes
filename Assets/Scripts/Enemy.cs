using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private GameObject _target;
    private bool _isSeeking;
    private float _velocity;
    private float _courseChangeTime;
    private bool _isCourseChanged;
    private float _stopSeekingTime;
    private float _stopSeekingInterval;
    private float _courseChangeInterval;
    private float _angularVelocity;
    private float _directAttackVelocity;
    private float _directAttackDistance;
    
    void Start ()
	{
        //load static data from data manager
	    _velocity = DataManager.GetInstance().GetEnemyVelocity();
	    _courseChangeTime = Time.time;
        _stopSeekingTime = Time.time;
	    _isCourseChanged = false;
        _courseChangeInterval = DataManager.GetInstance().GetEnemyCourseChangeTime();
        _stopSeekingInterval = DataManager.GetInstance().GetEnemyStopSeekingTime();
        _angularVelocity = DataManager.GetInstance().GetEnemyAngularVelocity();
        _directAttackDistance = DataManager.GetInstance().GetEnemyDirectAttackDistance();
        _directAttackVelocity = DataManager.GetInstance().GetEnemyDirectAttackVelocity();
	}

    public void Init(GameObject target, bool isSeeking)
    {
        //load current state related data
        _target = target;
        _isSeeking = isSeeking;
    }

    public bool IsSeeking()
    {
        return _isSeeking;
    }

	void Update () 
    {
        //swing the pendulum, if we are far away from target, directly attack it with increased velocity, if we are near
        if (!GameController.GetInstance().IsGameRunning)
            return;
	    transform.LookAt(_target.transform);
	    float XOffset = _angularVelocity*Mathf.Sin(_velocity);
	    if (Time.time - _courseChangeTime > _courseChangeInterval)
	    {
	        _isCourseChanged = Random.Range(0, 100) > 50;
            _courseChangeTime = Time.time;
	    }
        if (_isCourseChanged)
        {
            XOffset *= -1;
        }
	    if (_isSeeking)
	    {
            if ((transform.position - _target.transform.position).magnitude > _directAttackDistance)
                transform.Translate(new Vector3(XOffset, 0, _velocity));
            else
            {
                if (Mathf.Abs(_target.transform.position.y - transform.position.y) < Hero.HeroY)
                    transform.Translate(new Vector3(0, 0, _directAttackVelocity));
            }
	    }
        else
            transform.Translate(new Vector3(0, 0, -_directAttackVelocity));
	    if (!_isSeeking && Time.time - _stopSeekingTime > _stopSeekingInterval)
	        _isSeeking = true;
    }

    void OnCollisionEnter(Collision col)
    {
        //move away for some period, if target was successfully attacked
        if (!GameController.GetInstance().IsGameRunning)
            return;
        Hero hero = col.gameObject.GetComponent<Hero>();
        if (hero != null && _isSeeking)
        {
            _isSeeking = false;
        }
    }

    void OnTriggerEnter(Collider incoming)
    {
        //destroying on bullet collision
        Bullet bullet = incoming.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            GameController.GetInstance().SendEnemyDestroyedEvent(this);
            Destroy(bullet.gameObject);
            Destroy(gameObject);
        }
    }
}
