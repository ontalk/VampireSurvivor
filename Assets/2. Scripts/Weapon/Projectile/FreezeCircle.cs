using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeCircle : MonoBehaviour
{
    public int damage = 10;
    public float slowPercentage = 0.2f;
    public float radius = 1f;
    public float duration = 5f;

    private List<EnemyStats> enemiesInRange = new List<EnemyStats>();
    private Coroutine _destroyCoroutine;

    private void Start()
    {
        transform.localScale = Vector3.one * radius * 2; // 원의 지름을 설정
        StartCoroutine(ApplyDamageAndSlow());
        if (duration != float.PositiveInfinity)
        {
            _destroyCoroutine = StartCoroutine(DestroyAfterTime(duration));
        }
    }

    private void Update()
    {
        transform.position = Managers.Game.getPlayer().transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            if (enemyStats != null && !enemiesInRange.Contains(enemyStats))
            {
                enemiesInRange.Add(enemyStats);
                enemyStats.MoveSpeed *= (1 - slowPercentage);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemyStats = other.GetComponent<EnemyStats>();
            if (enemyStats != null && enemiesInRange.Contains(enemyStats))
            {
                enemiesInRange.Remove(enemyStats);
                enemyStats.MoveSpeed /= (1 - slowPercentage);
            }
        }
    }

    private IEnumerator ApplyDamageAndSlow()
    {
        while (true)
        {
            foreach (EnemyStats enemy in enemiesInRange)
            {
                //enemy.OnDamaged(damage);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (EnemyStats enemy in enemiesInRange)
        {
            enemy.MoveSpeed /= (1 - slowPercentage);
        }
        Destroy(gameObject);
    }

    public void UpdateDuration(float newDuration)
    {
        if (_destroyCoroutine != null)
        {
            StopCoroutine(_destroyCoroutine);
        }

        duration = newDuration;
        if (duration != float.PositiveInfinity)
        {
            _destroyCoroutine = StartCoroutine(DestroyAfterTime(duration));
        }
    }
}