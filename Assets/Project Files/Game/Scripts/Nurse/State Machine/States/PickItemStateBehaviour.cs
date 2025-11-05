using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class PickItemStateBehaviour : NurseStateBehaviour
    {
        private DispenserBuilding dispenserBuilding;
        private AnimalBehaviour targetAnimalBehaviour;

        private NavMeshAgent navMeshAgent;
        private Animator animator;

        public PickItemStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            // Get references
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
            animator = stateMachineController.ParentBehaviour.VisitorAnimator;
        }

        public override void OnStateActivated()
        {
            dispenserBuilding = null;

            InitialiseTargetItem();
        }

        public override void OnStateDisabled()
        {
            animator.SetBool(NurseBehaviour.RUN_HASH, false);
        }

        public override void Update()
        {
            if (targetAnimalBehaviour.AnimalStateMachineController.CurrentState != AnimalStateMachineController.State.PlacedOnTable)
            {
                stateMachineController.ParentBehaviour.RemoveItems(true);
                stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
            }

            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= 5.0f)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude <= 2.0f)
                        {
                            // Pick animal
                            dispenserBuilding.PickItem(stateMachineController.ParentBehaviour);

                            // Prepare deliver state
                            DeliverItemStateBehaviour deliverItemState = (DeliverItemStateBehaviour)stateMachineController.GetState(NurseStateMachineController.State.DeliveringItem);
                            deliverItemState.SetTargetAnimalBehaviour(targetAnimalBehaviour);

                            // Change state to delivering animal
                            stateMachineController.SetState(NurseStateMachineController.State.DeliveringItem);
                        }
                    }
                }
            }
        }

        private void InitialiseTargetItem()
        {
            targetAnimalBehaviour = stateMachineController.ParentBehaviour.Zone.GetSickAnimal();
            if (targetAnimalBehaviour != null)
            {
                if (targetAnimalBehaviour.ActiveSickness != null)
                {
                    dispenserBuilding = stateMachineController.ParentBehaviour.Zone.GetDispenser(targetAnimalBehaviour.ActiveSickness.RequiredItem);
                    if (dispenserBuilding != null)
                    {
                        targetAnimalBehaviour.MarkAsBusy();

                        animator.SetBool(NurseBehaviour.RUN_HASH, true);

                        navMeshAgent.enabled = true;
                        navMeshAgent.isStopped = false;

                        navMeshAgent.SetDestination(dispenserBuilding.transform.position);
                    }
                    else
                    {
                        // Free animal is missing
                        // Change state to waiting
                        stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
                    }
                }
                else
                {
                    // Free animal is missing
                    // Change state to waiting
                    stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
                }
            }
            else
            {
                // Required item is missing
                // Change state to waiting
                stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
            }
        }
    }
}