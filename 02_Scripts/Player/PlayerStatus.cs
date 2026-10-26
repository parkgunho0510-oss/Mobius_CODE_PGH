using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;



public class PlayerStatus : MonoBehaviour, IDamageable
{
    private PlayerController player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private InventoryUI _inventoryUI;

    [Header("HP Stats")]
    private float _currentHP;
    public float CurrentHP => _currentHP;
    public bool IsDead { get; private set; }

    [Header("Stamina Stats")]
    private float _currentStamina;
    public float CurrentStamina => _currentStamina;

    [Header("Stamina Settings")]
    [SerializeField] private float _regenDelay = 1f;
    [SerializeField] private float _regenInterval = 0.5f;
    [SerializeField] private float _regenAmount = 10f;

    [Header("ShakeData")]
    [SerializeField] private ShakeData _parryShakeData;

    [Header("Stat Point")]
    [SerializeField] private float _hpIncreaseAmount = 25f;
    [SerializeField] private float _StaminaIncreaseAmount = 10f;
    [SerializeField] private float _atkIncreaseAmount = 10f;
    [SerializeField] private float _defIncreaseAmount = 10f;
    [SerializeField] private float _luckIncreaseAmount = 10f;

    [Header("Player Stat Level")]
    [SerializeField] private int _maxStatLevel = 51;

    [SerializeField] private CurrentUsingItems _currentUsingItems;

    private float _equipmentATK;
    private float _equipmentDEF;
    private float _equipmentLUCK;
    private float _equipmentHP;

    private float _lastSpendTime;
    private float _equipmentSTAMINA;
    private float _regenTimer;

    private StatManager Stat => StatManager.Instance;

    public float TotalATK => ATK + _equipmentATK;
    public float TotalDEF => DEF + _equipmentDEF;
    public float TotalLUCK => LUCK + _equipmentLUCK;
    public float TotalHP => _maxHP + _equipmentHP;
    public float TotalSTAMINA => MaxStamina + _equipmentSTAMINA;


    #region Property

    public int StatPoint
    {
        get => Stat.StatPoint;
        set => Stat.StatPoint = value;
    }

    public int StatResetCount
    {
        get => Stat.StatResetCount;
        set => Stat.StatResetCount = value;
    }

    public float _maxHP
    {
        get => Stat.MaxHP;
        set => Stat.MaxHP = value;
    }

    public float MaxStamina
    {
        get => Stat.MaxStamina;
        set => Stat.MaxStamina = value;
    }

    public float ATK
    {
        get => Stat.BaseATK;
        set => Stat.BaseATK = value;
    }

    public float DEF
    {
        get => Stat.BaseDEF;
        set => Stat.BaseDEF = value;
    }

    public float LUCK
    {
        get => Stat.BaseLUCK;
        set => Stat.BaseLUCK = value;
    }

    public int HPLevel
    {
        get => Stat.HPLevel;
        set => Stat.HPLevel = value;
    }

    public int StaminaLevel
    {
        get => Stat.StaminaLevel;
        set => Stat.StaminaLevel = value;
    }

    public int ATKLevel
    {
        get => Stat.ATKLevel;
        set => Stat.ATKLevel = value;
    }

    public int DEFLevel
    {
        get => Stat.DEFLevel;
        set => Stat.DEFLevel = value;
    }

    public int LUCKLevel
    {
        get => Stat.LUCKLevel;
        set => Stat.LUCKLevel = value;
    }

    public int Level
    {
        get => Stat.Level;
        set => Stat.Level = value;
    }

    public int AccumulationExp
    {
        get => Stat.AccumulationExp;
        set => Stat.AccumulationExp = value;
    }
    #endregion


    private void OnEnable()
    {
        UIEvents.OnLevelUpEvent += LevelupGetStatPoint;
    }

    private void OnDisable()
    {
        UIEvents.OnLevelUpEvent -= LevelupGetStatPoint;
        
    }
    private void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();

        if (_currentUsingItems == null)
            _currentUsingItems = FindFirstObjectByType<CurrentUsingItems>();

        if (_inventory == null)
            _inventory = FindFirstObjectByType<Inventory>();

