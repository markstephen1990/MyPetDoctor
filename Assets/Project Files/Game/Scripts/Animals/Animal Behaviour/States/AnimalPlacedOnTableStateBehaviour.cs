using UnityEngine;

namespace Watermelon
{
    public class AnimalPlacedOnTableStateBehaviour : AnimalStateBehaviour
    {
        public AnimalPlacedOnTableStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
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
                IItemCarrying itemCarrying = other.GetComponent<IItemCarrying>();
                if (itemCarrying != null)
                {
                    if (stateMachineController.ParentBehaviour.CanBeCured(itemCarrying))
                    {
                        AudioController.PlaySound(AudioController.AudioClips.animalCureSound);

                        stateMachineController.ParentBehaviour.CureAnimal(itemCarrying);
                    }
                }
            }
        }

        public override void OnTriggerExit(Collider other)
        {

        }
    }
}