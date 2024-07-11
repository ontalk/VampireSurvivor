using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    [SerializeField]
    private float _lifeTime = 3f;
    [SerializeField]
    private float _createTime = 0f;

    public int _damage = 10;
    public float _speed = 10f;
    public float _force = 0f;
    public int _panatrate = 1;
    private int _piercing = 0;

    private void OnEnable()
    {
        _createTime = Managers.GameTime;
    }

    void FixedUpdate()
    {
        if (Managers.GameTime - _createTime > _lifeTime)
        {
            Managers.Resource.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag("Enemy"))
        {
            go.GetComponent<BaseController>().OnDamaged(_damage, _force);
            _piercing++;
            if (_piercing >= _panatrate) { }
                //Managers.Resource.Destroy(gameObject);
        }
    }
}