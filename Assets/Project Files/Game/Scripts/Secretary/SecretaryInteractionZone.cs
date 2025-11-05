using UnityEngine;

namespace Watermelon
{
    public class SecretaryInteractionZone : MonoBehaviour
    {
        private const float ACTIVATE_DURATION = 0.8f;

        [SerializeField] GameObject lockObject;
        [SerializeField] Transform fillTransform;

        private bool isBlocked;

        private SecretaryBehaviour secretaryBehaviour;

        private TweenCase activateTweenCase;

        public void Initialise(SecretaryBehaviour secretaryBehaviour)
        {
            this.secretaryBehaviour = secretaryBehaviour;

            // Reset fill zone scale
            fillTransform.localScale = Vector3.zero;

            // DEV Unlock Zone
            SetBlockState(false);
        }

        public void SetBlockState(bool blockState)
        {
            isBlocked = blockState;

            if (isBlocked)
            {
                // If zone is locked
                lockObject.SetActive(true);
            }
            else
            {
                // If zone is unlocked
                lockObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isBlocked)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                if (activateTweenCase != null && !activateTweenCase.IsCompleted)
                    activateTweenCase.Kill();

                fillTransform.localScale = Vector3.zero;

                activateTweenCase = fillTransform.DOScale(Vector3.one, ACTIVATE_DURATION).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    secretaryBehaviour.OnSecretaryZoneActivated();

                    fillTransform.localScale = Vector3.zero;
                });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isBlocked)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                if (activateTweenCase != null && !activateTweenCase.IsCompleted)
                    activateTweenCase.Kill();

                fillTransform.localScale = Vector3.zero;
            }
        }
    }
}