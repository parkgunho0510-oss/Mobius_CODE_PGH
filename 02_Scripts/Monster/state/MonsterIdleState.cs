using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterIdleState : MonsterState
{
    private float _idleTime = 1.5f;
    private Coroutine _coroutine;

    public MonsterIdleState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = false;

        _monster.Animator.SetBool("IsMove", false);
        _coroutine = _monster.StartCoroutine(Co_Idle());
    }

    private IEnumerator Co_Idle()
    {
        yield return new WaitForSeconds(_idleTime);
        _stateMachine.ChangeState(_monster.PatrolState);
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
    }
}