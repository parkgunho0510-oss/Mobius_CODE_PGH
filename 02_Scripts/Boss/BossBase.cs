using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController), typeof(NavMeshAgent), typeof(Animator))]
public abstract class BossBase : MonoBehaviour, IDamageable
{
    [Header("Ref")]
    [SerializeField] protected GameObject _player;
    //[SerializeField] protected MonsterDB _data;
    protected MonsterDB _data;

    [Header("MonsterDB ID")]
    [SerializeField] private string _monsterDataID;

    [Header ("Groggy")]
    private float _nextGroggyHpThreshold;
    private float _initialDefense;
    private int _totalWeakPoints;
    public bool IsGroggy { get; private set; } = false;
    public MonsterDB Data => _data;

    protected CharacterController _controller;
    protected NavMeshAgent _agent;
    protected Animator _animator;

    [Header("Gravity Stats")]
    [SerializeField] protected float _gravity = -9.81f;
    protected float _verticalVelocity;

    // 비헤이비어 트리 최상위 노드
    protected BTNode _bossTree;
    protected float _groggyTimer = 0f;

    // 속성
    public bool IsIgnoringGravity { get; set; } = false;
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public float DetectRange { get; private set; }
    public float AttackRange { get; private set; }
    public float TurnSpeed { get; private set; } = 10f;

    public float Defense {  get; private set; }
    public bool IsDead => CurrentHp <= 0f;

    public MonsterDB MonsterDB => _data;

    public GameObject Player => _player;
    public CharacterController Controller => _controller;
    public NavMeshAgent Agent => _agent;
    public Animator Animator => _animator;

    public event Action<float> OnHitEvent;

    protected virtual void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        // NavMeshAgent가 직접 Transform을 움직이지 못하게 차단
        _agent.updatePosition = false;
        _agent.updateRotation = false;

        if (!string.IsNullOrEmpty(_monsterDataID))
        {
            _data = DataManager.Instance.GetMonster(_monsterDataID);
            if (_data != null) ApplyData(_data);
        }

        //if (_data != null) ApplyData(_data);
        //else Debug.LogWarning($"[{name}] BossData is NULL");

        // 자식 클래스에서 트리를 조립하도록
        InitBossTree();
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        // 그로기(스턴) 상태면 트리 행동을 멈춤
        if (_groggyTimer > 0f)
        {
            _groggyTimer -= Time.deltaTime;

            if (_groggyTimer <= 0f)
            {
                EndGroggyState();
            }
        }
        else
        {
            // 그로기가 아닐 때만 비헤이비어 트리 가동
            _bossTree?.Evaluate();
        }

