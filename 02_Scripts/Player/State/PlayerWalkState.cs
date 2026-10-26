using UnityEngine;

public class PlayerWalkState : PlayerState
{
    //생성자
    public PlayerWalkState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // 걷기 상태 진입 시 특별히 트리거할 애니메이션이 있다면 호출

        player.CanRotate = true; //회전 가능
    }

    public override void LogicUpdate()
    {


        // 상태 전환 체크 (우선순위가 높은 액션부터 검사)
        if (player.IsAttackTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.AttackState);
            return; // 전환 후 아래 이동 로직을 막기 위해 return
        }

        // 입력값이 거의 없으면 Idle 상태로 전환
        if (player.MoveInput.sqrMagnitude <= 0.001f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (player.CharacterController.isGrounded && player.IsRollTriggered && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.RollState); //구르기
            return;
        }

        if (player.CharacterController.isGrounded && player.IsJumpTriggered)
        {
            stateMachine.ChangeState(player.JumpState); //점프 상태로 전환
            return;
        }

        if (player.IsBlocking && player.Stats.CurrentStamina > 0)
        {
            stateMachine.ChangeState(player.DefenceState);
            return;
        }
        if (player.IsParryTriggered && player.Stats.CurrentStamina >= 30f)
        {
            stateMachine.ChangeState(player.ParryState);
            return;
        }
        // 스프린트 여부에 따라 현재 속도를 배로 적용
        bool canRun = player.IsRun && player.Stats.CurrentStamina > 0;
        float currentSpeed = canRun ? player.MoveSpeed * 1.5f : player.MoveSpeed;

        if (canRun)
        {
            player.Stats.SpendStamina(3 * Time.deltaTime);
        }

        // 주 로직 실행 (이동 방향만 계산해서 Controller에게 전달)
        Vector3 dir = player.GetCameraDirection(player.MoveInput);
        //Vector3 dir = new Vector3(player.SmoothedInput.x, 0f, player.SmoothedInput.y);
        //dir = player.transform.TransformDirection(dir.normalized);

        player.CurrentMovement = dir * currentSpeed;
    }

    public override void Exit()
    {
        // 걷기 상태 종료 시 특별히 트리거할 애니메이션이 있다면 호출
    }
}