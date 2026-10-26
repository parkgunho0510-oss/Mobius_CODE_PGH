using System.Collections.Generic;

public class MonsterCombatTree
{
    private BTNode _root;

    public MonsterCombatTree(MonsterBase monster)
    {
        _root = new Selector(new List<BTNode>
        {
            // 몬스터가 직접 던져주는 고유 공격 노드를 1순위로 배치
            monster.GetAttackNode(), 
            
            // 공격이 안 되면 추적
            new MonsterChaseNode(monster)
        });
    }

    public void Abort()
    {
        _root.Abort();
    }

    public NodeState Evaluate()
    {
        return _root.Evaluate();
    }
}