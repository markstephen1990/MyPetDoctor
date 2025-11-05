using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class PickAnimalStateBehaviour : NurseStateBehaviour
    {
        private const float PATH_CALCULATION_COOLDOWN = 0.5f;

        private bool isAnimalFound;
        private AnimalBehaviour targetAnimalBehaviour;

        private NavMeshAgent navMeshAgent;
        private Transform nurseTransform;
        private Animator animator;

        private float lastCalculationTime;

        public PickAnimalStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            // Get references
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
            animator = stateMachineController.ParentBehaviour.VisitorAnimator;
            nurseTransform = stateMachineController.ParentBehaviour.transform;
        }

        public override void OnStateActivated()
        {
            isAnimalFound = false;
            targetAnimalBehaviour = null;

            InitialiseTargetAnimal();
        }

        public override void OnStateDisabled()
        {
            animator.SetBool(NurseBehaviour.RUN_HASH, false);
        }

        public override void Update()
        {
            if (isAnimalFound)
            {
                if (targetAnimalBehaviour.IsPicked)
                    InitialiseTargetAnimal();

                if (Time.time > lastCalculationTime)
                    CalculatePathToAnimal();
            }

            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= 4.5f)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude <= 2.0f)
                        {
                            if (isAnimalFound)
                            {
                                // Pick animal
                                targetAnimalBehaviour.PickAnimal(stateMachineController.ParentBehaviour);

                                // Change state to delivering animal
                                stateMachineController.SetState(NurseStateMachineController.State.DeliveringAnimal);
                            }
                        }
                    }
                }
            }
        }

        private void CalculatePathToAnimal()
        {
            lastCalculationTime = Time.time + PATH_CALCULATION_COOLDOWN;

            if (targetAnimalBehaviour != null && !targetAnimalBehaviour.IsPicked)
                navMeshAgent.SetDestination(targetAnimalBehaviour.transform.position);
        }

        private void InitialiseTargetAnimal()
        {
            targetAnimalBehaviour = stateMachineController.ParentBehaviour.Zone.GetFreeWaitingAnimal();
            if (targetAnimalBehaviour != null)
            {
                isAnimalFound = true;

                targetAnimalBehaviour.MarkAsBusy();

                animator.SetBool(NurseBehaviour.RUN_HASH, true);

                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;

                CalculatePathToAnimal();
            }
            else
            {
                // Free animal is missing
                // Change state to waiting
                stateMachineController.SetState(NurseStateMachineController.State.ReturnToWaiting);
            }
        }
    }
}