using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private float _speed;
    private float _distance;
    private Vector3 _source;

    //fly start point
    public void SetSource(Vector3 src)
    {
        _source = src;
    }

	void Start ()
	{
	    _speed = DataManager.GetInstance().GetBulletSpeed();
	    _distance = DataManager.GetInstance().GetBulletDistance();
	}
	
	void Update () 
    {
        //if bullet max distance reached, destroy
	    transform.Translate(new Vector3(0, 0, _speed));
        if ((_source - transform.position).magnitude > _distance)
            Destroy(gameObject);
	}
}
