using UnityEngine;

namespace Watermelon
{
    public class VisitorLeavingStateBehaviour : VisitorStateBehaviour
    {
        public VisitorLeavingStateBehaviour(VisitorStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {

        }

        public override void OnStateActivated()
        {
            stateMachineController.ParentBehaviour.SetTargetPosition(stateMachineController.ParentBehaviour.SpawnPosition, delegate
            {
                stateMachineController.ParentBehaviour.DisableVisitor();
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

        }

        public override void OnTriggerExit(Collider other)
        {

        }
    }
}