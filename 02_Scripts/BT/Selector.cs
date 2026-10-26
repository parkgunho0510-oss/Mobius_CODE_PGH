using System.Collections.Generic;

public class Selector : BTNode
{
    private List<BTNode> _nodes;
    private int _currentIndex = 0; // 상태 기억을 위한 인덱스 추가

    public Selector(List<BTNode> nodes)
    {
        _nodes = nodes;
    }

    // 셀렉터는 성공 시 멈춤
    public override NodeState Evaluate()
    {
        while (_currentIndex < _nodes.Count)
        {
            switch (_nodes[_currentIndex].Evaluate())
            {
                case NodeState.Running:
                    // 진행 중인 행동이 있으면 트리를 붙잡아두고 인덱스 유지
                    return State = NodeState.Running;

                case NodeState.Success:
                    // 하나라도 성공하면 셀렉터 전체 성공 및 인덱스 초기화
                    _currentIndex = 0;
                    return State = NodeState.Success;

                case NodeState.Failure:
                    // 실패하면 다음 자식 노드 평가
                    _currentIndex++;
                    continue;
            }
        }

        // 다 실패하면 Failure 반환
        _currentIndex = 0;
        return State = NodeState.Failure;
    }

    public override void Abort()
    {
        base.Abort();
        if (_currentIndex < _nodes.Count)
        {
            _nodes[_currentIndex].Abort();
        }
        _currentIndex = 0;
    }
}