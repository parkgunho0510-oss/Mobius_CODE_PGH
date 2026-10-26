public enum NodeState
{
    Running, // 행동 진행 중
    Success, // 행동 성공(완료)
    Failure  // 행동 실패(조건 안 맞음)
}

public abstract class BTNode
{
    public NodeState State { get; protected set; }
    public abstract NodeState Evaluate();

    //코루틴을 쓰는 노드는 여기서 정지
    public virtual void Abort()
    {
        State = NodeState.Failure;
    }
}