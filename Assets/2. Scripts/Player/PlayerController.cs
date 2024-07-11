using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;

public class PlayerController : BaseController
{
    #region Public
    [Header("Skills")]
    [SerializeField] private GameObject slashPrefab;
    public RuntimeAnimatorController[] animeCon;
    public bool _isDamaged = false;
    #endregion

    #region Private
    protected PlayerStats _stat;
    private SlashController playerSkills;
    float _invincibility_time = 0.2f; // �����ð�
    #endregion

    #region Protected Field
    protected override void Init()
    {
        _stat = GetComponent<PlayerStats>();
        _rigid = GetComponent<Rigidbody>();
        _sprite = GetComponent<SpriteRenderer>();
        _anime = GetComponent<Animator>();
        _type = Define.WorldObject.Player;
        /*if (gameObject.GetComponentInChildren<UI_HPBar>() == null)
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform, "UI_HPBar");*/
    }
    #endregion

    #region Public Field
        public void Init(Data.Player playerData)
        {
            _anime.runtimeAnimatorController = animeCon[playerData.id - 1];
            _stat.MaxHP = playerData.maxHp;
            _stat.HP = playerData.maxHp;
            _stat.Damage = playerData.damage;
            _stat.Defense = playerData.defense;
            _stat.MoveSpeed = playerData.moveSpeed;
            _stat.Cooldown = playerData.coolDown;
            _stat.Amount = playerData.amount;
            _stat.AddOrSetWeaponDict((Define.Weapons)playerData.weaponID, 1);
        }
        public void OnDamaged(Collider other)
        {
            Stat EnemyStat = other.gameObject.GetComponent<EnemyStats>();
            //_stat.HP -= Mathf.Max(EnemyStat.Damage - _stat.Defense, 1);

            if (_stat.HP <= 0)
                OnDead();
        }
        public override void OnDamaged(int damage, float force = 0)
        {

            //Managers.Event.PlayHitPlayerEffectSound();
            _stat.HP -= Mathf.Max(damage - _stat.Defense, 1);
            OnDead();
        }

        IEnumerator OnDamagedColor()
        {
            _isDamaged = true;
            _sprite.color = Color.red;

            yield return new WaitForSeconds(_invincibility_time);

            _isDamaged = false;
            _sprite.color = Color.white;
        }


    public override void OnDead()
    {
        if (_stat.HP < 0)
        {
            _anime.SetTrigger("isDead");
            _stat.HP = 0;
            //Managers.UI.ShowPopupUI<UI_GameOver>();
            //Managers.GamePause();
        }

    }
    #endregion

    #region Private Field

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EnemyWeapon" || other.gameObject.tag == "Enemy")
        {
            float currentTime = Managers.GameTime;
            if (!_isDamaged)
            {
                OnDamaged(other);
                StartCoroutine(OnDamagedColor());
            }
        }
    }


    #endregion
}


