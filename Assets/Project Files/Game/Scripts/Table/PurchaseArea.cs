using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [System.Serializable]
    public class PurchaseArea
    {
        [SerializeField] Transform areaContainer;

        [Space]
        [SerializeField] Text amountText;
        [SerializeField] Image currencyImage;
        [SerializeField] Image blockImage;

        // Currency
        private CurrencyType requiredCurrencyType;
        private Currency currency;

        private bool isBlocked;

        public void Initialise(CurrencyType currencyType, int amount, bool blockState)
        {
            requiredCurrencyType = currencyType;

            currency = CurrencyController.GetCurrency(currencyType);

            // Inititalise graphics
            currencyImage.sprite = currency.Icon;

            // Set amount
            SetAmount(amount);

            // Set block state
            SetBlockState(blockState);
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
        }

        public void SetAmount(int amount)
        {
            amountText.text = CurrencyHelper.Format(amount);
        }

        public void Enable()
        {
            areaContainer.gameObject.SetActive(true);
            areaContainer.localScale = Vector3.one;
        }

        public void EnableWithAnimation()
        {
            areaContainer.gameObject.SetActive(true);
            areaContainer.localScale = Vector3.zero;

            areaContainer.DOScale(1.0f, 0.5f).SetEasing(Ease.Type.CircIn);
        }

        public void Disable()
        {
            areaContainer.gameObject.SetActive(false);
        }

        public void DisableWithAnimation()
        {
            areaContainer.DOScale(0.0f, 0.5f).SetEasing(Ease.Type.CircOut).OnComplete(delegate
            {
                areaContainer.gameObject.SetActive(false);
            });
        }
    }
}