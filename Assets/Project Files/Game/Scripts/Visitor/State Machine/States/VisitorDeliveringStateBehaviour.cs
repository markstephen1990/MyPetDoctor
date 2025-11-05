using UnityEngine;

namespace Watermelon
{
    public class VisitorDeliveringStateBehaviour : VisitorStateBehaviour
    {
        private Transform transform;

        private TweenCase rotationTweenCase;

        public VisitorDeliveringStateBehaviour(VisitorStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {
            transform = stateMachineController.ParentBehaviour.transform;
        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.SetTargetPosition(stateMachineController.ParentBehaviour.PlacePosition, delegate
            {
                stateMachineController.ParentBehaviour.StopMovement();

                stateMachineController.ParentBehaviour.PlaceAnimalOnGround();

                Tween.NextFrame(delegate
                {
                    stateMachineController.SetState(VisitorStateMachineController.State.Leaving);
                });
            });
        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {

        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER) || other.CompareTag(PhysicsHelper.TAG_NURSE))
            {
                stateMachineController.ParentBehaviour.StopMovement();

                if (rotationTweenCase != null && !rotationTweenCase.IsCompleted)
                    rotationTweenCase.Kill();

                rotationTweenCase = transform.DOLookAt(other.transform.position, 0.15f);
            }
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER) || other.CompareTag(PhysicsHelper.TAG_NURSE))
            {
                stateMachineController.ParentBehaviour.SetTargetPosition(stateMachineController.ParentBehaviour.PlacePosition, delegate
                {
                    stateMachineController.ParentBehaviour.PlaceAnimalOnGround();
                });
            }
        }
    }
}