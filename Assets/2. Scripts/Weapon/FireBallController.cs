using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireballController : WeaponController
{
    public override int _weaponType { get { return (int)Define.Weapons.Fireball; } }

    private bool _isCool = false;
    public int level = 1; // Fireball 레벨
    private Coroutine _spawnCoroutine;
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable(){
        if (!_isCool)
        {
            StartCoroutine(SpawnWeapon());
        }
    }
    private void OnDisable(){
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }
    void Start()
    {
        if (!_isCool)
        {
            StartCoroutine(SpawnWeapon());
        }
    }

    IEnumerator SpawnWeapon()
    {
        while (true)
        {
            _isCool = true;
            int fireballCount = level; // 레벨에 따른 Fireball 개수
            float cooldown = 5f - (level - 1) * 0.5f; // 레벨에 따른 생성 주기
            float damage = 25f * Mathf.Pow(1.8f, level - 1); // 레벨에 따른 데미지

            for (int i = 0; i < fireballCount; i++)
            {
                GameObject go = Managers.Game.Spawn(Define.WorldObject.Weapon, "Weapon/Fireball", transform.position);
                SetWeapon(go, i, fireballCount, damage);
            }

            yield return new WaitForSeconds(cooldown);
            _isCool = false;
        }
    }

    protected void SetWeapon(GameObject weapon, int index, int total, float damage)
    {
        Fireball fireball = weapon.GetOrAddComponent<Fireball>();
        if (level < 5)
        {
            float spreadAngle = 20f; // 분산 각도
            float offsetAngle = (index - (total - 1) / 2f) * spreadAngle;
            fireball._dir = Quaternion.Euler(0, offsetAngle, 0) * transform.forward;
        }
        else
        {
            // 레벨 5일 때 Fireball이 플레이어 주변을 공전하도록 설정
            fireball._dir = Quaternion.Euler(0, index * (360f / total), 0) * Vector3.forward;
            fireball.StartOrbit(transform);
        }

        fireball._damage = (int)damage;
        fireball._speed = _movSpeed;
        fireball._force = _force;
        fireball._size = _size;
        fireball._panatrate = _penetrate;
        fireball.StartCoroutine(fireball.DestroyAfterTime(5f)); // 5초 후에 삭제
    }
    protected override void SetWeaponStat()
    {
        base.SetWeaponStat();
    }
    public void SetLevel(int newLevel)
    {
        Level = newLevel; // WeaponController의 Level 프로퍼티를 통해 레벨 설정
        SetWeaponStat(); // 무기 통계 업데이트
    }

    private void OnValidate()
    {
        if (Application.isPlaying && Managers.Data != null && Managers.Data.WeaponData != null)
        {
            SetLevel(level); // Inspector에서 레벨이 변경될 때 무기 통계 업데이트
        }
    }

}