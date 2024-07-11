using UnityEngine;
using System.Collections;

public class Poison : MonoBehaviour
{
    public int _damage;
    public float _force;
    private float _duration = 5f; // 독 지속 시간

    public void Initialize(int damage, float force, float size)
    {
        _damage = damage;
        _force = force;
        transform.localScale = Vector3.one * size;

        StartCoroutine(DestroyAfterTime());
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            BaseController enemyController = other.GetComponent<BaseController>();
            if (enemyController != null)
            {
                enemyController.OnDamaged(_damage, _force);
            }
        }
    }

    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
    }
}