using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class AnimalReturnToWaitingStateBehaviour : AnimalStateBehaviour
    {
        private const float PICK_UP_DELAY_AFTER_DROP = 2.0f;

        private float startReturnTime;

        private TweenCase rotationTweenCase;

        private NavMeshAgent navMeshAgent;
        private Transform transform;

        public AnimalReturnToWaitingStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
            transform = stateMachineController.ParentBehaviour.transform;
        }

        public override void OnStateActivated()
        {
            startReturnTime = Time.time + PICK_UP_DELAY_AFTER_DROP;

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
                            stateMachineController.SetState(AnimalStateMachineController.State.Waiting);
                        }
                    }
                }
            }
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                if (Time.time > startReturnTime)
                {
                    if (stateMachineController.ParentBehaviour.IsMovementAllowed)
                    {
                        // Kill rotation tween case
                        if (rotationTweenCase != null && !rotationTweenCase.IsCompleted)
                            rotationTweenCase.Kill();

                        // Stop movement
                        stateMachineController.ParentBehaviour.StopMovement();

                        // Rotate to player
                        rotationTweenCase = transform.DOLookAt(other.transform.position, 0.15f);
                    }

                    // Get carrying component
                    IAnimalCarrying carrying = other.GetComponent<IAnimalCarrying>();
                    if (carrying != null)
                    {
                        if (carrying.IsAnimalPickupAllowed())
                        {
                            // Create waiting indicator
                            stateMachineController.ParentBehaviour.CreateWaitingIndicator(carrying);
                        }
                        else
                        {
                            PlayerBehavior.SpawnMaxText();
                        }
                    }
                }
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                // Destroy waiting indicator
                stateMachineController.ParentBehaviour.DestroyWaitingIndicator();
            }
        }
    }
}