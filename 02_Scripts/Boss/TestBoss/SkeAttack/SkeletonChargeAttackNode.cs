using UnityEngine;

public class SkeletonChargeAttackNode : BossAttackNode
{
    private SkeletonBoss _skeletonBoss;
    private Vector3 _dashDirection;
    private float _remainingDistance;
    private bool _hasTriggeredEnd;

    public SkeletonChargeAttackNode(BossBase boss) : base(boss, "IsCharge", "BodyBox")
    {
        _skeletonBoss = boss as SkeletonBoss;
    }

    protected override void OnEnter()
    {
        _hasTriggeredEnd = false;

        // 타겟의 평면 위치 계산 (y축 무시)
        Vector3 targetPos = _boss.Player.transform.position;
        targetPos.y = _boss.transform.position.y;

        // 돌진 방향 고정
        _dashDirection = (targetPos - _boss.transform.position).normalized;

        // 목표 지점
        _remainingDistance = Vector3.Distance(_boss.transform.position, targetPos);

        // 애초에 거리가 1f보다 가깝다면 이동하지 않도록 0으로 보정
        if (_remainingDistance < 0f)
        {
            _remainingDistance = 0f;
        }
    }

    protected override void UpdateAction()
    {
        // 이미 종료 모션으로 넘어갔다면 이동 스킵
        if (_hasTriggeredEnd) return;

        if (_skeletonBoss.IsDashing)
        {
            // 남은 거리가 0 이하면 돌진 종료
            if (_remainingDistance <= 0f)
            {
                StopCharge();
                return;
            }

            // 이번 프레임에 이동할 거리 계산
            float moveStep = _skeletonBoss.ChargeSpeed * Time.deltaTime;

            // 남은 거리보다 이동량이 크면, 딱 남은 거리까지만 이동하도록 보정
            if (moveStep > _remainingDistance)
            {
                moveStep = _remainingDistance;
            }

            _boss.Controller.Move(_dashDirection * moveStep);
            _boss.Agent.nextPosition = _boss.transform.position; // NavMesh 동기화

            _remainingDistance -= moveStep;
        }
    }

    private void StopCharge()
    {
        _hasTriggeredEnd = true;

        // 애니메이터에 '돌진 종료' 트리거 전달 (진행중 모션 -> 끝 모션으로 전환)
        _boss.Animator.SetTrigger("ChargeEnd");
    }
}