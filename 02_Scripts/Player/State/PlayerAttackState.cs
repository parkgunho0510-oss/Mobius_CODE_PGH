using UnityEngine;

public class PlayerAttackState : PlayerState
{
    //������
    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    private bool _canCombo;
    private bool _comboBuffered;
    private int _comboStep = 1;

    public override void Enter()
    {
        _canCombo = false;
        _comboBuffered = false;

        player.CanRotate = false; // ȸ�� ����

        SoundEvents.RaiseSFXPlayedEvent($"Swing_{_comboStep}");
        SoundEvents.RaiseSFXPlayedEvent($"SpritSound_{_comboStep}");

        // ���� ���� �� ��Ʈ ��� �ѱ� (�ִϸ��̼��� �̵��� �ֵ�)
        player.Animator.applyRootMotion = true;

        SoundEvents.RaiseSFXPlayedEvent($"Swing_{_comboStep}");
        SoundEvents.RaiseSFXPlayedEvent($"SpritSound_{_comboStep}");

        // �� Ÿ°���� ���ڸ� �Ѱ���
        player.Animator.SetInteger("ComboStep", _comboStep);

        // �ִϸ��̼� ����
        player.Animator.SetBool("IsAttacking", true);
        player.Stats.SpendStamina(15f);

    }

    public override void LogicUpdate()
    {
        if (_canCombo && player.IsAttackTriggered)
        {
            _comboBuffered = true;
        }
    }

    public void OpenCombo()
    {
        _canCombo = true;
    }

    public void CloseCombo()
    {
        if (_comboBuffered && player.Stats.CurrentStamina > 0)
        {
            _comboStep++;

            int maxCombo = 1;
            if (player.CurrentItems != null &&
                player.CurrentItems.CurrentWeapon != null &&
                player.CurrentItems.CurrentWeapon.AnimData != null)
            {
                maxCombo = player.CurrentItems.CurrentWeapon.AnimData.MaxComboCount;
            }
            if (_comboStep > maxCombo)
            {
                _comboStep = 1;
                stateMachine.ChangeState(player.IdleState);
                return; 
            }

            // 최대 콤보 이내라면 다음 타수로 넘어감
            stateMachine.ChangeState(player.AttackState);
        }
        else
        {
            _comboStep = 1;
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        player.Animator.SetBool("IsAttacking", false);

        player.Animator.applyRootMotion = false;

        if (player.TryGetComponent<Combat_Test2>(out var combat))
        {
            combat.StopCasting();
        }
        CombatEventBus.RaiseAttackEnd();
    }
}