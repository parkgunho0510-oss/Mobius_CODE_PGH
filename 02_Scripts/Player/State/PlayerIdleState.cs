using UnityEngine;
using UnityEngine.Playables;

public class PlayerIdleState : PlayerState
{
    //생성자
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }


    public override void Enter()
    {
        player.CanRotate = true; // 제자리에 서서 시점 회전 가능
    }

    public override void LogicUpdate()
    {
        // 움직임 입력이 들어오면 Walk/Run 상태로 전환
        if (player.MoveInput.sqrMagnitude > 0.1f)
        {
            stateMachine.ChangeState(player.WalkState);
        }

        // 공격 입력이 들어오면 Attack 상태로 전환
        if (player.IsAttackTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.AttackState);
        }

        if (player.CharacterController.isGrounded && player.IsRollTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.RollState);
            return;
        }

        if (player.IsBlocking && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.DefenceState);
            return;
        }

        if (player.CharacterController.isGrounded && player.IsJumpTriggered)
        {
            stateMachine.ChangeState(player.JumpState); // 새로 만든 점프 상태로 전환
            return;
        }

        if (player.IsParryTriggered && player.Stats.CurrentStamina >= 30f)
        {
            stateMachine.ChangeState(player.ParryState);
            return;
        }

    }
}