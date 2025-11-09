namespace States
{
    public class PatrolState : IState
    {
        private int _currentPatrolPoint = 0;
        private bool _reversePatrolMoveFwd = true;
        public void OnEnter(MonsterAIController controller)
        {
            controller.Agent.destination = controller.PatrolPoints[_currentPatrolPoint].position;
        }

        public void TickState(MonsterAIController controller)
        {
            if (controller.bCanSeeTarget)
            {
                controller.ChangeState(controller.ChaseState);
                return;
            }
            // Agent has path computed
            if (!controller.Agent.pathPending)
            {
                // Is agent close enough to the target
                if (controller.Agent.remainingDistance <= controller.AcceptanceRadius)
                {
                    // Agent doesn't have a path or has stopped
                    if (!controller.Agent.hasPath || controller.Agent.velocity.sqrMagnitude <= 0.1f)
                    {
                        switch (controller.PatrolStyle)
                        {
                            case EPatrolStyle.PingPong:
                                if (_currentPatrolPoint == 0)
                                {
                                    _reversePatrolMoveFwd = true;
                                }
                                else if (_currentPatrolPoint == controller.PatrolPoints.Count - 1)
                                {
                                    _reversePatrolMoveFwd = false;
                                }

                                _currentPatrolPoint += _reversePatrolMoveFwd ? 1 : -1;
                                break;
                            case EPatrolStyle.FromStart:
                                _currentPatrolPoint = (_currentPatrolPoint + 1) % controller.PatrolPoints.Count;
                                break;
                        }
                        controller.ChangeState(controller.IdleState);
                    }
                }
            }
        }

        public void OnExit(MonsterAIController controller)
        {
        }
    }
}
