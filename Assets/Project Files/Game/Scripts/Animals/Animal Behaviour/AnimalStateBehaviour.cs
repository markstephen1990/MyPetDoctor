namespace Watermelon
{
    public abstract class AnimalStateBehaviour : StateBehaviour<AnimalStateMachineController>
    {
        public AnimalStateBehaviour(AnimalStateMachineController stateMachineController) : base(stateMachineController)
        {

        }
    }
}