using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    public class AnimalWaitingStateBehaviour : AnimalStateBehaviour
    {
        private TweenCase rotationTweenCase;

        private NavMeshAgent navMeshAgent;
        private Transform transform;

        private float lastPathGeneratedTime = float.MinValue;

        public AnimalWaitingStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            navMeshAgent = stateMachineController.ParentBehaviour.NavMeshAgent;
            transform = stateMachineController.ParentBehaviour.transform;
        }

        public override void OnStateActivated()
        {
            StartMovementAndRecalculate();
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
                            StartMovementAndRecalculate();
                        }
                    }
                }

                if (Time.time > lastPathGeneratedTime)
                {
                    StartMovementAndRecalculate();
                }
            }
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
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

        public override void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                StartMovementAndRecalculate();

                // Destroy waiting indicator
                stateMachineController.ParentBehaviour.DestroyWaitingIndicator();
            }
        }

        private void StartMovementAndRecalculate()
        {
            stateMachineController.ParentBehaviour.StartMovement();
            stateMachineController.ParentBehaviour.SetMovementSpeed(AnimalBehaviour.MOVEMENT_SPEED_DEFAULT);

            if (stateMachineController.ParentBehaviour.IsMovementAllowed)
            {
                navMeshAgent.SetDestination(stateMachineController.ParentBehaviour.AnimalSpawner.GetRandomPositionWalkPosition());
            }

            lastPathGeneratedTime = Time.time + Random.Range(5.0f, 9.0f);
        }
    }
}