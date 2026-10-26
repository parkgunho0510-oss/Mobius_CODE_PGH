using UnityEngine;

public class BossIdleNode : BTNode
{
    private BossBase _boss;

    public BossIdleNode(BossBase boss)
    {
        _boss = boss;
    }

    public override NodeState Evaluate()
    {
        // 플레이어를 발견하면 성공(Success) 반환 -> 시퀀스/셀렉터가 다음 노드로(추적 등) 넘김
        if (_boss.FindTarget())
        {
            return State = NodeState.Success;
        }

        // 아직 발견 못 했으면 계속 대기(Running)
        return State = NodeState.Running;
    }
}