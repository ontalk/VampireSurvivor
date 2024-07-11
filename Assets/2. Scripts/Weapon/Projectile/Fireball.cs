using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{

    public Vector3 _dir = new Vector3(1, 0, 0);


    [SerializeField]
    private float _lifeTime = 2f;
    [SerializeField]
    private float _createTime = 0f;

    public int _damage = 10;
    public float _speed = 20f;
    public float _force = 0f;
    public float _size = 1f;
    public int _panatrate = 1;
    private int _piercing = 0;
    private Transform _orbitTarget;

     private float _orbitSpeed = 50f; // 공전 속도

    private void OnEnable()
    {
        _createTime = Managers.GameTime;
    }

    void Update()
    {
        if (_orbitTarget != null)
        {
            transform.RotateAround(_orbitTarget.position, Vector3.up, _orbitSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += _dir * (_speed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (Managers.GameTime - _createTime > _lifeTime)
        {
            DoExplosion();
        }
        OnMove();
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        if (go.CompareTag("Enemy"))
            {
                go.GetComponent<BaseController>().OnDamaged(_damage, _force / 2);
                _piercing++;
                if (_piercing >= _panatrate)
                {
                    DoExplosion();
                }
            }
    }
    void DoExplosion()
    {/*
        Managers.Sound.Play("Explosion_02");
        GameObject explosion = Managers.Game.Spawn(Define.WorldObject.Weapon, "Weapon/Explosion", transform.position);
        explosion.transform.localScale = Vector3.one * _size;
        Explosion explosionStat = explosion.FindChild<Explosion>();
        explosionStat._damage = (int)(_damage * 1.1);
        explosionStat._force = _force;
        Managers.Resource.Destroy(gameObject);*/
    }

    void OnMove()
    {
        float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        transform.position += _dir * (_speed * Time.fixedDeltaTime);
    }

    public void StartOrbit(Transform target)
    {
        _orbitTarget = target;
    }
    public IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public void InitializeFromController(WeaponController controller)
    {
        _damage = controller._damage;
        _speed = controller._movSpeed;
        _force = controller._force;
        _size = controller._size;
        _panatrate = controller._penetrate;
    }
}