using UnityEngine;

public class MonsterCombatState : MonsterState
{
    private MonsterCombatTree _combatTree;

    public MonsterCombatState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine)
    {
        // 이 상태가 생성될 때 전투 전용 행동 트리도 함께 생성합니다.
    }

    public override void Enter()
    {
        if (_combatTree == null)
        {
            _combatTree = new MonsterCombatTree(_monster);
        }

        _monster.Agent.isStopped = false;

        _monster.Agent.stoppingDistance = Mathf.Max(0, _monster.AttackRange - 1.0f);
    }

    public override void LogicUpdate()
    {
        // FSM은 이 상태를 유지하고, 실제 판단은 BT에게 맡깁니다.
        // BT가 Running, Success, Failure 중 무엇을 반환하든 
        // '전투 상황'인 한 계속 Evaluate를 호출합니다.
        _combatTree.Evaluate();

        // [FSM의 역할: 강제 상태 전환 판단]
        // 만약 플레이어가 너무 멀어져서 추격 범위를 벗어남
        if (_monster.DistanceToPlayer() > _monster.DetectRange)
        {
            _stateMachine.ChangeState(_monster.PatrolState);
        }
    }

    public override void Exit()
    {
        // 전투 상태를 나갈 때 진행 중인 BT 로직 중단 처리
        _combatTree.Abort();
        _monster.Agent.isStopped = true;

        _monster.Agent.stoppingDistance = 0f;
    }
}