namespace Watermelon
{
    public class VisitorStateMachineController : StateMachineController<VisitorBehaviour, VisitorStateMachineController.State>
    {
        protected override void RegisterStates()
        {
            // Register and initialise states
            RegisterState(new VisitorWaitingStateBehaviour(this), State.Waiting);
            RegisterState(new VisitorDeliveringStateBehaviour(this), State.Delivering);
            RegisterState(new VisitorPickingAnimalStateBehaviour(this), State.PickingAnimal);
            RegisterState(new VisitorLeavingStateBehaviour(this), State.Leaving);
        }

        public enum State
        {
            Disabled = 0,
            Waiting = 1,
            Delivering = 2,
            PickingAnimal = 3,
            Leaving = 4
        }
    }
}