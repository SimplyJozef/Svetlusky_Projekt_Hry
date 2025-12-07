using System.Collections;
using UnityEngine;

namespace States
{
    public class IdleState : IState
    {
        private static readonly int _bIsWalking = Animator.StringToHash("bIsWalking");
        private float _idleTime = 0;
        
        public void OnEnter(MonsterAIController controller)
        {
            _idleTime = controller.IdleWaitTime;
            controller.Animator.SetBool(_bIsWalking, false);
        }

        public void TickState(MonsterAIController controller)
        {
            if (controller.bCanSeeTarget)
            {
                controller.ChangeState(controller.ChaseState);
            }
            if (_idleTime < 0)
            {
                controller.ChangeState(controller.PatrolState);
            }
            _idleTime -= Time.deltaTime;
        }

        public void OnExit(MonsterAIController controller)
        {
        }
    }
}
