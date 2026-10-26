using UnityEngine;

public class PlayerParryState : PlayerState
{
    private float _parryDuration = 1.2f; // 패링 애니메이션 전체 길이
    private float _timer;

    public PlayerParryState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.ResetTrigger("Parry");

        player.CanRotate = false;
        _timer = 0f;
        player.CurrentMovement = Vector3.zero;

        player.Stats.SpendStamina(30f);

        player.Animator.SetTrigger("Parry");
    }

    public override void LogicUpdate()
    {
        _timer += Time.deltaTime;

        // 패링 동작이 완전히 끝나면 Idle로 복귀
        if (_timer >= _parryDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        player.CanRotate = true;

        player.Animator.ResetTrigger("Parry");
    }
}