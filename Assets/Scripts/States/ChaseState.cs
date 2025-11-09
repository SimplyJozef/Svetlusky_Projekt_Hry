using UnityEngine;

namespace States
{
    public class ChaseState : IState
    {
        private float _distance;
        public void OnEnter(MonsterAIController controller)
        {
        }

        public void TickState(MonsterAIController controller)
        {
            if (!controller.bCanSeeTarget)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }
            // Check if is in attack distance, if yes stop (TODO add attack or something)
            _distance = Vector3.Distance(controller.Agent.transform.position, controller.Target.position);
            if (_distance < controller.AttackDistance)
            {
                controller.Agent.isStopped = true;
            }
            // Otherwise set players new position as destination
            else
            {
                controller.Agent.isStopped = false;
                controller.Agent.destination = controller.Target.position;
            }
        }

        public void OnExit(MonsterAIController controller)
        {
            controller.Agent.isStopped = false;
        }
    }
}
