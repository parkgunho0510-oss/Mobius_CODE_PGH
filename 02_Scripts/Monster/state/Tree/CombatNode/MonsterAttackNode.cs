using System.Collections;
using UnityEngine;

public abstract class MonsterAttackNode : BTNode
{
    protected MonsterBase _monster;
    protected MonsterCombat _combat;
    protected Coroutine _coroutine;
    protected bool _isAttacking = false;
    protected bool _isAnimationFinished = false;

    protected float _delay = 0f;

    public MonsterAttackNode(MonsterBase monster)
    {
        _monster = monster;
        _combat = monster.GetComponent<MonsterCombat>();
    }

    public override NodeState Evaluate()
    {
        if (!_isAttacking && _monster.DistanceToPlayer() > _monster.AttackRange)
            return State = NodeState.Failure;

        if (!_isAttacking)
        {
            _isAttacking = true;
            _isAnimationFinished = false;

            if (_combat != null) _combat.OnAttackFinished += OnAnimationComplete;

            _coroutine = _monster.StartCoroutine(Co_AttackTemplate());
            return State = NodeState.Running;
        }

        return State = NodeState.Running;
    }

    private void OnAnimationComplete()
    {
        _isAnimationFinished = true;
    }

    private IEnumerator Co_AttackTemplate()
    {
        _monster.Agent.isStopped = true;
        _monster.Agent.velocity = Vector3.zero;

        _monster.Animator.SetBool("IsMove", false);

        //  자식 클래스의 고유 공격 로직 실행
        yield return Co_ExecuteAttack();

        // Anim_Close 이벤트가 올 때까지 대기
        yield return new WaitUntil(() => _isAnimationFinished);

        // 후딜레이
        if (_delay > 0f)
        {
            yield return new WaitForSeconds(_delay);
        }

        if (_combat != null) _combat.OnAttackFinished -= OnAnimationComplete;

        _isAttacking = false;
        _coroutine = null;
        State = NodeState.Success;

        _monster.Agent.isStopped = false;
    }

    protected abstract IEnumerator Co_ExecuteAttack();

    public override void Abort()
    {
        base.Abort(); 

        if (_isAttacking)
        {
            if (_coroutine != null)
            {
                _monster.StopCoroutine(_coroutine);
                _coroutine = null;
            }

            if (_combat != null)
            {
                // 히트박스 판정 및 캐스팅 강제 종료
                _combat.ForceStopAttack();
            }

            ResetNode();
        }
    }
    private void ResetNode()
    {
        if (_combat != null) _combat.OnAttackFinished -= OnAnimationComplete;
        _isAttacking = false;
        _isAnimationFinished = false;
    }
}