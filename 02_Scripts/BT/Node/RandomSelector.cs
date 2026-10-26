using System.Collections.Generic;
using UnityEngine;

// 자식 노드 중 하나를 랜덤하게 골라서 실행하는 셀렉터
public class RandomSelector : BTNode
{
    private List<BTNode> _nodes;
    private BTNode _currentNode;

    public RandomSelector(List<BTNode> nodes)
    {
        _nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        if (_currentNode == null)
        {
            int rand = Random.Range(0, _nodes.Count);
            _currentNode = _nodes[rand];
        }

        NodeState result = _currentNode.Evaluate();

        // 행동이 끝났거나(Success) 조건이 안 맞아 실패(Failure)하면 노드 초기화
        if (result != NodeState.Running)
        {
            _currentNode = null; 
            return State = result;
        }

        return State = NodeState.Running;
    }

    public override void Abort()
    {
        base.Abort();
        _currentNode?.Abort();
        _currentNode = null;
    }
}