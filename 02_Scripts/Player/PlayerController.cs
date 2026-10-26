using UnityEngine;
using UnityEngine.InputSystem;
using Assets._02_Scripts.Player.State;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _rotateSpeed = 30f;
    public float MoveSpeed => _moveSpeed;

    [Header("Gravity Stats")]
    [SerializeField] private float _gravity = -9.81f; // �߷� ���ӵ�
    private float _verticalVelocity; // ���� ���� �ӵ�
    public float VerticalVelocity
    { get 
        {
         return _verticalVelocity;
        } 
        set
        { _verticalVelocity = value; }
    } // ���� �ӵ��� ������ �� �ֵ��� ������Ƽ
    public float Gravity => _gravity;

    [Header("Jump Stats")]
    public bool IsJumpTriggered { get; private set; } //���� Ʈ���� (������Ʈ ��)
    [SerializeField] private float _jumpHeight = 1.5f;
    public float JumpHeight => _jumpHeight;

    [Header("Roll Stats")]
    public bool IsRollTriggered { get; private set; }

    [Header("Parry Stats")]
    public bool IsParryTriggered { get; private set; }

    [Header("PickUp")]
    public bool IsPickUpTriggered { get; private set; }

    [Header("Inventory")]
    InputAction _inventoryOpenAction;
    InputAction _inventoryCloseAction;

    [Header("Interact")]
    InputAction _interactAction;

    [Header("Status")]
    InputAction _statusOpenAction;
    InputAction _statusCloseAction;

    [Header("Quest")]
    InputAction _questLogOpenAction;
    InputAction _questLogCloseAction;

    [Header("Ref")]
    [SerializeField] PlayerInteractor _playerInteractor;

    #region ������Ƽ
    public PlayerStatus Stats { get; private set; }
    [SerializeField] private CurrentUsingItems _currentItems;
    public CurrentUsingItems CurrentItems => _currentItems;
    public Vector3 CurrentMovement { get; set; } // �̹� �����ӿ� �̵��� ���� ����
    public bool CanRotate { get; set; } = true;  // ���� ���¿��� ȸ���� �������� ����
    public Vector2 LookInput { get; private set; }

    public Transform MainCamTransform { get; private set; }
    #endregion
    #region ������Ʈ ������
    // ������Ʈ ������
    public Animator Animator { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    #endregion
    #region FSM
    // FSM
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerDefenceState DefenceState { get; private set; }
    public PlayerRollState RollState { get; private set; }

    public PlayerParryState ParryState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    // ... ������, ��� ��� ���� �߰�
    #endregion
    #region �Է� ������
    // �Է� ������
    public Vector2 MoveInput { get; private set; }
    public bool IsAttackTriggered { get; private set; }
    public bool IsRun { get; private set; } // �޸��� ����

    public bool IsBlocking { get; private set; } // ��� ����

    public bool IsInputLocked { get; private set; }

    //public Vector2 SmoothedInput { get; private set; }
    //private Vector2 _smoothInputVelocity;
    #endregion

    private void Awake()
    {
        // init
        _playerInteractor = GetComponent<PlayerInteractor>();
        Stats = GetComponent<PlayerStatus>();
        Animator = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        PlayerInput = GetComponent<PlayerInput>();
        if (_currentItems == null)
        {
            _currentItems = FindFirstObjectByType<CurrentUsingItems>();
        }
        if (Camera.main != null)
        {
            MainCamTransform = Camera.main.transform;
        }
        #region FSM 

        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine);
        WalkState = new PlayerWalkState(this, StateMachine);
        AttackState = new PlayerAttackState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        RollState = new PlayerRollState(this, StateMachine);
        DefenceState = new PlayerDefenceState(this, StateMachine);
        ParryState = new PlayerParryState(this, StateMachine);    
        HitState = new PlayerHitState(this, StateMachine);
        #endregion

        if(PlayerInput != null)
        {
            _inventoryOpenAction    = PlayerInput.actions["InventoryOpen"];
            _inventoryCloseAction   = PlayerInput.actions["InventoryClose"];
            _interactAction         = PlayerInput.actions["Interact"];
            _statusOpenAction       = PlayerInput.actions["StatusOpen"];
            _statusCloseAction      = PlayerInput.actions["StatusClose"];
            _questLogOpenAction     = PlayerInput.actions["QuestLogOpen"];
            _questLogCloseAction    = PlayerInput.actions["QuestLogClose"];

            _inventoryOpenAction.performed  += OnInventoryOpen;
            _inventoryCloseAction.performed += OnInventoryClose;
            _interactAction.performed       += OnTryInteract;
            _statusOpenAction.performed     += OnStatusOpen;
            _statusCloseAction.performed    += OnStatusClose;
            _questLogOpenAction.performed   += OnQuestLogOpen;
            _questLogCloseAction.performed  += OnQuestLogClose;

            PlayerInput.SwitchCurrentActionMap("Player");
        }


        UIEvents.RaiseCursorHid();
    }

    private void Start()
    {
        // ���� ���� ����
        StateMachine.Initialize(IdleState);

        if (DungeonSceneManager.Instance != null)
        {
            DungeonSceneManager.Instance.DSMRegisterPlayer(this);
        }

    }

    private void OnEnable()
    {
        // PlayerInput�� �ʱ�ȭ�� ���¶��, ���� �׼� ���� ������ ���� (���� ����)
        if (PlayerInput != null && PlayerInput.currentActionMap != null)
        {
            PlayerInput.currentActionMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (PlayerInput != null && PlayerInput.currentActionMap != null)
        {
            PlayerInput.currentActionMap.Disable();
        }
    }
    private void Update()
    {
       //UI Open State
        if (IsInputLocked)
        {
            CurrentMovement = Vector3.zero;
            UpdateAnimator();
            return;
        }

        if (Stats.IsDead)
        {
            ApplyRotation();
            return;
        }


        ReadInput();

        ApplyGravity();

        CurrentMovement = Vector3.zero;

        // ���� ������ ������ �����
        StateMachine.CurrentState.LogicUpdate();

        ApplyRotation();
        ApplyMovement();
        TryPickUp();
        UpdateAnimator();
    }

    private void ReadInput()
    {
        MoveInput = PlayerInput.actions["Move"].ReadValue<Vector2>();
        LookInput = PlayerInput.actions["Look"].ReadValue<Vector2>();
        IsAttackTriggered = PlayerInput.actions["Attack"].triggered;
        IsJumpTriggered = PlayerInput.actions["Jump"].triggered;
        IsRun = PlayerInput.actions["Sprint"].IsPressed();
        IsRollTriggered = PlayerInput.actions["Roll"].triggered;
        IsBlocking = PlayerInput.actions["Block"].IsPressed();
        IsParryTriggered = PlayerInput.actions["Parry"].triggered;        
        IsPickUpTriggered = PlayerInput.actions["PickUp"].triggered;
        //SmoothedInput = Vector2.SmoothDamp(SmoothedInput, MoveInput, ref _smoothInputVelocity, 0.1f);
        //// 0�̳� 1�� ��� Ƣ�� �ʰ�, �� 0.1�ʿ� ���� ��ǥ �Է°�(MoveInput)�� ���� �ε巴�� ���� ��ȭ
        //// ref _smoothInputVelocity�� ���� �������� �ӵ��� ��� �ε巴�� �������� ����

        //if (SmoothedInput.sqrMagnitude < 0.0001f) //���� �ӵ� �ʱ�ȭ (�Ķ���� �� �ʼ��� �ƴ�)
        //{
        //    SmoothedInput = Vector2.zero;
        //    _smoothInputVelocity = Vector2.zero; // �ӵ��� �Բ� �ʱ�ȭ
        //}
    }

    private void ApplyGravity()
    {
        //�߷�
        if (CharacterController.isGrounded)
            //ĳ���� ��Ʈ�ѷ� ���� ��ü���� �׶��� üĿ
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

    //  ���� �̵��� �߷��� ���� �̵� ����
    private void ApplyMovement()
    {
        Vector3 finalVelocity = CurrentMovement;
        finalVelocity.y = _verticalVelocity; // �߷� �ջ�

        CharacterController.Move(finalVelocity * Time.deltaTime);
    }

    //  ȸ�� ����
    private void ApplyRotation()
    {
        // ���°� ȸ���� ����߰�, ȸ�� �Է°��� ���� ���� ȸ��
        if (CanRotate && MoveInput.sqrMagnitude > 0.01f)
        {
            Vector3 dir = CameraForward;

            if (dir != Vector3.zero)
            {
                // ��ǥ ȸ���� ��� (ī�޶� ���� ��)
                Quaternion targetRotation = Quaternion.LookRotation(dir);

                // �ε巴�� ĳ���� ȸ��
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
            }
        }
    }
    private void TryPickUp()
    {
        if(IsPickUpTriggered) _playerInteractor.TryPickUp();
    }
    //  �ִϸ����� �Ķ���� ������Ʈ
    private void UpdateAnimator()
    {
        float speed = 1f;
        if (!IsBlocking && IsRun && Stats.CurrentStamina > 0)
            speed = 2f;

        float targetY = MoveInput.y * speed;
        float targetX = MoveInput.x * speed;

        Animator.SetFloat("Forward", targetY, 0.1f, Time.deltaTime);
        Animator.SetFloat("Side", targetX, 0.1f, Time.deltaTime);
    }

    #region Event
    public void OnInventoryOpen(InputAction.CallbackContext callback)
    {
        UIEvents.RaiseInventoryOpened();
        UIEvents.RaiseCursorShowed();
        PlayerInput.SwitchCurrentActionMap("UI");
    }
    public void OnInventoryClose(InputAction.CallbackContext callback)
    {
        UIEvents.RaiseInventoryClosed();
        UIEvents.RaiseCursorHid();
        PlayerInput.SwitchCurrentActionMap("Player");
    }
    public void OnTryInteract(InputAction.CallbackContext callback)
    {
        if(_playerInteractor.TryInteract())
        {
            UIEvents.RaiseCursorShowed();
            PlayerInput.SwitchCurrentActionMap("UI");
        }
    }
    public void OnStatusOpen(InputAction.CallbackContext context)
    {
        UIEvents.RaiseStatusOpened();
        UIEvents.RaiseCursorShowed();
        PlayerInput.SwitchCurrentActionMap("UI");
    }
    public void OnStatusClose(InputAction.CallbackContext context)
    {
        UIEvents.RaiseStatusClosed();
        UIEvents.RaiseCursorHid();
        PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void OnQuestLogOpen(InputAction.CallbackContext context)
    {
        QuestEvents.RaiseQuestLogOpened();
        UIEvents.RaiseCursorShowed();
        PlayerInput.SwitchCurrentActionMap("UI");
    }
    public void OnQuestLogClose(InputAction.CallbackContext context)
    {
        QuestEvents.RaiseQuestLogClosed();
        UIEvents.RaiseCursorHid();
        PlayerInput.SwitchCurrentActionMap("Player");
    }
    #endregion
    #region Animation_Event
    // �ִϸ��̼� �̺�Ʈ ��Ŀ: �޺� ���� ����
    public void Anim_OpenCombo()
    {
        if (StateMachine.CurrentState == AttackState)
        {
            AttackState.OpenCombo();
            
        }
    }

    // �ִϸ��̼� �̺�Ʈ ��Ŀ: �޺� �Ǵ� �� ���� ����
    public void Anim_CloseCombo()
    {
        if (StateMachine.CurrentState == AttackState)
        {
            AttackState.CloseCombo();
            
        }
    }

    // �ִϸ��̼� �̺�Ʈ ��Ŀ : ���ڱ� �Ҹ�
    /*public void Anim_Footstep()
    {
        
        if (CharacterController.isGrounded)
        {
            // �޸��� ���̸� ���� �� ū �Ҹ�, �ƴϸ� ���� �߼Ҹ�
            string stepSound = IsRun ? "Footstep_1" : "Footstep_2";
            SoundEvents.RaiseSFXPlayedEvent(stepSound);
        }
    }*/
    #endregion

    #region ī�޶� ���� ���� �޼���

    // ȸ����
    public Vector3 CameraForward
    {
        get
        {
            if (MainCamTransform == null) return transform.forward;
            Vector3 forward = MainCamTransform.forward;
            forward.y = 0f;
            return forward.normalized;
        }
    }

    // �Է°�(WASD)�� ������ ī�޶� ������ �̵� ���͸� ����ؼ� ��ȯ �̵���
    public Vector3 GetCameraDirection(Vector2 input)
    {
        if (MainCamTransform == null) return Vector3.zero;

        Vector3 right = MainCamTransform.right;
        right.y = 0f;
        right.Normalize();

        return (CameraForward * input.y + right * input.x).normalized;
    }

    #endregion
}