        // 이동 및 중력은 항상 적용
        ApplyGravity();
        ApplyMovement();
    }
    private void EndGroggyState()
    {
        IsGroggy = false;
        _animator.SetBool("IsGroggy", false);
    }
    protected void ApplyGravity()
    {
        if (IsIgnoringGravity) return;

        if (_controller.isGrounded)
        {
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }
    public virtual void ApplyGroggy(float duration)
    {
        if (IsDead) return;

        _bossTree?.Abort();
        _groggyTimer = duration;
        IsGroggy = true; // 약점 노출 상태 활성화
        _animator.SetBool("IsGroggy", true);
    }
    public void ReduceDefenseFromPartBreak()
    {
        if (_totalWeakPoints > 0)
        {
            Defense -= (_initialDefense / _totalWeakPoints);
            Defense = Mathf.Max(Defense, 0f);
        }
    }

    public virtual void ApplyStun(float duration)
    {
        if (IsDead) return;

        _bossTree?.Abort();
        _groggyTimer = duration;
        IsGroggy = false; // 일반 스턴이므로 그로기 아님
        _animator.SetTrigger("Stun");
    }

    protected void ApplyMovement()
    {
        Vector3 finalVelocity = Vector3.zero;

        if (!_agent.pathPending && !_agent.isStopped)
        {
            Vector3 direction = _agent.nextPosition - transform.position;
            direction.y = 0; // 수평 방향만 추출

            if (direction.magnitude > 0.05f)
            {
                finalVelocity = direction.normalized * _agent.speed;
            }
        }

        if (!IsIgnoringGravity)
        {
            finalVelocity.y = _verticalVelocity;
        }

        _controller.Move(finalVelocity * Time.deltaTime);
        _agent.nextPosition = transform.position;

        Vector3 horizontalVelocity = new Vector3(finalVelocity.x, 0f, finalVelocity.z);
        _animator.SetBool("IsMove", horizontalVelocity.magnitude > 0.05f);
    }

    public void RotateToTarget()
    {
        if (_player == null) return;
        Vector3 dir = (_player.transform.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * TurnSpeed);
        }
    }

    public void ApplyData(MonsterDB data)
    {
        MaxHp = data.Hp;
        CurrentHp = MaxHp;
        DetectRange = data.DetectRange;
        AttackRange = data.AttackRange;
        Defense = data.Defense;

        _agent.speed = data.MoveSpeed;
        _agent.stoppingDistance = AttackRange;

        _nextGroggyHpThreshold = MaxHp * 0.75f;
        _initialDefense = Defense;
        _totalWeakPoints = GetComponentsInChildren<BossWeakPoint>().Length;
    }

    public virtual void TakeDamage(float damage, GameObject attacker = null, bool isParryable = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default)
    {
        if (IsDead) return;

        float damageReduction = Defense * 0.2f;
        float finalDamage = Mathf.Max(damage - damageReduction, 1f);

        CurrentHp -= finalDamage;
        OnHitEvent?.Invoke(CurrentHp);

        // TODO: EffectEvents.RaiseEffectPlayedEvent 등 이펙트 호출

        Quaternion sparkRot = Quaternion.LookRotation(hitNormal);
        EffectEvents.RaiseEffectPlayedEvent(EffectType.SPARK, hitPoint, sparkRot);

        Vector3 attackDir = (hitPoint - attacker.transform.position).normalized;

        // 표면 반사 방향 반영 hitNormal과 attackDir을 섞어서 
        // 칼에 베여서 피가 튕겨 나가는 각도 생성 (반사 벡터 활용)
        Vector3 reflectDir = Vector3.Reflect(attackDir, hitNormal);

        // 반사된 방향 + 타격 표면 방향을 섞어 자연스럽게 표면을 타고 튀게 하는 용도
        Vector3 finalBleedDir = Vector3.Lerp(reflectDir, hitNormal, 0.3f).normalized;

        // 너무 수직으로만 튀지 않게 하기 위해 랜덤성 추가
        finalBleedDir += new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(-0.2f, 0.3f), UnityEngine.Random.Range(-0.4f, 0.4f));

        Quaternion bloodRot = Quaternion.LookRotation(finalBleedDir.normalized);
        EffectEvents.RaiseEffectPlayedEvent(EffectType.BLOOD, hitPoint, bloodRot);
        SoundEvents.RaiseSFXPlayedEvent("KnightShriek");
        SoundEvents.RaiseSFXPlayedEvent("SwordHit");

        if (CurrentHp <= _nextGroggyHpThreshold && CurrentHp > 0)
        {
            ApplyGroggy(15f); // 5초 그로기
            _nextGroggyHpThreshold -= MaxHp * 0.25f;
        }
        if (CurrentHp <= 0)
        {
            // 사망 시 트리 강제 종료 및 사망 처리
            _bossTree?.Abort();
            _animator.SetTrigger("Die");
            OnDie();
        }
    }

    // 사망 시 콜라이더 해제 등 물리적 처리를 위한 가상 함수
    protected virtual void OnDie()
    {
        _controller.enabled = false;
        _agent.enabled = false;

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 5f);
    }

    #region Utility
    public bool FindTarget() => Vector3.Distance(transform.position, _player.transform.position) < DetectRange;
    public float DistanceToPlayer() => Vector3.Distance(transform.position, _player.transform.position);
    public bool IsPlayerDead()
    {
        var status = _player.GetComponent<PlayerStatus>();
        return status != null && status.IsDead;
    }
    #endregion

    protected abstract void InitBossTree();
}