using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterStunState : MonsterState
{
    private float _duration;
    private Coroutine _coroutine;

    public MonsterStunState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public void SetDuration(float duration) => _duration = duration;

    public override void Enter()
    {
        _monster.Agent.isStopped = true;
        _monster.Agent.velocity = Vector3.zero;
        _coroutine = _monster.StartCoroutine(Co_Stun());
    }

    private IEnumerator Co_Stun()
    {
        _monster.Animator.speed = 0f;
        _monster.Animator.SetTrigger("Stunned");
        _monster.Animator.speed = 1f;

        yield return new WaitForSeconds(_duration);

        if (_monster.DistanceToPlayer() <= _monster.AttackRange)
            _stateMachine.ChangeState(_monster.CombatState);
        else
            _stateMachine.ChangeState(_monster.IdleState);
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
        _monster.Agent.isStopped = false;
    }
}