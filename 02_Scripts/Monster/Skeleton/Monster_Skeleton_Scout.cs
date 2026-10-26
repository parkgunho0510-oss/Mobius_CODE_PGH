using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster_Skeleton_Scout : MonsterBase
{
    [Header("Equipment")]
    [SerializeField] private MonsterCombat _combat;

    public MonsterCombat Combat => _combat;

    // 이 몬스터는 '정찰병 전용 근접 공격 노드'를 사용한다고 선언
    public override BTNode GetAttackNode()
    {
        return new Selector(new List<BTNode>
        {  
            // 체력/거리 조건을 확인하고, 맞으면 도망 (10m)
            new FleeNode(this, safeDistance: 10f),

            // 1순위(도망) 조건이 안 맞아서 Failure가 떨어지면 공격 실행
            new MeleeAttackNode(this)
        });
    }
}