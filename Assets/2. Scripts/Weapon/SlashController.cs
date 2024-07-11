using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashController : WeaponController
{
    public override int _weaponType { get { return (int)Define.Weapons.Slash; } }
    public PlayerStats playerStats;

    public GameObject slashPrefab;
    public Transform playerTransform;

    public void Awake() //�̰� ���߿� �����Ұ� -> ui���� Ŭ�������� �÷��̰� �ǵ��� �Ұ���.
    {
        
    }
    public void Start()
    {
        StartCoroutine(PerformSlashAttack());
    }

    public IEnumerator PerformSlashAttack()
    {
        while (true)
        {
            yield return StartCoroutine(CreateAndDestroySlash());
            yield return new WaitForSeconds(1f);
        }
    }

    public List<(Vector3, Quaternion)> GetSlashTransforms()
    {
        List<(Vector3, Quaternion)> transforms = new List<(Vector3, Quaternion)>();
        if (playerTransform != null)
        {
            Vector3 spawnPosition1 = playerTransform.position + playerTransform.up * 3f + playerTransform.forward * 2f;
            Vector3 spawnPosition2 = playerTransform.position + playerTransform.up * 3f + playerTransform.forward * 2f;

            Quaternion rotation1 = playerTransform.rotation * Quaternion.Euler(0, 0, -145);
            Quaternion rotation2 = playerTransform.rotation * Quaternion.Euler(0, 0, -32);

            transforms.Add((spawnPosition1, rotation1));
            transforms.Add((spawnPosition2, rotation2));
        }
        return transforms;
    }

    public IEnumerator CreateAndDestroySlash()
    {
        List<(Vector3, Quaternion)> slashTransforms = GetSlashTransforms();
        foreach (var (position, rotation) in slashTransforms)
        {
            GameObject slashInstance = Object.Instantiate(slashPrefab, position, rotation);
            SetWeapon(slashInstance);
            Object.Destroy(slashInstance, 0.5f);
        }
        yield return null;
    }

    protected void SetWeapon(GameObject weapon)
    {
        Slash slashAttack = weapon.GetComponent<Slash>();
        slashAttack._damage = _damage;
        slashAttack._speed = _movSpeed;
        slashAttack._force = _force;
        slashAttack._panatrate = _penetrate;
    }
}