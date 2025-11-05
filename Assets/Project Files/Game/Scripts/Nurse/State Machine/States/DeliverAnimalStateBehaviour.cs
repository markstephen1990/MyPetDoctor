using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class DeliverAnimalStateBehaviour : NurseStateBehaviour
    {
        private const float PATH_CALCULATION_COOLDOWN = 0.5f;

        private bool isTableFound;
        private TableBehaviour targetTableBehaviour;

        private NavMeshAgent navMeshAgent;
        private Transform nurseTransform;
        private Animator animator;

        private float lastCalculationTime;

        public DeliverAnimalStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
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
            if (stateMachineController.ParentBehaviour.IsAnimalCarrying())
            {
                isTableFound = false;
                targetTableBehaviour = null;

                InitialiseTargetTable();
            }
            else
            {
                // Change state to delivering animal
                stateMachineController.SetState(NurseStateMachineController.State.Waiting);
            }
        }

        public override void OnStateDisabled()
        {
            animator.SetBool(NurseBehaviour.RUN_HASH, false);
        }

        public override void Update()
        {
            if (isTableFound)
            {
                if (targetTableBehaviour.IsAnimalPlaced)
                    InitialiseTargetTable();

                if (Time.time > lastCalculationTime)
                    CalculatePathToTable();
            }

            if (navMeshAgent.isActiveAndEnabled)
            {
                if (!navMeshAgent.pathPending)
                {
                    if (navMeshAgent.remainingDistance <= 8.0f)
                    {
                        if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude <= 2.0f)
                        {
                            if (isTableFound)
                            {
                                // Place animal
                                targetTableBehaviour.PlaceAnimal(stateMachineController.ParentBehaviour);

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
        }

        private void CalculatePathToTable()
        {
            lastCalculationTime = Time.time + PATH_CALCULATION_COOLDOWN;

            if (targetTableBehaviour != null)
                navMeshAgent.SetDestination(targetTableBehaviour.transform.position);
        }

        private void InitialiseTargetTable()
        {
            targetTableBehaviour = stateMachineController.ParentBehaviour.Zone.GetRandomFreeTable();
            if (targetTableBehaviour != null)
            {
                isTableFound = true;

                targetTableBehaviour.MarkAsBusy();

                animator.SetBool(NurseBehaviour.RUN_HASH, true);

                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;

                CalculatePathToTable();
            }
            else
            {
                // Free animal is missing
                // Change state to waiting
                stateMachineController.SetState(NurseStateMachineController.State.Waiting);
            }
        }
    }
}