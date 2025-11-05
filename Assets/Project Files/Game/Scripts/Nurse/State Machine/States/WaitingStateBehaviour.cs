using UnityEngine;

namespace Watermelon
{
    public class WaitingStateBehaviour : NurseStateBehaviour
    {
        private const float MIN_DELAY_BEFORE_ACTION = 2.0f;
        private const float MAX_DELAY_BEFORE_ACTION = 5.0f;

        private TweenCase rotationTweenCase;

        private float actionTime;

        public WaitingStateBehaviour(NurseStateMachineController nurseStateMachineController) : base(nurseStateMachineController)
        {

        }

        public override void OnStateRegistered()
        {

        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.VisitorAnimator.SetBool(NurseBehaviour.RUN_HASH, false);
            stateMachineController.ParentBehaviour.NavMeshAgent.enabled = false;

            actionTime = Time.time + Random.Range(MIN_DELAY_BEFORE_ACTION, MAX_DELAY_BEFORE_ACTION);
        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {
            if (Time.time > actionTime)
            {
                if (stateMachineController.ParentBehaviour.IsAnimalCarrying())
                {
                    // Find free table
                    if (stateMachineController.ParentBehaviour.Zone.HasFreeTable())
                    {
                        stateMachineController.SetState(NurseStateMachineController.State.DeliveringAnimal);
                    }
                }
                else
                {
                    if (stateMachineController.ParentBehaviour.Zone.HasRequiredItem())
                    {
                        stateMachineController.SetState(NurseStateMachineController.State.PickingItem);
                    }
                    // Find free animal
                    else if (stateMachineController.ParentBehaviour.Zone.GetFreeWaitingAnimal() != null)
                    {
                        stateMachineController.SetState(NurseStateMachineController.State.PickingAnimal);
                    }
                }

                actionTime = Random.Range(MIN_DELAY_BEFORE_ACTION, MAX_DELAY_BEFORE_ACTION);
            }
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                if (rotationTweenCase != null && !rotationTweenCase.IsCompleted)
                    rotationTweenCase.Kill();

                rotationTweenCase = stateMachineController.ParentBehaviour.transform.DOLookAt(other.transform.position, 0.15f);
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {

            }
        }
    }
}