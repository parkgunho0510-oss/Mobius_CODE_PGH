using UnityEngine;

public class SkeletonComboAttackNode : BTNode
{
    private BossBase _boss;
    private MonsterCombat _combat;
    private int _comboStep = 0;

    private bool _isAttacking = false;
    private bool _isFinished = false;

    // 루트 모션 기존 상태를 기억할 변수
    private bool _originalRootMotion = false;

    public SkeletonComboAttackNode(BossBase boss)
    {
        _boss = boss;
        _combat = boss.GetComponent<MonsterCombat>();
    }

    public override NodeState Evaluate()
    {
        // 콤보 시작
        if (!_isAttacking)
        {
            _isAttacking = true;
            _isFinished = false;
            _comboStep = 1;

            // 1. 노드 진입 시: 현재 루트 모션 상태를 저장하고 강제로 끕니다.
            _originalRootMotion = _boss.Animator.applyRootMotion;
            _boss.Animator.applyRootMotion = false;

            _combat.OnAttackFinished += OnComboFinished;

            ExecuteHit();
        }

        // 3타까지 모두 끝난 경우
        if (_isFinished)
        {
            ResetNode();
            return State = NodeState.Success;
        }

        return State = NodeState.Running;
    }

    private void ExecuteHit()
    {
        if (_comboStep > 3)
        {
            _isFinished = true;
            return;
        }

        _boss.Agent.isStopped = true;
        _boss.Agent.velocity = Vector3.zero;

        // 2. 유니티 내장 함수를 사용하여 즉각 회전 (스냅)
        Vector3 targetLookPos = _boss.Player.transform.position;
        targetLookPos.y = _boss.transform.position.y; // 위아래로 꺾이는 현상 방지
        _boss.transform.LookAt(targetLookPos);

        // Animator 제어
        _boss.Animator.SetInteger("Combo", _comboStep);
        _boss.Animator.SetTrigger("IsCombo");

        _combat.SetActiveHitboxGroup("WeaponBox");
    }

    private void OnComboFinished()
    {
        _comboStep++;
        ExecuteHit();
    }

    private void ResetNode()
    {
        _isAttacking = false;
        _isFinished = false;
        _comboStep = 0;

        _boss.Animator.SetInteger("Combo", 0);
        _combat.OnAttackFinished -= OnComboFinished;

        // 3. 콤보 종료 시: 원래의 루트 모션 상태로 완벽하게 원상복구합니다.
        _boss.Animator.applyRootMotion = _originalRootMotion;
    }

    public override void Abort()
    {
        base.Abort();
        if (_isAttacking) ResetNode(); // Abort 시에도 ResetNode가 호출되어 루트 모션이 정상 복구됨
    }
}