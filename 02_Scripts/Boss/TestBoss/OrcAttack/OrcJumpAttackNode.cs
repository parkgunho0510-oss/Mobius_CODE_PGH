using UnityEngine;

public class OrcJumpAttackNode : BossAttackNode
{
    private OrcBoss _orcBoss;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _jumpProgress;

    public OrcJumpAttackNode(BossBase boss) : base(boss, "IsJump", "BodyBox")
    {
        _orcBoss = boss as OrcBoss;
    }

    protected override void OnEnter()
    {
        _startPos = _boss.transform.position;
        _targetPos = _boss.Player.transform.position;
        _jumpProgress = 0f;
        _boss.IsIgnoringGravity = true;
    }

    protected override void UpdateAction()
    {
        if (_orcBoss.IsJumping)
        {
            _jumpProgress += Time.deltaTime * _orcBoss.JumpSpeed;
            float clampedProgress = Mathf.Clamp01(_jumpProgress);

            Vector3 nextPos = Vector3.Lerp(_startPos, _targetPos, clampedProgress);
            nextPos.y += Mathf.Sin(clampedProgress * Mathf.PI) * _orcBoss.JumpHeight;

            Vector3 moveDelta = nextPos - _boss.transform.position;
            _boss.Controller.Move(moveDelta);
            _boss.Agent.nextPosition = _boss.transform.position;
            
            if (clampedProgress >= 1f)
            {
                _orcBoss.Anim_JumpEnd();
            }
        }
    }

    protected override void OnExit()
    {
        _boss.IsIgnoringGravity = false;
    }
}