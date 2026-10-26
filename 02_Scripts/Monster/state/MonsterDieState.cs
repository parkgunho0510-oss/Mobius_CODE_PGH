using System.Collections;
using System.Threading;

public class MonsterDieState : MonsterState
{
    public MonsterDieState(MonsterBase monster, MonsterStateMachine stateMachine)
        : base(monster, stateMachine) { }

    public override void Enter()
    {
        _monster.SetDead(true);
        _monster.Agent.isStopped = true;
        _monster.Agent.velocity = UnityEngine.Vector3.zero;
        _monster.StartCoroutine(Co_Die());
    }

    private IEnumerator Co_Die()
    {
        _monster.Animator.SetBool("IsMove", false);
        _monster.Animator.SetBool("IsDead", true);
        SoundEvents.RaiseSFXPlayedEvent("MonsterDie");
        _monster.GetComponent<UnityEngine.Collider>().enabled = false;

        yield return new UnityEngine.WaitForSeconds(2f);
        _monster.MonsterDrop.Drop();
        _monster.Player.GetComponent<PlayerStatus>().AddExp(_monster.Data.DropExp);
        if (_monster.PoolKey != null)
            MonsterEvents.RaiseMonsterReturnedEvent(_monster.PoolKey, _monster.gameObject);
        else
            _monster.gameObject.SetActive(false);
    }
}