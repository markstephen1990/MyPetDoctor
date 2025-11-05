using UnityEngine;

namespace Watermelon
{
    public class NurseStateMachineController : StateMachineController<NurseBehaviour, NurseStateMachineController.State>
    {
        protected override void RegisterStates()
        {
            // Register and initialise states
            RegisterState(new WaitingStateBehaviour(this), State.Waiting);
            RegisterState(new ReturnToWaitingStateBehaviour(this), State.ReturnToWaiting);
            RegisterState(new PickAnimalStateBehaviour(this), State.PickingAnimal);
            RegisterState(new DeliverAnimalStateBehaviour(this), State.DeliveringAnimal);
            RegisterState(new PickItemStateBehaviour(this), State.PickingItem);
            RegisterState(new DeliverItemStateBehaviour(this), State.DeliveringItem);
        }

        public enum State
        {
            Disabled = 0,
            Waiting = 1,
            ReturnToWaiting = 2,
            PickingAnimal = 3,
            DeliveringAnimal = 4,
            PickingItem = 5,
            DeliveringItem = 6,
        }
    }
}