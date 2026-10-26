using System;

// 조건이 참이면 Success, 거짓이면 Failure를 반환합니다. (진행 중 상태 없음)
public class ConditionNode : BTNode
{
    private Func<bool> _condition;

    public ConditionNode(Func<bool> condition)
    {
        _condition = condition;
    }

    public override NodeState Evaluate()
    {
        return State = _condition() ? NodeState.Success : NodeState.Failure;
    }
}