        if (_inventoryUI == null)
            _inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    private void Start()
    {
        if (player != null && player.Animator != null)
            player.Animator.SetBool("Die", false);

    
            _currentHP = _maxHP;
            _currentStamina = MaxStamina;
        UIEvents.RaiseLevelUpEvent(Level);
        UIEvents.RaiseExpChangedEvent(AccumulationExp);
        UIEvents.RaiseStatPointChanged(StatPoint);

        UIEvents.RaiseHPChanged(_currentHP, _maxHP);
        UIEvents.RaiseStaminaChanged(_currentStamina, MaxStamina);

        UIEvents.RaiseMaxHPChanged(_maxHP);
        UIEvents.RaiseMaxStaminaChanged(MaxStamina);

        UIEvents.RaiseATKStatChanged(ATK);
        UIEvents.RaiseDEFStatChanged(DEF);
        UIEvents.RaiseLUCKStatChanged(LUCK);

        UIEvents.RaiseStatLevelChanged(StatType.HP, HPLevel);
        UIEvents.RaiseStatLevelChanged(StatType.STAMINA, StaminaLevel);
        UIEvents.RaiseStatLevelChanged(StatType.ATK, ATKLevel);
        UIEvents.RaiseStatLevelChanged(StatType.DEF, DEFLevel);
        UIEvents.RaiseStatLevelChanged(StatType.LUCK, LUCKLevel);

        RefreshStatPoint();
    }
    private void Update()
    {
        if (IsDead) return;

        if (player.StateMachine.CurrentState == player.IdleState ||
            player.StateMachine.CurrentState == player.WalkState)
        {
            RegenStamina();
        }
    }
    private void ApplyExp()
    {
        LevelData data = DataManager.Instance.GetLevel($"LVL_{Level}");
        if (data == null) return;

        float per = 0f;
        while (true)
        {
            int requireExp = data.NextExp;
            int totalExp = data.TotalExp;

            // 0으로 나누기 방지 (CSV 데이터 누락 대비)
            if (requireExp <= 0) break;

            per = (AccumulationExp - totalExp) / (float)requireExp;

            // 레벨업 조건을 만족했을 때
            if (per >= 1f)
            {
                Level += 1;
                UIEvents.RaiseLevelUpEvent(Level);

                // 핵심 수정: 레벨업을 했으므로 다음 레벨의 데이터를 새로 가져옴
                data = DataManager.Instance.GetLevel($"LVL_{Level}");

                // 만렙 도달 등으로 다음 레벨 데이터가 없다면 루프 강제 탈출
                if (data == null)
                {
                    per = 1f; // 경험치바를 100%로 고정
                    break;
                }
            }
            else
            {
                // 레벨업 조건에 미달하면 정상적으로 루프 탈출
                break;
            }
        }

        // 경험치 UI 업데이트 (안전하게 0~100 사이 값으로 클램핑)
        int figure = (int)(Mathf.Clamp01(per) * 100);
        UIEvents.RaiseExpChangedEvent(figure);
    }
    public void Heal(float amount)
    {
        _currentHP += amount;

        _currentHP = Mathf.Clamp(_currentHP, 0f, TotalHP);

        UIEvents.RaiseHPChanged(_currentHP, TotalHP);
    }
    public void TakeDamage(float amount, GameObject attacker = null, bool isParryable = false, Vector3 hitPoint = default, Vector3 hitNormal = default, ShakeData shakeData = default)
    {
        if (IsDead || amount <= 0) return;

        // 공격 방향 판별 로직 추가 
        bool isFrontalAttack = true;
        if (attacker != null)
        {
            // 플레이어에서 공격자를 향하는 방향 벡터
            Vector3 dirToAttacker = (attacker.transform.position - transform.position).normalized;
            dirToAttacker.y = 0;

            Vector3 playerForward = transform.forward;
            playerForward.y = 0;

            // 1이면 정면, 0이면 측면(90도), -1이면 후방
            // 0.5f는 기준 각도 ±60도(총 120도) 이내를 정면으로 본다는 의미
            float dotProduct = Vector3.Dot(playerForward.normalized, dirToAttacker.normalized);

            if (dotProduct < 0.5f)
            {
                isFrontalAttack = false; // 뒤나 옆에서 맞음
            }
        }

        // 패링 로직
        if (isParryable && player.StateMachine.CurrentState == player.ParryState && isFrontalAttack)
        {
            Debug.Log("패링 성공");
            CameraEvents.RaiseShake(_parryShakeData);

            if (attacker != null && attacker.TryGetComponent<MonsterBase>(out var monster))
                monster.ApplyStun(3.0f);
            else if (attacker != null && attacker.TryGetComponent<BossBase>(out var boss))
                boss.ApplyStun(3.0f);
            return;
        }

        if (shakeData != null) CameraEvents.RaiseShake(shakeData);

        // 방어 로직
        if (player.IsBlocking && _currentStamina > 0 && isFrontalAttack)
        {
            SpendStamina(20f);
            Debug.Log("방어 성공");
            return;
        }
    


        float damageReduction = TotalDEF * 0.1f;
        float finalDamage = Mathf.Max(amount - damageReduction, 1f);

        Debug.Log($"원본 데미지 : {amount}");
        Debug.Log($"데미지 감소량 : {damageReduction}");
        Debug.Log($"최종 데미지 : {finalDamage}");

       
        _currentHP -= finalDamage;
        UIEvents.RaiseHPChanged(_currentHP, _maxHP);

        if (_currentHP <= 0f)
        {
            _currentHP = 0f;
            IsDead = true;
            StartCoroutine(Co_Die());
        }
        else
        {
            player.StateMachine.ChangeState(player.HitState);
        }
    }
    private void RegenStamina()
    {
        if (_currentStamina >= MaxStamina) return;
        if (Time.time - _lastSpendTime < _regenDelay) return;

        _regenTimer += Time.deltaTime;

        if (_regenTimer >= _regenInterval)
        {
            _regenTimer = 0f;
            _currentStamina += _regenAmount;
            if (_currentStamina > MaxStamina) _currentStamina = MaxStamina;
            UIEvents.RaiseStaminaChanged(_currentStamina, MaxStamina);

        }
    }

