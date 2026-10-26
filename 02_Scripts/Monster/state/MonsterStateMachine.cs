using UnityEngine;

public class MonsterStateMachine
{
    public MonsterState CurrentState { get; private set; }

    public void Initialize(MonsterState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(MonsterState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
