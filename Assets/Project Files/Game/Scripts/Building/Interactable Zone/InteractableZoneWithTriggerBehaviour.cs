using UnityEngine;

namespace Watermelon
{
    public class InteractableZoneWithTriggerBehaviour : MonoBehaviour, IInteractableZoneBehaviour
    {
        [SerializeField] float triggerDelay;

        [Space]
        [SerializeField] Transform graphicsTransform;
        [SerializeField] float graphicsActiveSize;
        [SerializeField] Ease.Type scaleEasing;

        private IInteractableZoneWithTrigger interactableZone;

        private TweenCase triggerTweenCase;

        public void Initialise(IInteractableZone interactableZone)
        {
            this.interactableZone = (IInteractableZoneWithTrigger)interactableZone;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();

                interactableZone.OnZoneEnter(playerBehavior);

                if (triggerTweenCase != null && !triggerTweenCase.IsCompleted)
                    triggerTweenCase.Kill();

                triggerTweenCase = graphicsTransform.DOScale(graphicsActiveSize, triggerDelay).SetEasing(scaleEasing).OnComplete(delegate
                {
                    interactableZone.OnZoneTriggerActivated(playerBehavior);

                    graphicsTransform.localScale = Vector3.zero;
                });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                interactableZone.OnZoneExit(other.GetComponent<PlayerBehavior>());

                ResetTrigger();
            }
        }

        private void ResetTrigger()
        {
            if (triggerTweenCase != null && !triggerTweenCase.IsCompleted)
                triggerTweenCase.Kill();

            graphicsTransform.localScale = Vector3.zero;
        }
    }
}