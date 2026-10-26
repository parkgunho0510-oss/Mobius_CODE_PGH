using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 5f;
    
    private float _damage;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // 날아가다가 어딘가에 박히지 않으면 메모리 누수 방지를 위해 5초 뒤 자동 파괴
        Destroy(gameObject, _lifeTime); 
    }

    public void Fire(Vector3 direction, float damage)
    {
        _damage = damage;
        // 유니티 내장 Rigidbody를 이용해 날려보냄
        _rb.linearVelocity = direction.normalized * _speed; 
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 타겟(플레이어)과 충돌했을 때 데미지 처리
        if (other.CompareTag("Player"))
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // 타격 방향(-transform.forward)을 넘겨주어 MonsterBase의 출혈/스파크 파티클 방향을 맞춤
                damageable.TakeDamage(_damage, gameObject, true, transform.position, -transform.forward);
            }
            Destroy(gameObject); // 플레이어 타격 후 화살 소멸
        }
    }
}