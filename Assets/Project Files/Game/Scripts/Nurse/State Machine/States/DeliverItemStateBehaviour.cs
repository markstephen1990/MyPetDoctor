using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class DeliverItemStateBehaviour : NurseStateBehaviour
    {
        private AnimalBehaviour animalBehaviour;
        private bool isAnimalLinked;

        private NavMeshAgent navMeshAgent;
        private Animator animator;

        public DeliverItemStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
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
            if (isAnimalLinked)
            {
                animator.SetBool(NurseBehaviour.RUN_HASH, true);

                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;

                navMeshAgent.SetDestination(animalBehaviour.transform.position);
            }
            else
            {
                // Change state to waiting
                stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
            }
        }

        public override void OnStateDisabled()
        {
            animator.SetBool(NurseBehaviour.RUN_HASH, false);

            isAnimalLinked = false;
            animalBehaviour = null;
        }

        public override void Update()
        {
            if (!isAnimalLinked)
                return;

            if (animalBehaviour.AnimalStateMachineController.CurrentState != AnimalStateMachineController.State.PlacedOnTable)
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
                            animalBehaviour.CureAnimal(stateMachineController.ParentBehaviour);

                            bool nextStateFound = false;

                            if (Random.value >= 0.5f)
                            {
                                if (stateMachineController.ParentBehaviour.Zone.HasRequiredItem())
                                {
                                    stateMachineController.SetState(NurseStateMachineController.State.PickingItem);

                                    nextStateFound = true;
                                }
                                else if (stateMachineController.ParentBehaviour.Zone.GetFreeWaitingAnimal() != null)
                                {
                                    stateMachineController.SetState(NurseStateMachineController.State.PickingAnimal);

                                    nextStateFound = true;
                                }
                            }
                            else
                            {
                                if (stateMachineController.ParentBehaviour.Zone.GetFreeWaitingAnimal() != null)
                                {
                                    stateMachineController.SetState(NurseStateMachineController.State.PickingAnimal);

                                    nextStateFound = true;
                                }
                                else if (stateMachineController.ParentBehaviour.Zone.HasRequiredItem())
                                {
                                    stateMachineController.SetState(NurseStateMachineController.State.PickingItem);

                                    nextStateFound = true;
                                }
                            }

                            if (!nextStateFound)
                            {
                                // Change state to waiting
                                stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
                            }
                        }
                    }
                }
            }
        }

        public void SetTargetAnimalBehaviour(AnimalBehaviour animalBehaviour)
        {
            this.animalBehaviour = animalBehaviour;

            isAnimalLinked = true;
        }
    }
}