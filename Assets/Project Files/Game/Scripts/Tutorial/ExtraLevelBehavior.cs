using UnityEngine;

namespace Watermelon
{
    public abstract class ExtraLevelBehavior : MonoBehaviour
    {
        public abstract void Initialise(Level level);
        public abstract void OnGameLoaded();
    }
}