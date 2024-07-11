using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class PoisonController : WeaponController
{
    public override int _weaponType { get { return (int)Define.Weapons.Poison; } }

    private bool _isCool = false;
    public int level = 1;

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
            int potionCount = (level < 5) ? level : 8; // 레벨에 따른 PoisonPotion 개수
            float cooldown = 5f; // 5초마다 생성

            for (int i = 0; i < potionCount; i++)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 1f; // 캐릭터 머리 위에서 생성
                GameObject go = Managers.Game.Spawn(Define.WorldObject.Weapon, "Weapon/PoisonPotion", spawnPosition);
                SetWeapon(go, i, potionCount);
            }

            yield return new WaitForSeconds(cooldown);
            _isCool = false;
        }
    }

    protected void SetWeapon(GameObject weapon, int index, int total)
    {
        PoisonPotion potion = Util.GetOrAddComponent<PoisonPotion>(weapon);
    
        float spreadAngle = 45f; // 더 넓은 분산 각도
        float offsetAngle = (index - (total - 1) / 2f) * spreadAngle;
        Vector3 direction = Quaternion.Euler(0, offsetAngle, 0) * transform.forward;
        
        potion.Initialize(direction, _damage, _movSpeed, _force, _size);
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