using UnityEngine;

namespace Assets._02_Scripts.Player.State
{
    public class PlayerJumpState : PlayerState
    {
        private Vector3 _cachedMovement;
        public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            SoundEvents.RaiseSFXPlayedEvent("Jump");
            player.Animator.SetBool("IsJumping", true);

            player.VerticalVelocity = Mathf.Sqrt(player.JumpHeight * -2f * player.Gravity);
            //점프 단발

            // 스프린트 여부에 따라 현재 속도를 배로 적용
            float currentSpeed = player.IsRun ? player.MoveSpeed * 1.5f : player.MoveSpeed;

            // 점프 버튼을 누른 순간의 입력값을 가져와서 이동 벡터를 계산 저장
            Vector3 dir = player.GetCameraDirection(player.MoveInput);
            _cachedMovement = dir * currentSpeed;
        }

        public override void LogicUpdate()
        {
            player.CurrentMovement = _cachedMovement;

            if (player.VerticalVelocity < 0f)
            {
                // stateMachine.ChangeState(player.FallState); 
            }

            // 바닥에 닿으면 다시 Idle이나 Walk로 복귀
            if (player.CharacterController.isGrounded && player.VerticalVelocity <= 0f)
            {
                stateMachine.ChangeState(player.MoveInput.sqrMagnitude > 0.1f ? player.WalkState : player.IdleState);
            }
        }


        public override void Exit() {
            player.Animator.SetBool("IsJumping", false);
        }
    }
}
