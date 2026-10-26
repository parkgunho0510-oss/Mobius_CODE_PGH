using System;

// 외부에서 전달된 함수를 실행하고 그 결과(NodeState)를 그대로 반환
public class ActionNode : BTNode
{
    private Func<NodeState> _action;

    public ActionNode(Func<NodeState> action)
    {
        _action = action;
    }

    public override NodeState Evaluate()
    {
        return State = _action();
    }
}