using UnityEngine;

public class PlayerRollState : PlayerState
{
    private Vector3 _rollDirection;
    private float _rollTimer;
    private float _rollDuration = 0.5f; // ������ ���� �ð�
    private float _rollSpeedMultiplier = 2.5f; // �ӵ� ���

    public PlayerRollState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {

        player.CanRotate = false; // ������ �߿��� ���� ��ȯ ����
        _rollTimer = 0f;

        SoundEvents.RaiseSFXPlayedEvent("Roll");

        // ������ �ִϸ��̼� ����
        player.Animator.SetBool("Roll", true);

        // �Է°��� �ִٸ� �� ��������, ���ٸ� ���� ĳ���Ͱ� �ٶ󺸴� �������� ���� ����
        if (player.MoveInput.sqrMagnitude > 0.1f)
        {
            _rollDirection = player.GetCameraDirection(player.MoveInput);
        }
        else
        {
            _rollDirection = player.transform.forward;
        }
        player.Stats.SpendStamina(30f);

    }

    public override void LogicUpdate()
    {
        _rollTimer += Time.deltaTime;

        // ������ ���� �ӵ��� �о���
        player.CurrentMovement = _rollDirection * (player.MoveSpeed * _rollSpeedMultiplier);

        // ������ ������ �ð��� ������ ���� ����
        if (_rollTimer >= _rollDuration)
        {
            // �����Ⱑ ���� ������ �Է°��� ���� �ȱ� �Ǵ� ��� ���·� ��ȯ
            if (player.MoveInput.sqrMagnitude > 0.1f)
            {
                stateMachine.ChangeState(player.WalkState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        player.Animator.SetBool("Roll", false);

        // ���߿� ������ �����Ҷ� ���
    }
}