    // 스테미나 소모 함수
    public void SpendStamina(float amount)
    {
        if (amount <= 0 || _currentStamina <= 0) return;

        _currentStamina -= amount;
        if (_currentStamina < 0) _currentStamina = 0;

        _lastSpendTime = Time.time;
        _regenTimer = 0f;
        UIEvents.RaiseStaminaChanged(_currentStamina, MaxStamina);
    }

    // Exp
    public void AddExp(int addedExp)
    {
        AccumulationExp += addedExp;
        ApplyExp();
    }



    public void IncreaseMaxHP(float amount)
    {
        _maxHP += amount;
        _currentHP = _maxHP;

        UIEvents.RaiseMaxHPChanged(_maxHP);
        UIEvents.RaiseHPChanged(_currentHP, _maxHP);

    }


    public void IncreaseMaxStamina(float amount)
    {
        MaxStamina += amount;
        _currentStamina = MaxStamina;

        UIEvents.RaiseMaxStaminaChanged(MaxStamina);
        UIEvents.RaiseStaminaChanged(_currentStamina, MaxStamina);
    }

    public void IncreaseATK(float amount)
    {
        ATK += amount;
        UIEvents.RaiseATKStatChanged(TotalATK);
    }

    public void IncreaseDEF(float amount)
    {
        DEF += amount;
        UIEvents.RaiseDEFStatChanged(TotalDEF);
    }
    public void IncreaseLuck(float amount)
    {
        LUCK += amount;
        UIEvents.RaiseLUCKStatChanged(TotalLUCK);
    }


