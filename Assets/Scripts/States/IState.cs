namespace States
{
    public interface IState
    {
        public void OnEnter(MonsterAIController controller);
        public void TickState(MonsterAIController controller);
        public void OnExit(MonsterAIController controller);
    }
}
