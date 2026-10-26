using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class MonsterBase : MonoBehaviour , IDamageable
{

    [Header("NavMeshAgent")]
    protected NavMeshAgent _agent;

    // MonsterDB ID (Ex: MON_SKE_001)
    [Header("MonsterDB ID")]
    [SerializeField] private string _monsterDataID;
    protected MonsterDB _data;

    [Header("Ref")]
    [SerializeField] private GameObject _player;
    [SerializeField] private MonsterDrop _monsterDrop;
    [SerializeField] protected Animator _animator;
    [SerializeField] private float _turnSpeed = 60f;

    private bool _isDead;

    #region[Property]
    // Can handle data through Property
    public NavMeshAgent Agent => _agent;
    public Animator Animator => _animator;
    public GameObject Player => _player;
    public float TurnSpeed => _turnSpeed;
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public float Damage { get; private set; }
    public float Defense { get; private set; }
    public float MoveSpeed { get; private set; }
    public float DetectRange { get; protected set; }
    public float AttackRange { get; protected set; }
    public bool IsDead => _isDead;
    public GameObject PoolKey { get; set; }
    
    public MonsterDrop MonsterDrop { get { return _monsterDrop; } }
    public MonsterDB Data { get { return _data; } }
    public MonsterDB EvolutionData;
    #endregion
    #region[FSM]
    // FSM
    public MonsterStateMachine StateMachine { get; private set; }
    public MonsterIdleState IdleState { get; private set; }
    public MonsterDamagedState DamagedState { get; private set; }
    public MonsterEvolutionState EvolutionState { get; private set; }
    public MonsterDieState DieState { get; private set; }
    public MonsterStunState StunState { get; private set; }
    public MonsterPatrolState PatrolState { get; private set; }
    public MonsterCombatState CombatState { get; private set; }
    #endregion
    public event Action<float> OnHitEvent;
    protected virtual void Awake()
    {
        if (_monsterDrop == null) _monsterDrop = GetComponent<MonsterDrop>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        StateMachine = new MonsterStateMachine();
        #region States
        CombatState = new MonsterCombatState(this, StateMachine);
        IdleState = new MonsterIdleState(this, StateMachine);
        PatrolState = new MonsterPatrolState(this, StateMachine);
        DamagedState = new MonsterDamagedState(this, StateMachine);
        EvolutionState = new MonsterEvolutionState(this, StateMachine);
        DieState = new MonsterDieState(this, StateMachine);
        StunState = new MonsterStunState(this, StateMachine);
        #endregion
        if (_data != null) ApplyMonsterData(_data);
    }
    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }
    protected virtual void Update()
    {
        if (_isDead) return;
        StateMachine.CurrentState.LogicUpdate();
    }

    public void Initialize()
    {
        if (_data == null && !string.IsNullOrEmpty(_monsterDataID))
        {
            _data = DataManager.Instance.GetMonster(_monsterDataID);
        }

        if (_data != null)
        {
            ApplyMonsterData(_data);
            if (_data.EvolveData != null)  EvolutionData = _data.EvolveData;
        }
        else
        {
            Debug.LogError($"[{name}] 데이터 로드 실패! ID: {_monsterDataID}");
        }


        _isDead = false;
        GetComponent<Collider>().enabled = true;
        _animator.SetBool("IsDead", false);
        _animator.SetBool("IsMove", false);
        StateMachine.ChangeState(IdleState);
    }

    public virtual void ApplyMonsterData(MonsterDB data)
    {
        MaxHp = data.Hp;
        CurrentHp = MaxHp;
        Damage = data.Damage;
        Defense = data.Defense;
        MoveSpeed = data.MoveSpeed;
        DetectRange = data.DetectRange;
        AttackRange = data.AttackRange;

        _agent.speed = MoveSpeed;
        _agent.stoppingDistance = 0f;
    }

    public void TakeDamage(float damage, GameObject attacker = null, bool isParryable = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default)
    {
        if (_isDead) return;

        float damageReduction = Defense * 0.2f;
        float finalDamage = Mathf.Max(damage - damageReduction, 1f);
        Debug.Log($"[ 몬스터 피격] 원본 데미지 : {damage}  몬스터 방어력 : {Defense} 데미지 감소량 : {damageReduction}  최종 데미지 : {finalDamage}"
);

        CurrentHp -= finalDamage;
        OnHitEvent?.Invoke(CurrentHp);

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

        if (CurrentHp <= 0)
        {
            StateMachine.ChangeState(DieState);
            return;
        }

        if (StateMachine.CurrentState != StunState)
            StateMachine.ChangeState(DamagedState);
    }

    #region[Utility]
    // Utility Method
    public bool FindTarget()
    {
        return Vector3.Distance(transform.position, _player.transform.position) < DetectRange;
    }

    public virtual void ApplyStun(float duration)
    {
        if (_isDead) return;
        StunState.SetDuration(duration);
        StateMachine.ChangeState(StunState);
    }

    public void RotateToTarget(int speed = 1)
    {
        Vector3 dir = (_player.transform.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * (_turnSpeed * speed));
    }
    
    public void SetTarget(GameObject player) => _player = player;

    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, _player.transform.position);
    }
    public bool IsPlayerDead()
    {
        var status = _player.GetComponent<PlayerStatus>();
        return status != null && status.IsDead;
    }
    #endregion

    public void SetDead(bool value) => _isDead = value;

    public abstract BTNode GetAttackNode();
}
