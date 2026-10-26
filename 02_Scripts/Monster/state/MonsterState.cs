using UnityEngine;


public abstract class MonsterState
{
    protected MonsterBase _monster;
    protected MonsterStateMachine _stateMachine;

    public MonsterState(MonsterBase monster, MonsterStateMachine stateMachine)
    {
        _monster = monster;
        _stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void Exit() { }
}