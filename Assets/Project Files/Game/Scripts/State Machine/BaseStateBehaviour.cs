using UnityEngine;

namespace Watermelon
{
    public abstract class BaseStateBehaviour
    {
        public abstract void OnStateRegistered();
        public abstract void OnStateActivated();
        public abstract void OnStateDisabled();

        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }

        public abstract void Update();
    }
}