    public void TryIncreaseStat(StatType statType)
    {
        if (StatPoint <= 0)
        {
            Debug.Log("스탯 포인트 부족");
            return;
        }
        if (!CanIncrease(statType))
        {
            Debug.Log("최대 레벨 도달");
            return;
        }


        StatPoint--;

        switch (statType)
        {
            case StatType.HP:
                IncreaseMaxHP(_hpIncreaseAmount);
                HPLevel++;
                UIEvents.RaiseStatLevelChanged(StatType.HP, HPLevel);
                break;

            case StatType.STAMINA:
                IncreaseMaxStamina(_StaminaIncreaseAmount);
                StaminaLevel++;
                UIEvents.RaiseStatLevelChanged(StatType.STAMINA, StaminaLevel);
                break;

            case StatType.ATK:
                IncreaseATK(_atkIncreaseAmount);
                ATKLevel++;
                UIEvents.RaiseStatLevelChanged(StatType.ATK, ATKLevel);
                break;

            case StatType.DEF:
                IncreaseDEF(_defIncreaseAmount);
                DEFLevel++;
                UIEvents.RaiseStatLevelChanged(StatType.DEF, DEFLevel);
                break;

            case StatType.LUCK:
                IncreaseLuck(_luckIncreaseAmount);
                LUCKLevel++;
                UIEvents.RaiseStatLevelChanged(StatType.LUCK, LUCKLevel);
                break;
        }

        UIEvents.RaiseStatPointChanged(StatPoint);
    }


    private void LevelupGetStatPoint(int nextLevel)
    {
        StatPoint += 3;
        UIEvents.RaiseStatPointChanged(StatPoint);
        Debug.Log($"레벨업  / 현재 레벨: {nextLevel} / 현재 스탯포인트: {StatPoint}");
    }


    public void ResetStat()
    {
        if (StatResetCount <=0)
        {
            Debug.Log("스탯 초기화권이 없습니다");
            return;
        }

        int returnPoint = 0;

        returnPoint += HPLevel - 1;
        returnPoint += StaminaLevel - 1;
        returnPoint += ATKLevel - 1;
        returnPoint += DEFLevel - 1;
        returnPoint += LUCKLevel - 1;


        HPLevel = 1;
        StaminaLevel = 1;
        ATKLevel = 1;
        DEFLevel = 1;
        LUCKLevel = 1;


        StatPoint += returnPoint;
        StatResetCount--;

        RefreshStatPoint();
        ResetBaseStats();
        UIEvents.RaiseStatResetCountChanged(StatResetCount);

        Debug.Log($"스탯 초기화 완료 / 반환 포인트: {returnPoint}");

    }

    private void RefreshStatPoint()
    {
        UIEvents.RaiseStatPointChanged(StatPoint);
        UIEvents.RaiseStatResetCountChanged(StatResetCount);

        UIEvents.RaiseStatLevelChanged(StatType.HP, HPLevel);
        UIEvents.RaiseStatLevelChanged(StatType.STAMINA, StaminaLevel);
        UIEvents.RaiseStatLevelChanged(StatType.ATK, ATKLevel);
        UIEvents.RaiseStatLevelChanged(StatType.DEF, DEFLevel);
        UIEvents.RaiseStatLevelChanged(StatType.LUCK, LUCKLevel);

    }

    private void ResetBaseStats()
    {
        _maxHP = 100f;
        _currentHP = _maxHP;

        MaxStamina = 100f;
        _currentStamina = MaxStamina;

        ATK = 25f;
        DEF = 10f;
        LUCK = 0f;

        ApplyEquipmentStat(_currentUsingItems);

        UIEvents.RaiseMaxHPChanged(TotalHP);
        UIEvents.RaiseMaxStaminaChanged(TotalSTAMINA);
        UIEvents.RaiseATKStatChanged(TotalATK);
        UIEvents.RaiseDEFStatChanged(TotalDEF);
        UIEvents.RaiseLUCKStatChanged(TotalLUCK);
    }

    private bool CanIncrease(StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
                return HPLevel  < _maxStatLevel;

            case StatType.STAMINA:
                return StaminaLevel  < _maxStatLevel;

            case StatType.ATK:
                return ATKLevel  < _maxStatLevel;

            case StatType.DEF:
                return DEFLevel  < _maxStatLevel;

            case StatType.LUCK:
                return LUCKLevel  < _maxStatLevel;
        }

