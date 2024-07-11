using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : BaseController
{
    #region public variable
    [Header("Enemy")]
    public GameObject hudDamageText;
    public RuntimeAnimatorController[] _animeCon;
    public Transform _target;
    public float obstacleAvoidanceDistance = 2f;
    public float obstacleAvoidanceForce = 10f;
    public float attackDistance;
    public float stopDistance;
    public BoxCollider Area;
    #endregion

    #region private variable
    BoxCollider boxCollider;
    EnemyStats enemyStats;
    Animator anim;
    bool _isLive = true;
    bool _isRange = false;
    bool _isAttack = false;
    bool _isTracking = true;
    #endregion

    #region protected Variable
    protected EnemyStats _stat;
    #endregion

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        _stat = GetComponent<EnemyStats>();
        _rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Init();
    }
    protected override void Init()
    {
        _stat = GetComponent<EnemyStats>();
        _rigid = GetComponent<Rigidbody>();
        _sprite = GetComponent<SpriteRenderer>();
        _anime = GetComponent<Animator>();
        _type = Define.WorldObject.Enemy;
        _target = Managers.Game.getPlayer().GetComponent<Transform>();
    }
    public static event Action OnEnemyDestroyed;

    private void OnDestroy()
    {
        OnEnemyDestroyed?.Invoke();
    }    private void Update()
    {
    
    }
    private void FixedUpdate()
    {
        if (!_isLive)
            return;

        OnMove();
        if (_isRange && !_isAttack)
        {
            StartCoroutine(RangeAttack());
        }
    }


    #region public Field
    public override void OnDamaged(int damage, float force = 0)
    {

            //Managers.Event.PlayHitEnemyEffectSound();
            _anime.SetTrigger("Damage");
            int calculateDamage = Mathf.Max(damage - _stat.Defense, 1);
            _stat.HP -= calculateDamage;
            _rigid.AddForce((_rigid.position - _target.position).normalized * (force * 200f));
            FloatDamageText(calculateDamage);
            OnDead();
    }

    public override void OnDead()
    {
        if (_stat.HP <= 0)
        {
            _isLive = false;
            _stat.HP = 0;

            SpawnExp();
            //Managers.Event.DropItem(_stat, transform);
            transform.localScale = Vector3.one;
            Managers.Game.Despawn(gameObject);
            Destroy(gameObject);
        }
    }
    public EnemyStats GetEnemyStats()
    {
        return enemyStats;
    }

    public void Init(Data.Monster monsterStat, int level, Define.MonsterType type)
    {
        // if (monsterStat.name != gameObject.name)
        //     return;

        int mul = 1;
        switch (type)
        {
            case Define.MonsterType.Enemy:
                mul = 1;
                break;
            case Define.MonsterType.middleBoss:
                mul = 50;
                break;
        }

        // 배열의 길이를 확인하고, 인덱스가 유효한지 확인합니다.
        if (monsterStat.id - 1 >= 0 && monsterStat.id - 1 < _animeCon.Length)
            _anime.runtimeAnimatorController = _animeCon[monsterStat.id - 1];
       

        if (monsterStat.id == 5)
            _isRange = true;

        if (type == Define.MonsterType.middleBoss)
            transform.localScale = Vector3.one * 2;

        try
        {
            _stat.MonsterStyle = (Define.MonsterStyle)System.Enum.Parse(typeof(Define.MonsterStyle), monsterStat.name, true);
        }
        catch (ArgumentException e)
        {
            _stat.MonsterStyle = Define.MonsterStyle.unknown; // 기본값 설정
        }

        _stat.MonsterType = type;
        _stat.MoveSpeed = monsterStat.moveSpeed * ((float)(100f + level) / 100f);
        _stat.MaxHP = SetRandomStat((int)(monsterStat.maxHp * ((100f + 10f * level) / 100f))) * mul;
        _stat.HP = _stat.MaxHP;
        _stat.Damage = SetRandomStat((int)(monsterStat.damage * ((100f + level) / 100f)));
        _stat.Defense = SetRandomStat((int)(monsterStat.defense * ((100f + level) / 100f)));
        _rigid.mass = 3;
        _stat.ExpPoint = 10 * level;
        _stat.ExpMul = monsterStat.expMul;

        if (type == Define.MonsterType.middleBoss)
        {
            Debug.Log("Boss Spawn! ");
            Debug.Log($"MaxHp : {_stat.MaxHP}");
            Debug.Log($"Hp : {_stat.HP}");
            Debug.Log($"Damage : {_stat.Damage}");
            Debug.Log($"Defense : {_stat.Defense}");
            Debug.Log($"Weight : {_rigid.mass}");
        }
    }

    #endregion

    #region Private Field

    int SetRandomStat(int value)
    {
        value = (int)(value * UnityEngine.Random.Range(0.9f, 1.1f));
        return value;
    }

    void FloatDamageText(int damage)
    {
        //GameObject hudText = Instantiate(hudDamageText);
        //hudText.transform.position = transform.position + Vector3.up * 1.5f;
        //hudText.GetComponent<UI_DamageText>()._damage = damage;
    }

    void SpawnExp()
    {
        /*GameObject expGo = Managers.Game.Spawn(Define.WorldObject.Unknown, "Content/Exp");
        expGo.transform.position = transform.position;
        Exp_Item expPoint = expGo.GetComponent<Exp_Item>();
        expPoint.SetExp(_stat.ExpPoint, _stat.ExpMul);

        if (expPoint._expMul == 1)
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[0];
        else if (expPoint._expMul == 2)
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[1];
        else
            expGo.GetComponent<SpriteRenderer>().sprite = expPoint._sprite[2];
*/
    }

    void OnMove()
    {
        if (_target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

            if (distanceToPlayer > attackDistance && _isTracking)
            {
                // 플레이어를 향해 이동
                Vector3 dirVec = _target.position - _rigid.position;
                Vector3 nextVec = dirVec.normalized * (_stat.MoveSpeed * Time.fixedDeltaTime);
                _anime.SetBool("isWalk", true);
                _rigid.MovePosition(_rigid.position + nextVec);
                _rigid.velocity = Vector3.zero; // 이동 후 속도 초기화

                // 플레이어를 바라보게 설정
                transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));

                if (distanceToPlayer < stopDistance)
                    _isTracking = false;
            }
            else
            {
                // 공격 범위 내에 있을 때
                _anime.SetBool("isWalk", false);
                _rigid.velocity = Vector3.zero; // 이동 멈추기
                if (!_isAttack)
                {
                    StartCoroutine(RangeAttack());
                    _isAttack = true;
                }

                if (distanceToPlayer > stopDistance)
                    _isTracking = true;
            }
        }
    }

    IEnumerator RangeAttack()
    {
        _isAttack = true;
        _anime.SetTrigger("Attack");
        yield return new WaitForSeconds(0.2f);
        Area.enabled = true;
        yield return new WaitForSeconds(1f);
        Area.enabled = false;
        yield return new WaitForSeconds(1f);
        _isAttack = false;
    }
    IEnumerator AttackRange()
    {
        _isAttack = true;
        SpawnBullet();
        yield return new WaitForSeconds(2f);
        _isAttack = false;
    }
    private void OnEnable()
    {
        _isLive = true;
    }

    void SpawnBullet()
    {
        _anime.SetTrigger("Attack");

        EnemyBullet bullet = Managers.Resource.Instantiate("Weapon/EnemyBullet", _rigid.position).GetOrAddComponent<EnemyBullet>();
        bullet._damage = _stat.Damage;
        bullet._speed = 5f;
        bullet._dir = (_target.position - _rigid.position).normalized;
    }
    #endregion
}