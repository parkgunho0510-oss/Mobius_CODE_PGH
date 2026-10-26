using System.Collections.Generic;
using UnityEngine;

public class SkeletonBoss : BossBase
{
    [Header("Skeleton Boss Settings")]
    // 인스펙터에서 설정 가능하지만, 외부 스크립트에서 수정 불가
    [SerializeField] private float _chargeSpeed = 30f;

    // 외부(돌진 노드)에서는 읽기(Get)만 가능하도록 프로퍼티 노출
    public float ChargeSpeed => _chargeSpeed;

    public bool IsDashing { get; private set; }

    // --- 이펙트 관련 세팅 ---
    [System.Serializable]
    private struct CombatEffectInfo 
    {
        public GameObject Prefab;
        public Transform SpawnPoint;
        public bool AttachToParent;
        public float DestroyAfter;
    }

    [Header("Combo Effects")]
    [SerializeField] private CombatEffectInfo[] _comboEffects;

    protected override void InitBossTree()
    {
        var MeleeAttacks = new List<BTNode> {
            new BossAttackNode(this, "IsAttack", "WeaponBox"),
            new SkeletonComboAttackNode(this)
        };

        var RangedAttacks = new List<BTNode> {
            new SkeletonChargeAttackNode(this)
        };

        var combatSelector = new Selector(new List<BTNode>
        {
            new BossCombatSelector(this, MeleeAttacks, RangedAttacks),
            new BossChaseNode(this, 1f)
        });

        _bossTree = new Sequence(new List<BTNode>
        {
            new BossIdleNode(this),
            combatSelector,
            new BossCooldownNode(this, 2f)
        });
    }

    #region Animation Events
    public void Anim_DashStart() => IsDashing = true;
    public void Anim_DashEnd() => IsDashing = false;

    public void Anim_SpawnEffect(int index)
    {
        if (index < 0 || index >= _comboEffects.Length || _comboEffects[index].Prefab == null) return;

        var info = _comboEffects[index];
        GameObject fx = Instantiate(info.Prefab, info.SpawnPoint.position, info.SpawnPoint.rotation);

        if (info.AttachToParent)
        {
            fx.transform.SetParent(info.SpawnPoint);
        }

        if (info.DestroyAfter > 0f)
        {
            Destroy(fx, info.DestroyAfter);
        }
    }
    #endregion
}