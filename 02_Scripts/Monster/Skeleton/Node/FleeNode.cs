using UnityEngine;
using UnityEngine.AI;

public class FleeNode : BTNode
{
    //도망 노드
    private MonsterBase _monster;

    // 상태 관리 변수들
    private bool _isUsed = false;     // 1회용 플래그
    private bool _isFleeing = false;  // 현재 도망 중인지
    private float _fleeTimer = 0f;    // 도망친 시간
    private float _safeDistance;      // 이만큼 멀어지면 도망 종료

    public FleeNode(MonsterBase monster, float safeDistance = 10f)
    {
        _monster = monster;
        _safeDistance = safeDistance;
    }

    public override NodeState Evaluate()
    {
        // [1회용 처리]
        if (_isUsed) return State = NodeState.Failure;

        // 도망 중이 아닌데 체력이 30% 이상이면 평소처럼 전투
        if (!_isFleeing && _monster.CurrentHp > _monster.MaxHp * 0.3f)
        {
            return State = NodeState.Failure;
        }

        // 최초 도망 시작 세팅
        if (!_isFleeing)
        {
            _isFleeing = true;
            _fleeTimer = 0f;

            _monster.Agent.isStopped = false;
            _monster.Agent.speed = _monster.MoveSpeed * 1.5f; // 1.5배 속도로 도망

            _monster.Agent.updateRotation = false;
            _monster.Animator.SetBool("IsMove", true);
        }

        _fleeTimer += Time.deltaTime;
        float distToPlayer = _monster.DistanceToPlayer();

        // 충분히 멀리 도망쳤다면 도망 성공
        if (distToPlayer >= _safeDistance)
        {
            _monster.Animator.SetBool("IsMove", false);
            EndFlee(NodeState.Success);
            return State;
        }

        // 도망치는데 플레이어가 사거리 안으로 쫓아옴
        if (_fleeTimer > 0.5f && distToPlayer <= _monster.AttackRange)
        {
            // 도망 실패 처리 -> 트리에서 다음 순위인 '공격 노드'가 즉시 실행되어 반격함
            EndFlee(NodeState.Failure);
            return State;
        }

        // --- 도망  로직 ---

        // 플레이어 반대 방향으로 목적지 갱신
        Vector3 dirAwayFromPlayer = (_monster.transform.position - _monster.Player.transform.position).normalized;
        dirAwayFromPlayer.y = 0;

        Vector3 fleeTarget = _monster.transform.position + (dirAwayFromPlayer * 5f);
        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            _monster.Agent.SetDestination(hit.position);
        }

        // 이동은 뒤로 하지만, 회전은 플레이어를 향해 Slerp
        Vector3 dirToPlayer = (_monster.Player.transform.position - _monster.transform.position).normalized;
        dirToPlayer.y = 0;
        if (dirToPlayer != Vector3.zero)
        {
            _monster.transform.rotation = Quaternion.Slerp(
                _monster.transform.rotation,
                Quaternion.LookRotation(dirToPlayer),
                Time.deltaTime * _monster.TurnSpeed);
        }

        return State = NodeState.Running;
    }

    // 도망이 끝나거나 강제 취소될 때 원상복구 해주는 함수
    private void EndFlee(NodeState finalState)
    {
        _isUsed = true; // 1회용
        _isFleeing = false;

        if (_monster != null && _monster.Agent != null && _monster.Agent.isOnNavMesh)
        {
            _monster.Agent.speed = _monster.MoveSpeed;   // 속도 원상복구
            _monster.Agent.updateRotation = true;        // 자동 회전 원상복구

            _monster.Agent.ResetPath();
            _monster.Agent.velocity = Vector3.zero;

            _monster.Agent.isStopped = true;
        }

        State = finalState;
    }

    // 스턴이나 사망 시 불림
    public override void Abort()
    {
        base.Abort();
        if (_isFleeing) EndFlee(NodeState.Failure);
    }
}