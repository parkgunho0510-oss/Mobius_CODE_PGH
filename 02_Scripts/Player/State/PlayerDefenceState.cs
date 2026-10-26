using UnityEngine;

public class PlayerDefenceState : PlayerState
{
    private int upperBodyLayerIndex;

    public PlayerDefenceState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.CanRotate = true; // 방어 중에도 방향 전환은 가능

        upperBodyLayerIndex = player.Animator.GetLayerIndex("Deffensive");

        if (upperBodyLayerIndex != -1)
            player.Animator.SetLayerWeight(upperBodyLayerIndex, 1.0f);
    }

    public override void LogicUpdate()
    {
        if (!player.IsBlocking || player.Stats.CurrentStamina <= 0)
        {
            if (player.MoveInput.sqrMagnitude > 0.001f)
                stateMachine.ChangeState(player.WalkState);
            else
                stateMachine.ChangeState(player.IdleState);
            return;
        }
        if (player.IsParryTriggered && player.Stats.CurrentStamina >= 30f)
        {
            stateMachine.ChangeState(player.ParryState);
            return;
        }

        float speed = player.MoveSpeed * 0.7f;

        if (player.MoveInput.sqrMagnitude > 0.001f)
        {
            Vector3 dir = player.GetCameraDirection(player.MoveInput);
            player.CurrentMovement = dir * speed;
        }
        else
        {
            player.CurrentMovement = Vector3.zero;
        }
    }

    public override void Exit()
    {
        if (upperBodyLayerIndex != -1)
            player.Animator.SetLayerWeight(upperBodyLayerIndex, 0.0f);
    }
}