using UnityEngine;

namespace Watermelon
{
    public class AnimalCarryingStateBehaviour : AnimalStateBehaviour
    {
        public AnimalCarryingStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }

        public override void OnStateRegistered()
        {

        }

        public override void OnStateActivated()
        {

        }

        public override void OnStateDisabled()
        {

        }

        public override void Update()
        {

        }

        public override void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
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
                // Destroy waiting indicator
                stateMachineController.ParentBehaviour.DestroyWaitingIndicator();
            }
        }
    }
}