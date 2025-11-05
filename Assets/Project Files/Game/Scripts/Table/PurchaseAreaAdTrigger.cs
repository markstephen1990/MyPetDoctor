using UnityEngine;

namespace Watermelon
{
    public class PurchaseAreaAdTrigger : MonoBehaviour
    {
        private IPurchaseObject purchaseObject;

        private UIGame gameUI;

        private bool isActive;

        public void Initialise(IPurchaseObject purchaseObject)
        {
            this.purchaseObject = purchaseObject;

            // Get UI component
            gameUI = UIController.GetPage<UIGame>();

            isActive = true;
        }

        public void SetState(bool state)
        {
            isActive = state;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                gameUI.ActivateZoneAdButton(purchaseObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isActive)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                gameUI.DisableZoneAdButton();
            }
        }
    }
}