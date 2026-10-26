using System.Collections.Generic;
using UnityEngine;

public class OrcBoss : BossBase
{
    [Header("Orc Boss Settings")]
    public float ChargeSpeed = 15f;
    public float JumpSpeed = 1.5f;
    public float JumpHeight = 4f;
    public ParticleSystem GroundSmashParticle;

    public bool IsDashing { get; private set; }
    public bool IsJumping { get; private set; }

    protected override void InitBossTree()
    {
        var MeleeAttacks = new List<BTNode> {
            new BossAttackNode(this, "IsHeavySmash", "HandBox"),
            new BossAttackNode(this, "IsGroundSmash", "GroundBox")
        };
        var RangedAttacks = new List<BTNode> {
            new OrcChargeAttackNode(this),
            new OrcJumpAttackNode(this)
        };

        // 트리 조립
        var combatSelector = new Selector(new List<BTNode>
        {
            new BossCombatSelector(this, MeleeAttacks, RangedAttacks),
            // 공격 범위(1배수) 밖이면 보스가 직접 걸어가서 추적하도록 설정
            new BossChaseNode(this, 1f)
        });

        var combatLoopSequence = new Sequence(new List<BTNode>
        {
            new BossIdleNode(this),
            combatSelector,
            new BossCooldownNode(this, 2f)
        });

        _bossTree = new Selector(new List<BTNode>
        {
            new Sequence(new List<BTNode>
            {
                new ConditionNode(() => CurrentHp <= MaxHp * 0.5f),
                new ActionNode(() => { Animator.SetBool("IsPhase2", true); return NodeState.Success; }),
                combatLoopSequence // 2페이즈일때 사용할 것
            }),
            combatLoopSequence
        });
    }

    #region Animation Events
    public void Anim_DashStart() => IsDashing = true;
    public void Anim_DashEnd() => IsDashing = false;
    public void Anim_JumpStart() => IsJumping = true;
    public void Anim_JumpEnd() => IsJumping = false;

    public void Anim_SpawnPar()
    {
        if (GroundSmashParticle != null)
        {
            GroundSmashParticle.transform.position = transform.position;
            GroundSmashParticle.Play();
        }
    }
    #endregion
}