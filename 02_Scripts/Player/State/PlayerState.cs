using UnityEngine;

// 모든 상태의 뼈대
public abstract class PlayerState
//FSM을 인터페이스가 아닌 클래스를 쓴 이유는 공통된 변수 선언과 할당 로직 코드 중복 제거
//추상인 이유 : 상속을 위한 뼈대이기에 new선언 되면 안 됨
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController player; // 이동 로직, 컴포넌트를 가진 메인 스크립트

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
    {
        //생성자
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }    // 상태 진입 시 (애니메이션 재생 등)
    public virtual void LogicUpdate() { } // Update 로직
    public virtual void Exit() { }     // 상태 탈출 시 (변수 초기화 등)
}