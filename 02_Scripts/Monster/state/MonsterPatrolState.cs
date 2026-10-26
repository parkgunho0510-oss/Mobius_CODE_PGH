using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterPatrolState : MonsterState
{
    private Coroutine _coroutine;

    public MonsterPatrolState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = false;

        _monster.Animator.SetBool("IsMove", true);
        _coroutine = _monster.StartCoroutine(Co_Patrol());
    }

    private IEnumerator Co_Patrol()
    {
        Vector3 dest = _monster.transform.position
            + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        _monster.Agent.SetDestination(dest);

        yield return new WaitUntil(() => !_monster.Agent.pathPending);

        while (_monster.Agent.remainingDistance > _monster.Agent.stoppingDistance)
        {
            if (_monster.FindTarget())
            {
                _stateMachine.ChangeState(_monster.CombatState);
                yield break;
            }
            yield return null;
        }

        _stateMachine.ChangeState(_monster.IdleState);
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
    }
}