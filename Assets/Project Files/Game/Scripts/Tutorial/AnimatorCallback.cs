using UnityEngine;
using UnityEngine.Events;

namespace FernGames
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