using UnityEngine;

public class OrcChargeAttackNode : BossAttackNode
{
    private OrcBoss _orcBoss;
    private Vector3 _dashDirection;

    public OrcChargeAttackNode(BossBase boss) : base(boss, "IsCharge", "BodyBox")
    {
        _orcBoss = boss as OrcBoss;
    }

    protected override void OnEnter()
    {
        // 돌진할 방향을 미리 고정 (유도탄 방지)
        _dashDirection = (_boss.Player.transform.position - _boss.transform.position).normalized;
        _dashDirection.y = 0;
    }

    protected override void UpdateAction()
    {
        if (_orcBoss.IsDashing)
        {
            _boss.Controller.Move(_dashDirection * _orcBoss.ChargeSpeed * Time.deltaTime);
            _boss.Agent.nextPosition = _boss.transform.position; // NavMesh 동기화
        }
    }
}