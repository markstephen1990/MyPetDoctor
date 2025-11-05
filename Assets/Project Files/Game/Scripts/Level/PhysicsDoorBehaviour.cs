using UnityEngine;

namespace Watermelon
{
    public class PhysicsDoorBehaviour : MonoBehaviour
    {
        [SerializeField] BoxCollider leftDoorCollider;
        [SerializeField] Rigidbody leftDoorRigidbody;

        [SerializeField] BoxCollider rightDoorCollider;
        [SerializeField] Rigidbody rightDoorRigidbody;

        private bool isActive;
        public bool IsActive => isActive;

        private void Awake()
        {
            SetState(false);
        }

        public void SetState(bool state)
        {
            isActive = state;

            leftDoorCollider.enabled = state;
            leftDoorRigidbody.isKinematic = !state;

            rightDoorCollider.enabled = state;
            rightDoorRigidbody.isKinematic = !state;
        }

        private void OnTriggerEnter(Collider other)
        {

        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                // Player enter room
            }
        }
    }
}