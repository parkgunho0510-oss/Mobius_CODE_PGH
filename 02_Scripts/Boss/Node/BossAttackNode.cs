using UnityEngine;

public class BossAttackNode : BTNode
{
    protected BossBase _boss;
    protected string _animTriggerName;
    protected string _hitboxGroupName;

    protected bool _isAttacking;
    protected bool _isFinished;

    public BossAttackNode(BossBase boss, string animBoolName, string hitboxGroupName = "")
    {
        _boss = boss;
        _animTriggerName = animBoolName;
        _hitboxGroupName = hitboxGroupName;
    }

    public override NodeState Evaluate()
    {
        // 공격 시작 (Enter)
        if (!_isAttacking)
        {
            _isAttacking = true;
            _isFinished = false;

            _boss.Agent.isStopped = true;
            _boss.Agent.velocity = Vector3.zero;
            _boss.RotateToTarget();
            _boss.Animator.SetTrigger(_animTriggerName);

            var combat = _boss.GetComponent<MonsterCombat>();
            if (combat != null)
            {
                if (!string.IsNullOrEmpty(_hitboxGroupName))
                    combat.SetActiveHitboxGroup(_hitboxGroupName);

                combat.OnAttackFinished += FinishAttack; // 이벤트 구독
            }

            OnEnter(); // 자식 클래스 초기화용 가상 함수
        }

        // 공격 완료 시 (Exit)
        if (_isFinished)
        {
            ResetAttack();
            return State = NodeState.Success; // 성공 반환 -> 이후 쿨다운 노드로 넘어감
        }

        // 공격 진행 중 (Logic Update)
        UpdateAction(); // 돌진, 점프 등 특수 이동 처리용 가상 함수

        return State = NodeState.Running; // 안 끝났으면 트리를 붙잡아둠
    }

    private void FinishAttack() => _isFinished = true;

    private void ResetAttack()
    {
        _isAttacking = false;
        var combat = _boss.GetComponent<MonsterCombat>();
        if (combat != null) combat.OnAttackFinished -= FinishAttack; // 구독 해제
        OnExit();
    }

    public override void Abort()
    {
        base.Abort();
        if (_isAttacking)
        {
            var combat = _boss.GetComponent<MonsterCombat>();
            if (combat != null)
            {
              combat.ForceStopAttack();
            }
            ResetAttack();
        }
    }

    // 특수 공격 패턴을 구현하는 자식들을 위한 생명주기 메서드
    protected virtual void OnEnter() { }
    protected virtual void UpdateAction() { }
    protected virtual void OnExit() { }
}