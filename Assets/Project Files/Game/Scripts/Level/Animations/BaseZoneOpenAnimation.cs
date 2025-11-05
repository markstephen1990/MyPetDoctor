using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Zone))]
    public abstract class BaseZoneOpenAnimation : MonoBehaviour, ISceneSavingCallback
    {
        protected Zone zone;

        public abstract void OnZoneInitialised(Zone zone);
        public abstract void OnZoneOpened();

        public abstract void OnSceneSaving();

        [Button("Relink Components")]
        public void RelinkComponents()
        {
            OnSceneSaving();
        }
    }
}