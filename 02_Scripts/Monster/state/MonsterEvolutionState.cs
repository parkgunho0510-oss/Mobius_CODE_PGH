using System.Collections;
using System.Threading;
using UnityEngine;

public class MonsterEvolutionState : MonsterState
{
    private Coroutine _coroutine;

    public MonsterEvolutionState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.Agent.isStopped = true;
        _coroutine = _monster.StartCoroutine(Co_Evolution());
    }

    protected virtual IEnumerator Co_Evolution()
    {
        yield return new UnityEngine.WaitForSeconds(2.0f);

        _monster.ApplyMonsterData(_monster.EvolutionData);
        _monster.EvolutionData = null;

        float dist = _monster.DistanceToPlayer();
        if (dist <= _monster.AttackRange)
            _stateMachine.ChangeState(_monster.CombatState);
        else if (dist <= _monster.DetectRange)
            _stateMachine.ChangeState(_monster.CombatState);
        else
            _stateMachine.ChangeState(_monster.IdleState);
    }

    public override void Exit()
    {
        if (_coroutine != null) _monster.StopCoroutine(_coroutine);
    }
}