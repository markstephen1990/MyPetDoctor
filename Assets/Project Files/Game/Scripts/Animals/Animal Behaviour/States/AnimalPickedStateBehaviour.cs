using UnityEngine;

namespace Watermelon
{
    public class AnimalPickedStateBehaviour : AnimalStateBehaviour
    {
        public AnimalPickedStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {

        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.StopMovement();
        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {

        }

        public override void OnTriggerEnter(Collider other)
        {

        }

        public override void OnTriggerExit(Collider other)
        {

        }
    }
}