        return false;
    }

    public void ApplyEquipmentStat(CurrentUsingItems items)
    {
        _equipmentATK = 0f;
        _equipmentDEF = 0f;
        _equipmentLUCK = 0f;
        _equipmentHP = 0f;
        _equipmentSTAMINA = 0f;

        if (items.CurrentWeapon != null)
        {   _equipmentATK += items.CurrentWeapon.ATK;
            _equipmentDEF += items.CurrentWeapon.DEF;
            _equipmentLUCK += items.CurrentWeapon.LUCK;
            _equipmentHP += items.CurrentWeapon.HP;
            _equipmentSTAMINA += items.CurrentWeapon.STAMINA;

        }
        if (items.Armor1 != null)
        {
            _equipmentDEF += items.Armor1.DEF;
            _equipmentHP += items.Armor1.HP;
            _equipmentSTAMINA += items.Armor1.STAMINA;
            _equipmentLUCK += items.Armor1.LUCK;
        }
        if (items.Armor2 != null)
        {
            _equipmentDEF += items.Armor2.DEF;
            _equipmentHP += items.Armor2.HP;
            _equipmentSTAMINA += items.Armor2.STAMINA;
            _equipmentLUCK += items.Armor2.LUCK;
        }

        UIEvents.RaiseMaxHPChanged(TotalHP);
        UIEvents.RaiseMaxStaminaChanged(TotalSTAMINA);
        UIEvents.RaiseATKStatChanged(TotalATK);
        UIEvents.RaiseDEFStatChanged(TotalDEF);
        UIEvents.RaiseLUCKStatChanged(TotalLUCK);
    }

    public PlayerStatSaveData CreateStatSaveData()
    {
        PlayerStatSaveData data = new();
        data.StatPoint          = StatPoint;
        data.StatResetCount     = StatResetCount;
        data.ATKLevel           = ATKLevel;
        data.DEFLevel           = DEFLevel;
        data.LUCKLevel          = LUCKLevel;
        data.HPLevel            = HPLevel;
        data.StaminaLevel       = StaminaLevel;

        return data;
    }

    public PlayerSaveData CreatePlayerSaveData()
    {
        PlayerSaveData data     = new();

        data.Level              = Level;
        data.Exp                = AccumulationExp;
        data.Gold               = GoldManager.Instance.Gold;

        return data;
    }
    public void LoadStatSaveData(PlayerStatSaveData data)
    {
        StatPoint       = data.StatPoint;
        StatResetCount  = data.StatResetCount;
        ATKLevel        = data.ATKLevel;
        DEFLevel        = data.DEFLevel;
        LUCKLevel       = data.LUCKLevel;
        HPLevel         = data.HPLevel;
        StaminaLevel    = data.StaminaLevel;

        RefreshStatPoint();
    }
    public void LoadPlayerSaveData(PlayerSaveData data)
    {
        Level = data.Level;
        AccumulationExp = data.Exp;
        GoldManager.Instance.Gold = data.Gold;

        UIEvents.RaiseLevelUpEvent(Level);

        LevelData levelData = DataManager.Instance.GetLevel($"LVL_{Level}");
        if (levelData == null) return;

        int requireExp = levelData.NextExp;
        int totalExp = levelData.TotalExp;

        float per = (AccumulationExp - totalExp) / (float)requireExp;
        int figure = (int)(Mathf.Clamp01(per) * 100);
        UIEvents.RaiseExpChangedEvent(figure);


        UIEvents.RaiseGoldChanged(GoldManager.Instance.Gold);
    }
    

    IEnumerator Co_Die()
    {
        IsDead = false;
        Debug.Log("플레이어 사망");
        player.Animator.SetBool("Die", true);

        _currentHP = Stat.MaxHP;
        IsDead = true;

        // 1. 인벤토리 패널티 적용
        if (_inventory != null)
        {
            _inventory.ApplyDeathPenalty();
        }

        // 2. 장착/퀵슬롯 UI 동기화
        if (_inventoryUI != null)
        {
            _inventoryUI.UpdateCurrentUsingItems();
        }

        yield return new WaitForSeconds(4f);

        SceneManager.LoadScene("Lobby");

        UIEvents.RaisePlayerDead();
    }
    public void HealStamina(float amount)
    {
        _currentStamina += amount;

        _currentStamina = Mathf.Clamp(_currentStamina, 0, TotalSTAMINA);

        UIEvents.RaiseStaminaChanged(_currentStamina, TotalSTAMINA);
    }


}