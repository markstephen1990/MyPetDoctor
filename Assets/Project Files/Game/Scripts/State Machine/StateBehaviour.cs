namespace Watermelon
{
    public abstract class StateBehaviour<T> : BaseStateBehaviour
    {
        protected T stateMachineController;
        public T StateMachineController => stateMachineController;

        public StateBehaviour(T stateMachineController)
        {
            this.stateMachineController = stateMachineController;
        }
    }
}