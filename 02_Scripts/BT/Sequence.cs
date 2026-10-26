using System.Collections.Generic;

public class Sequence : BTNode
{
    private List<BTNode> _nodes;
    private int _currentIndex = 0;

    public Sequence(List<BTNode> nodes)
    {
        _nodes = nodes;
    }

    // 시퀀스는 실패 시 멈춤
    public override NodeState Evaluate()
    {
        // while을 사용하여 현재 실행 중인 인덱스부터 평가를 재개
        while (_currentIndex < _nodes.Count)
        {
            switch (_nodes[_currentIndex].Evaluate())
            {
                case NodeState.Running:
                    // 진행 중인 행동이 있으면 트리를 붙잡아두고 인덱스 유지
                    return State = NodeState.Running;

                case NodeState.Success:
                    // 성공했으면 다음 리스트에 있는 행동으로 넘어감
                    _currentIndex++;
                    continue;

                case NodeState.Failure:
                    // 하나라도 실패하면 시퀀스 전체 실패 및 인덱스 초기화
                    _currentIndex = 0;
                    return State = NodeState.Failure;
            }
        }

        // 리스트의 모든 행동을 실패 없이 끝마쳤다면 최종 성공
        _currentIndex = 0;
        return State = NodeState.Success;
    }

    public override void Abort()
    {
        base.Abort();
        // 전체 노드가 아닌 현재 실행 중이던 노드만 정확히 중단
        if (_currentIndex < _nodes.Count)
        {
            _nodes[_currentIndex].Abort();
        }
        _currentIndex = 0;
    }
}