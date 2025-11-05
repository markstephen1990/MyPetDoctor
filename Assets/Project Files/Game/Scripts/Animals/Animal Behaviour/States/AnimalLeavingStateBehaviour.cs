using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class AnimalLeavingStateBehaviour : AnimalStateBehaviour
    {
        private NavMeshAgent navMeshAgent;

        public AnimalLeavingStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.StartMovement();
            stateMachineController.ParentBehaviour.SetMovementSpeed(AnimalBehaviour.MOVEMENT_SEEED_RUN);

            if (stateMachineController.ParentBehaviour.IsMovementAllowed)
            {
                navMeshAgent.SetDestination(stateMachineController.ParentBehaviour.AnimalSpawner.GetRandomPositionWalkPosition());
            }
        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {
            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude <= 2.0f)
                        {
                            stateMachineController.ParentBehaviour.StopMovement();
                        }
                    }
                }
            }
        }

        public override void OnTriggerEnter(Collider other)
        {

        }

        public override void OnTriggerExit(Collider other)
        {

        }
    }
}