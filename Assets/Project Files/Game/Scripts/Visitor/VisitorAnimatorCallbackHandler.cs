using UnityEngine;

namespace Watermelon
{
    public class VisitorAnimatorCallbackHandler : MonoBehaviour
    {
        private VisitorBehaviour visitorBehaviour;

        public void Init(VisitorBehaviour visitorBehaviour)
        {
            this.visitorBehaviour = visitorBehaviour;
        }

        public void OnAnimalPlaced()
        {
            visitorBehaviour.OnPlaceAnimation();
        }

        public void LeftStepCallback()
        {
            // Do nothing
        }

        public void RightStepCallback()
        {
            // Do nothing
        }
    }
}