namespace Watermelon
{
    public class AnimalStateMachineController : StateMachineController<AnimalBehaviour, AnimalStateMachineController.State>
    {
        protected override void RegisterStates()
        {
            // Register and initialise states
            RegisterState(new AnimalCarryingStateBehaviour(this), State.Carrying);
            RegisterState(new AnimalWaitingStateBehaviour(this), State.Waiting);
            RegisterState(new AnimalReturnToWaitingStateBehaviour(this), State.ReturnToWaiting);
            RegisterState(new AnimalPickedStateBehaviour(this), State.Picked);
            RegisterState(new AnimalPlacedOnTableStateBehaviour(this), State.PlacedOnTable);
            RegisterState(new AnimalLeavingStateBehaviour(this), State.Leaving);
        }

        public enum State
        {
            Disabled = 0,
            Carrying = 1,
            Waiting = 2,
            ReturnToWaiting = 3,
            Picked = 4,
            PlacedOnTable = 5,
            Leaving = 6
        }
    }
}