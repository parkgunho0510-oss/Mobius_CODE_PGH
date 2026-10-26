using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterDamagedState : MonsterState
{
    private Coroutine _coroutine;

    public MonsterDamagedState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = true;
        _monster.Agent.velocity = UnityEngine.Vector3.zero;

        _monster.Animator.SetTrigger("Hit");
        _coroutine = _monster.StartCoroutine(Co_Damaged());
    }

    private IEnumerator Co_Damaged()
    {
        yield return new UnityEngine.WaitForSeconds(0.5f);

        if (_monster.CurrentHp <= 20 && _monster.EvolutionData != null)
        {
            _stateMachine.ChangeState(_monster.EvolutionState);
        }
        else
        {
            _stateMachine.ChangeState(_monster.CombatState);
        }
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
        _monster.Agent.isStopped = false;
    }
}