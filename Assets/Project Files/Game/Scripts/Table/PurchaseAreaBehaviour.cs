using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class PurchaseAreaBehaviour : MonoBehaviour
    {
        [SerializeField] RectTransform canvasRectTransform;

        [Space]
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] Image currencyImage;
        [SerializeField] Image blockImage;
        [SerializeField] Image borderImage;

        // Currency
        private CurrencyType requiredCurrencyType;
        private Currency currency;

        // Purchase object
        private IPurchaseObject targetPurchaseObject;

        // State
        private bool isBlocked;
        private bool isPurchasingEnabled;
        private bool isAdAllowed;

        private PurchaseAreaAdTrigger purchaseAreaAdTrigger;

        public void Initialise(IPurchaseObject targetPurchaseObject, bool blockState, bool isAdAllowed = false)
        {
            this.targetPurchaseObject = targetPurchaseObject;
            this.isAdAllowed = isAdAllowed;

            if (isAdAllowed)
            {
                purchaseAreaAdTrigger = LevelController.CreatePurchaseAreaAdTrigger();
                purchaseAreaAdTrigger.transform.position = transform.position;
                purchaseAreaAdTrigger.gameObject.SetActive(true);

                purchaseAreaAdTrigger.Initialise(targetPurchaseObject);
            }

            requiredCurrencyType = targetPurchaseObject.Price.CurrencyType;

            currency = CurrencyController.GetCurrency(requiredCurrencyType);

            // Inititalise graphics
            currencyImage.sprite = currency.Icon;

            // Set amount
            SetAmount(targetPurchaseObject.Price.Amount - targetPurchaseObject.PlacedCurrencyAmount);

            // Set block state
            SetBlockState(blockState);
        }

        public void TransformInitialise(Vector3 position, Quaternion rotation, Vector2 size, float canvasSize, float borderMultiplier)
        {
            transform.position = position;
            transform.rotation = rotation;

            canvasRectTransform.sizeDelta = size;
            canvasRectTransform.localScale = canvasSize.ToVector3();

            borderImage.pixelsPerUnitMultiplier = borderMultiplier;
        }

        public void SetBlockState(bool blockState)
        {
            isBlocked = blockState;

            if (isBlocked)
            {
                // If zone is locked
                amountText.gameObject.SetActive(false);
                currencyImage.gameObject.SetActive(false);

                blockImage.gameObject.SetActive(true);
            }
            else
            {
                // If zone is unlocked
                blockImage.gameObject.SetActive(false);

                amountText.gameObject.SetActive(true);
                currencyImage.gameObject.SetActive(true);
            }

            if (purchaseAreaAdTrigger != null)
                purchaseAreaAdTrigger.SetState(!blockState);
        }

        public void SetAmount(int amount)
        {
            amountText.text = CurrencyHelper.Format(amount);
        }

        public void Enable()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }

        public void EnableWithAnimation()
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;

            transform.DOScale(1.0f, 0.38f).SetEasing(Ease.Type.SineOut);

            borderImage.type = Image.Type.Filled;
            borderImage.fillAmount = 0;
            borderImage.DOFillAmount(1.0f, 0.5f).OnComplete(delegate
            {

            });
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void DisableWithAnimation()
        {
            transform.DOScale(0.0f, 0.5f).SetEasing(Ease.Type.CircOut).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isBlocked)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();
                if (playerBehavior != null)
                {
                    playerBehavior.SetPurchaseObject(targetPurchaseObject);

                    targetPurchaseObject.OnPlayerEntered(playerBehavior);

                    isPurchasingEnabled = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isBlocked)
                return;

            if (isPurchasingEnabled)
            {
                if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
                {
                    PlayerBehavior playerBehavior = other.GetComponent<PlayerBehavior>();
                    if (playerBehavior != null)
                    {
                        playerBehavior.ResetPurchaseObject(targetPurchaseObject);

                        targetPurchaseObject.OnPlayerExited(playerBehavior);

                        isPurchasingEnabled = false;
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (purchaseAreaAdTrigger != null)
            {
                purchaseAreaAdTrigger.gameObject.SetActive(false);
            }
        }
    }
}