using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class ReturnToWaitingStateBehaviour : NurseStateBehaviour
    {
        private float actionTime;

        private NavMeshAgent navMeshAgent;

        public ReturnToWaitingStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            // Get references
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.VisitorAnimator.SetBool(NurseBehaviour.RUN_HASH, true);
            stateMachineController.ParentBehaviour.NavMeshAgent.enabled = true;
            stateMachineController.ParentBehaviour.NavMeshAgent.isStopped = false;
            stateMachineController.ParentBehaviour.NavMeshAgent.SetDestination(stateMachineController.ParentBehaviour.Zone.GetNurseSpawnPosition());
        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {
            if (Time.time > actionTime)
            {

            }

            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude <= 2.0f)
                        {
                            // Change state to delivering animal
                            stateMachineController.SetState(NurseStateMachineController.State.Waiting);
                        }
                    }
                }
            }
        }
    }
}