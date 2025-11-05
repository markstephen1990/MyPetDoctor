using UnityEngine;
using UnityEngine.Events;

namespace Watermelon
{
    public class PlayerTrigger : MonoBehaviour
    {
        [SerializeField] UnityEvent onTriggerEnter;
        [SerializeField] UnityEvent onTriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();
                if (playerBehavior != null)
                {
                    onTriggerEnter?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();
                if (playerBehavior != null)
                {
                    onTriggerExit?.Invoke();
                }
            }
        }
    }
}