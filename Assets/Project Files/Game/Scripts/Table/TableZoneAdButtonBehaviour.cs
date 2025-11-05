using UnityEngine;

namespace Watermelon
{
    public class TableZoneAdButtonBehaviour : MonoBehaviour
    {
        private static readonly Vector3 OFFSET_POSITION = new Vector3(0, 9, 3);

        [SerializeField] CanvasGroup canvasGroup;

        private bool isActive;
        public bool IsActive => isActive;

        private TweenCase fadeTweenCase;

        private IPurchaseObject purchaseObject;

        public void Initialise(IPurchaseObject purchaseObject)
        {
            this.purchaseObject = purchaseObject;

            // Enable object
            gameObject.SetActive(true);

            // Reset variables
            if (fadeTweenCase != null && fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            isActive = false;
            canvasGroup.alpha = 0.0f;

            // Place on the right position
            transform.position = UIController.FixUIElementToWorld(purchaseObject.Transform, OFFSET_POSITION);

            // Show button
            Show();
        }

        private void LateUpdate()
        {
            transform.position = UIController.FixUIElementToWorld(purchaseObject.Transform, OFFSET_POSITION);
        }

        public void Show()
        {
            if (isActive)
                return;

            isActive = true;

            if (fadeTweenCase != null && fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            fadeTweenCase = canvasGroup.DOFade(1.0f, 0.3f).SetEasing(Ease.Type.QuintOut);
        }

        public void Hide()
        {
            if (!isActive)
                return;

            isActive = false;

            if (fadeTweenCase != null && fadeTweenCase.IsCompleted)
                fadeTweenCase.Kill();

            fadeTweenCase = canvasGroup.DOFade(0.0f, 0.2f).SetEasing(Ease.Type.QuintIn).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }

        public void OnButtonClick()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                {
                    Tween.NextFrame(delegate
                    {
                        // Place required currency amount
                        purchaseObject.PlaceCurrency(purchaseObject.Price.Amount - purchaseObject.PlacedCurrencyAmount);
                        purchaseObject.OnPurchaseCompleted();
                    });

                    // Disable panel
                    Hide();
                }
            });
        }
    }
}