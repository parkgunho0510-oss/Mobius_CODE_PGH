using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class WeaponSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Combat_Test2 _combat;
    
    [SerializeField] Transform _handSocket;

    [SerializeField] private CurrentUsingItems _currentUsingItems;
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private PlayerStatus  _playerStatus;

   

    private Animator _animator;
    private RuntimeAnimatorController _baseController;
    private PlayerInput _playerInput;
    private PlayerController _player;


    private EquipmentData _weapon1;
    private EquipmentData _weapon2;

    private Weapon[] _equippedWeapons = new Weapon[2];

    //private Weapon _weapon1;
    //private Weapon _weapon2;
    //private Weapon weapon;

    private GameObject _mainWeaponObj;
    private GameObject _subWeaponObj;

    private bool _isMainWeapon = true;


    public Weapon[] EquippedWeapons => _equippedWeapons;

    void Awake()
    {
        if (_inventoryUI ==null)
        {
            _inventoryUI= FindFirstObjectByType<InventoryUI>();
        }

        if (_currentUsingItems==null)
        {
            _currentUsingItems= FindFirstObjectByType<CurrentUsingItems>();
        }

        if (_playerStatus==null)
        {
            _playerStatus= GetComponent<PlayerStatus>();
        }

        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _player = GetComponent<PlayerController>();
        _baseController = _animator.runtimeAnimatorController;
    }
    private void OnEnable()
    {
        //WeaponSwitchEvents.OnWeaponChanged += OnSlotDataChanged;
    }

    private void OnDisable()
    {
        //WeaponSwitchEvents.OnWeaponChanged -= OnSlotDataChanged;
    }
    private void Start()
    {
        if (_currentUsingItems != null)
        {
            if (_currentUsingItems.IsDataLoaded)
                SetWeapon();
            else
                _currentUsingItems.OnItemsInitialized += SetWeapon;
        }
    }

    private void OnDestroy()
    {
        if (_currentUsingItems != null)
        {
            _currentUsingItems.OnItemsInitialized -= SetWeapon;
        }
    }

    void Update()
    {
        if (_playerInput.actions["SwitchWeapon"].triggered)
        {
            if (_mainWeaponObj == null || _subWeaponObj == null)
            {
                Debug.LogWarning("아직 무기 로딩이 완료되지 않아 스왑할 수 없습니다.");
                return;
            }

            Debug.Log("R키 입력");

            Debug.Log($"현재 상태: {_player.StateMachine.CurrentState}");

            if (CanSwitchWeapon())
            {
                Debug.Log("무기 교체 가능");
                SwitchWeapon();
            }
            else
            {
                Debug.LogWarning("무기 교체 불가능 상태");
            }
        }
    }
    private bool CanSwitchWeapon()
    {
        if (_player == null || _player.StateMachine == null)
        {
            Debug.LogWarning("Player 또는 StateMachine 없음");
            return false;
        }

        var currentState = _player.StateMachine.CurrentState;

        Debug.Log($"현재 상태: {currentState.GetType().Name}");
        Debug.Log($"IdleState: {_player.IdleState.GetType().Name}");
        Debug.Log($"WalkState: {_player.WalkState.GetType().Name}");

        return currentState == _player.IdleState || currentState == _player.WalkState;
    }
    
    void SetWeapon()
    {
        _weapon1 = _currentUsingItems.GetMainWeaponData();
        _weapon2 = _currentUsingItems.GetSubWeaponData();

        Debug.Log($"메인 무기 데이터: {(_weapon1 != null ? _weapon1.ItemName : "NULL")}");
        Debug.Log($"서브 무기 데이터: {(_weapon2 != null ? _weapon2.ItemName : "NULL")}");

        if (_weapon1 != null)
            Debug.Log($"메인 무기프리팹: {(_weapon1.WeaponPrefab != null ? _weapon1.WeaponPrefab.name : "NULL")}");

        if (_weapon2 != null)
            Debug.Log($"서브 무기프리팹: {(_weapon2.WeaponPrefab != null ? _weapon2.WeaponPrefab.name : "NULL")}");

        CreateMainWeapon(_weapon1);
        CreateSubWeapon(_weapon2);

        ChangeWeaponAnimatorController(_weapon1);
    }

    void CreateMainWeapon(EquipmentData data)
    {
      if (data == null || data.WeaponPrefab == null)
    {
        Debug.LogWarning("CreateSubWeapon: data 또는 WeaponPrefab이 NULL입니다.");
        return;
    }

        _mainWeaponObj = Instantiate(data.WeaponPrefab, _handSocket);
        _mainWeaponObj.transform.localPosition = Vector3.zero;
        _mainWeaponObj.transform.localRotation = Quaternion.identity;
        _mainWeaponObj.SetActive(true);

        Weapon weapon = _mainWeaponObj.GetComponent<Weapon>();
        _combat.SetWeapon(weapon);
    }

    void CreateSubWeapon(EquipmentData data)
    {
        if (data == null || data.WeaponPrefab == null)
        {
            Debug.LogWarning("CreateSubWeapon: data 또는 WeaponPrefab이 NULL입니다.");
            return;
        }

        _subWeaponObj = Instantiate(data.WeaponPrefab, _handSocket);

        _subWeaponObj.transform.localPosition = Vector3.zero;
        _subWeaponObj.transform.localRotation = Quaternion.identity;

        _subWeaponObj.SetActive(false);

        //Weapon weapon = _mainWeaponObj.GetComponent<Weapon>();
        //_combat.SetWeapon(weapon);
    }


    public void SwitchWeapon()
    {
        _isMainWeapon = !_isMainWeapon;

        _mainWeaponObj.SetActive(_isMainWeapon);
        _subWeaponObj.SetActive(!_isMainWeapon);

        Weapon weapon = null;

        if (_isMainWeapon)
        {
            _currentUsingItems.CurrentWeapon = _weapon1;
            weapon = _mainWeaponObj.GetComponent<Weapon>();
            ChangeWeaponAnimatorController(_weapon1);
        }
        else
        {
            _currentUsingItems.CurrentWeapon = _weapon2;
            weapon = _subWeaponObj.GetComponent<Weapon>();
            ChangeWeaponAnimatorController(_weapon2);
        }

        if (weapon != null)
        {
            _combat.SetWeapon(weapon);
        }
        else
        {
            Debug.LogWarning("현재 무기 오브젝트에 Weapon 컴포넌트가 없음");
        }



        _playerStatus.ApplyEquipmentStat(_currentUsingItems);

        Debug.Log("무기 스왑");

        UIEvents.RaiseWeaponSwitched(_currentUsingItems.CurrentWeapon, _currentUsingItems.GetSubWeapon());
    }


    public void ChangeAndCreateMainWeapon(EquipmentData data)
    {
        _weapon1 = data;

        if (_mainWeaponObj != null)
        {
            Destroy(_mainWeaponObj);
        }

        _mainWeaponObj = CreateWeaponObj(data);

        if (_isMainWeapon)
        {
            _mainWeaponObj.SetActive(true);
            _currentUsingItems.CurrentWeapon = _weapon1;
            _combat.SetWeapon(_mainWeaponObj.GetComponent<Weapon>());
            _playerStatus.ApplyEquipmentStat(_currentUsingItems);
            ChangeWeaponAnimatorController(_weapon1);
        }
        else
        {
            _mainWeaponObj.SetActive(false);
        }

    }
    public void ChangeAndCreateSubWeapon(EquipmentData data)
    {
        _weapon2 = data;

        if (_subWeaponObj != null)
            Destroy(_subWeaponObj);

        _subWeaponObj = CreateWeaponObj(data);

        if (!_isMainWeapon)
        {
            _subWeaponObj.SetActive(true);
            _currentUsingItems.CurrentWeapon = _weapon2;

            _combat.SetWeapon(_subWeaponObj.GetComponent<Weapon>());

            _playerStatus.ApplyEquipmentStat(_currentUsingItems);

            ChangeWeaponAnimatorController(_weapon2);
        }
        else
        {
            _subWeaponObj.SetActive(false);
        }
    }


    private GameObject CreateWeaponObj(EquipmentData data)
    {
        if (data == null || data.WeaponPrefab == null)
            return null;

        GameObject obj = Instantiate(data.WeaponPrefab, _handSocket);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.SetActive(false);

        return obj;
    }

    private void ChangeWeaponAnimatorController(EquipmentData data)
    {
        RuntimeAnimatorController targetController = _baseController;
        if (data != null && data.AnimData != null && data.AnimData.WeaponAnimatorController != null)
        {
            targetController = data.AnimData.WeaponAnimatorController;
        }

        if (_animator.runtimeAnimatorController == targetController) return;

        // 1. 기존 파라미터 백업
        Dictionary<int, object> parameters = new Dictionary<int, object>();
        foreach (AnimatorControllerParameter param in _animator.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    parameters[param.nameHash] = _animator.GetFloat(param.nameHash);
                    break;
                case AnimatorControllerParameterType.Int:
                    parameters[param.nameHash] = _animator.GetInteger(param.nameHash);
                    break;
                case AnimatorControllerParameterType.Bool:
                    parameters[param.nameHash] = _animator.GetBool(param.nameHash);
                    break;
            }
        }

        // 방어 상태 유지를 위한 상체 레이어 가중치 백업
        float[] layerWeights = new float[_animator.layerCount];
        for (int i = 0; i < _animator.layerCount; i++)
        {
            layerWeights[i] = _animator.GetLayerWeight(i);
        }

        // 컨트롤러 교체
        _animator.runtimeAnimatorController = targetController;

        // 멈칫거림 방지
        foreach (var kvp in parameters)
        {
            if (kvp.Value is float f) _animator.SetFloat(kvp.Key, f);
            else if (kvp.Value is int i) _animator.SetInteger(kvp.Key, i);
            else if (kvp.Value is bool b) _animator.SetBool(kvp.Key, b);
        }

        // 백업한 레이어 가중치 복구 (방어 중 무기 변경 시 방어 안 풀림)
        for (int i = 0; i < layerWeights.Length && i < _animator.layerCount; i++)
        {
            _animator.SetLayerWeight(i, layerWeights[i]);
        }
    }

    //void CreateWeaponInSlot(int index)
    //{
    //    EquipmentData data = _currentUsingItems.GetWeaponData(index);
    //    if (data == null || data.WeaponPrefab == null) return;

    //    GameObject obj = Instantiate(data.WeaponPrefab, _handSocket);
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localRotation = Quaternion.identity;

    //    if (obj.TryGetComponent<Weapon>(out Weapon weapon))
    //    {
    //        _equippedWeapons[index] = weapon;
    //        obj.SetActive(false);
    //    }
    //}
    //void InitializeWeapons()
    //{
    //    for (int i = 0; i < _equippedWeapons.Length; i++)
    //    {
    //        if (_equippedWeapons[i] != null) continue;
    //        _quickSlotUI.AssignWeapon(i, _currentUsingItems.GetWeaponData(i));

    //        CreateWeaponInSlot(i);
    //    }
    //}
    ///*void DestroyCurrentWeapon()
    //{
    //    if (_currentInstance == null) return;
    //    Destroy(_currentInstance.gameObject);
    //    _currentInstance = null;
    //}*/

    //public void SwitchWeapon()
    //{
    //    int next = (_currentWeapon + 1) % 2;

    //    Debug.Log($"무기 교체 시도: {_currentWeapon} -> {next}");

    //    SetWeapon(next);
    //}



    //public void OnSlotDataChanged(int slotIndex, EquipmentData newData)
    //{
    //    Debug.Log(newData);
    //    Debug.Log(slotIndex);
    //    _quickSlotUI.AssignWeapon(slotIndex, newData);

    //    if (_equippedWeapons[slotIndex] != null)
    //    {
    //        Destroy(_equippedWeapons[slotIndex].gameObject);
    //        _equippedWeapons[slotIndex] = null;
    //    }

    //    // inventory에 물어보지 말고, 방금 받은 newData를 직접 넘기도록
    //    if (newData != null && newData.WeaponPrefab != null)
    //    {
    //        CreateWeaponDirectly(slotIndex, newData);
    //    }

    //    if (slotIndex == _currentWeapon)
    //        SetWeapon(_currentWeapon);
    //}
    //void CreateWeaponDirectly(int index, EquipmentData data)
    //{
    //    Debug.Log($"{data.ItemName} 모델 생성 시작!");
    //    GameObject obj = Instantiate(data.WeaponPrefab, _handSocket);
    //    obj.transform.localPosition = Vector3.zero;
    //    obj.transform.localRotation = Quaternion.identity;

    //    if (obj.TryGetComponent<Weapon>(out Weapon weapon))
    //    {
    //        _equippedWeapons[index] = weapon;
    //        obj.SetActive(false); // 일단 비활성화 (EquipFromSlot에서 켜줌)
    //    }
    //}
}