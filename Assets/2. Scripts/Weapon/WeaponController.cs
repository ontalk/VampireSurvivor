using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;

public abstract class WeaponController : MonoBehaviour
{
    protected GameObject _player = null;
    private PlayerStats _playerStat;
    private Dictionary<int, Data.WeaponData> _weaponData;
    private Dictionary<int, Data.WeaponLevelData> _weaponStat;
    public Define.WorldObject _type = Define.WorldObject.Weapon;
    private Animator _anime;


    public abstract int _weaponType { get; }
    private int _level = 1;
    public int Level
    {
        get
        {
            Debug.Log($"Get level : {_level}");
            return _level;
        }
        set
        {
            Debug.Log($"Set level : {_level}");
            _level = value;
            SetWeaponStat();
        }
    }
    public int _damage = 1;
    public float _movSpeed = 1;
    public float _force = 1;
    public float _cooldown = 1;
    public float _size = 1;
    public int _penetrate = 1;
    public int _countPerCreate = 1;



    protected virtual void Awake()
    {
        _player = Managers.Game.getPlayer();
        _playerStat = _player.GetComponent<PlayerStats>();
        _weaponData = Managers.Data.WeaponData;
        _anime = transform.GetComponent<Animator>();
        _weaponStat = MakeLevelDataDict(_weaponType);
        SetWeaponStat();
    }


    protected virtual void SetWeaponStat()
    {
        if (_level > 5)
            _level = 5;

        Data.WeaponLevelData levelData = _weaponStat[_level];
        _damage = (int)(levelData.damage * ((float)(100 + _playerStat.Damage) / 100f));
        _movSpeed = levelData.movSpeed;
        _force = levelData.force;
        _cooldown = levelData.cooldown * (100f / (100f + _playerStat.Cooldown));
        _size = levelData.size;
        _penetrate = levelData.penetrate;
        _countPerCreate = levelData.countPerCreate + _playerStat.Amount;
    }

    protected Dictionary<int, Data.WeaponLevelData> MakeLevelDataDict(int weaponID)
    {
        Dictionary<int, Data.WeaponLevelData> _weaponLevelData = new Dictionary<int, Data.WeaponLevelData>();
        if (_weaponData.TryGetValue(weaponID, out Data.WeaponData weaponData))
        {
            foreach (Data.WeaponLevelData weaponLevelData in weaponData.weaponLevelData)
            {
                _weaponLevelData.Add(weaponLevelData.level, weaponLevelData);
            }
        }

        return _weaponLevelData;

    }



}