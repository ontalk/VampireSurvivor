using UnityEngine;
using Unity.VisualScripting;

public class PoisonPotion : MonoBehaviour
{
    private Vector3 _initialVelocity;
    private Vector3 _gravity = new Vector3(0, -9.8f, 0);
    private float _elapsedTime = 0f;

    private int _damage;
    private float _speed;
    private float _force;
    private float _size;

    public void Initialize(Vector3 direction, int damage, float speed, float force, float size)
    {
        _damage = damage;
        _speed = speed;
        _force = force;
        _size = size;

        transform.localScale = Vector3.one * _size;

        // 포물선 운동을 위한 초기 속도 설정
        float angle = 45f; // 발사 각도 (조절 가능)
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
        _initialVelocity = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, horizontalDir)) * horizontalDir * _speed;

        // 위쪽 방향으로의 속도 추가
        _initialVelocity += Vector3.up * _speed * Mathf.Tan(angle * Mathf.Deg2Rad);
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        
        // 포물선 운동 계산
        Vector3 displacement = _initialVelocity * _elapsedTime + 0.5f * _gravity * _elapsedTime * _elapsedTime;
        transform.position = transform.position + displacement;

        // 이동 방향을 향해 회전
        if (_initialVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_initialVelocity + _gravity * _elapsedTime);
        }

        // 바닥에 닿았는지 확인 (y 좌표가 0 이하일 때)
        if (transform.position.y <= 0)
        {
            CreatePoisonEffect();
            Destroy(gameObject);
        }
    }

    void CreatePoisonEffect()
    {
        GameObject poisonEffect = Managers.Game.Spawn(Define.WorldObject.Weapon, "Weapon/Poison", transform.position);
        Poison poison = poisonEffect.GetOrAddComponent<Poison>();
        poison.Initialize(_damage, _force, _size);
    }
}