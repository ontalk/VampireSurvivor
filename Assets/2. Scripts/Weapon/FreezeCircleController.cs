using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class FreezeCircleController : WeaponController
{
    public override int _weaponType { get { return (int)Define.Weapons.FreezeCircle; } }

    private bool _isCool = false;
    private Coroutine _spawnCoroutine;
    private GameObject _currentFreezeCircle;
public int level = 1;
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (!_isCool)
        {
            StartCoroutine(SpawnWeapon());
        }
    }

    private void OnDisable()
    {
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

            if (_currentFreezeCircle != null)
            {
                Destroy(_currentFreezeCircle);
            }

            _currentFreezeCircle = Managers.Game.Spawn(Define.WorldObject.Weapon, "Weapon/FreezeCircle", transform.position);
            SetWeapon(_currentFreezeCircle);

            if (Level > 5)
            {
                yield break; // 5레벨 이상일 때 코루틴 종료
            }

            yield return new WaitForSeconds(5f); // 5초마다 생성
            _isCool = false;
        }
    }

    protected void SetWeapon(GameObject weapon)
    {
        FreezeCircle freezeCircle = Util.GetOrAddComponent<FreezeCircle>(weapon);
        freezeCircle.damage = _damage;
        freezeCircle.slowPercentage = 0.2f + (Level - 1) * 0.05f; // 레벨에 따른 슬로우 효과 증가
        freezeCircle.radius = _size;
        freezeCircle.duration = Level >= 5 ? float.PositiveInfinity : _cooldown;
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
            SetLevel(Level);
        }
    }
}