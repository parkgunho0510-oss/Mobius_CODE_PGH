using System.Collections.Generic;
using UnityEngine;

public class Monster_Skeleton_Archer : MonsterBase
{
    [Header("Ranged Equipment")]
    [SerializeField] private Arrow _arrowPrefab;
    [SerializeField] private Transform _firePoint;

    protected override void Update()
    {
        base.Update();

        if (!IsDead && Player != null && FindTarget())
        {
            RotateToTarget();
        }
    }

    public override BTNode GetAttackNode()
    {
        return new Selector(new List<BTNode>
        {  
            // 체력이 낮으면 도망
            new FleeNode(this, safeDistance: 25f),

            // 거리가 확보되었다면 원거리 공격 실행
            new RangedAttackNode(this)
        });
    }

    // 이 메서드는 애니메이션 클립 Animation Event로 넣고 호출
    public void ShootArrow()
    {
        if (_arrowPrefab == null || _firePoint == null || Player == null) return;

        Arrow arrow = Instantiate(_arrowPrefab, _firePoint.position, _firePoint.rotation);
        
        // 플레이어의 약간 위를 향해 쏘도록 보정
        Vector3 targetPos = Player.transform.position + Vector3.up * 1.0f;
        Vector3 direction = (targetPos - _firePoint.position).normalized;
        
        arrow.Fire(direction, Damage);
        
        // 사운드가 필요하다면 추가
        // SoundEvents.RaiseSFXPlayedEvent(
    }
}