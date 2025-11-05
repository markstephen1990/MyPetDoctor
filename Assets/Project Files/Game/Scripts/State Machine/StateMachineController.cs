using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class StateMachineController<K, T> where K : MonoBehaviour
    {
        private T currentState;
        public T CurrentState => currentState;

        private List<BaseStateBehaviour> stateBehaviours;
        private Dictionary<T, int> stateBehavioursLink;
        private int statesCount;

        private BaseStateBehaviour activeStateBehaviour;
        public BaseStateBehaviour ActiveStateBehaviour => activeStateBehaviour;

        private K parentBehaviour;
        public K ParentBehaviour => parentBehaviour;

        public void Initialise(K parentBehaviour, T defaultState)
        {
            this.parentBehaviour = parentBehaviour;

            // Reset variables
            stateBehaviours = new List<BaseStateBehaviour>();
            stateBehavioursLink = new Dictionary<T, int>();
            statesCount = 0;

            // Register and initialise states
            RegisterStates();

            // Enable default state
            SetState(defaultState);
        }

        protected void RegisterState(BaseStateBehaviour stateBehaviour, T state)
        {
            stateBehaviours.Add(stateBehaviour);
            stateBehavioursLink.Add(state, statesCount);

            statesCount++;

            stateBehaviour.OnStateRegistered();
        }

        public void SetState(T state)
        {
            if (!currentState.Equals(state))
            {
                // Disable current state
                if (activeStateBehaviour != null)
                    activeStateBehaviour.OnStateDisabled();

                // Rewrite current state
                currentState = state;
                activeStateBehaviour = stateBehaviours[stateBehavioursLink[currentState]];

                // Activate new state
                activeStateBehaviour.OnStateActivated();
            }
        }

        public BaseStateBehaviour GetState(T state)
        {
            return stateBehaviours[stateBehavioursLink[state]];
        }

        protected abstract void RegisterStates();
    }
}