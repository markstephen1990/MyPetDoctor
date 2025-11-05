using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public class AnimatorCallback : MonoBehaviour
    {
        [SerializeField] UnityEvent onCallback;

        public void OnCallback()
        {
            onCallback?.Invoke();
        }
    }
}