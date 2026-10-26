using UnityEngine;

public class BossCooldownNode : BTNode
{
    private BossBase _boss;
    private float _delayTime;
    private float _timer;
    private bool _isInitialized = false;

    public BossCooldownNode(BossBase boss, float delayTime = 3f)
    {
        _boss = boss;
        _delayTime = delayTime;
    }

    public override NodeState Evaluate()
    {
        // 쿨타임 진입 시 초기화 (Running 상태가 아니거나 처음 진입할 때)
        if (!_isInitialized || State != NodeState.Running)
        {
            _timer = 0f;
            _isInitialized = true;

            // 이동 중지 및 속도 초기화
            _boss.Agent.isStopped = true;
            _boss.Agent.velocity = Vector3.zero;
        }

        _timer += Time.deltaTime;

        _boss.RotateToTarget();

        // 설정된 쿨타임 시간이 지나면 성공 반환
        if (_timer >= _delayTime)
        {
            _isInitialized = false; // 다음 진입을 위해 초기화 플래그 리셋
            return State = NodeState.Success;
        }

        // 쿨타임 중에는 계속 Running 상태 유지
        return State = NodeState.Running;
    }

    public override void Abort()
    {
        base.Abort();
        _isInitialized = false;
        _boss.Agent.isStopped = true;
    }
}