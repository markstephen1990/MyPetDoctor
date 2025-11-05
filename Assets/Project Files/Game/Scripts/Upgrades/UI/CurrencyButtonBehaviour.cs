using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [System.Serializable]
    public class CurrencyButtonBehaviour
    {
        [SerializeField] Button buttonBehaviour;
        [SerializeField] Image buttonImage;

        [Space]
        [SerializeField] Image currencyImage;
        [SerializeField] Text currencyAmountText;

        [Space]
        [SerializeField] Sprite activeBackgroundSprite;
        [SerializeField] Sprite unactiveBackgroundSprite;
        [SerializeField] Color activeTextColor = Color.white;
        [SerializeField] Color unactiveTextColor = Color.white;

        private Currency currency;
        private int requiredAmount;

        public void Initialise(CurrencyType currencyType, int requiredAmount)
        {
            this.requiredAmount = requiredAmount;

            currency = CurrencyController.GetCurrency(currencyType);

            currencyImage.sprite = currency.Icon;
            currencyAmountText.text = CurrencyHelper.Format(requiredAmount);

            Redraw();
        }

        public void SetText(string text)
        {
            currencyAmountText.text = text;
        }

        public void Disable()
        {
            buttonImage.sprite = unactiveBackgroundSprite;
            currencyAmountText.color = unactiveTextColor;
        }

        public void Enable()
        {
            buttonImage.sprite = activeBackgroundSprite;
            currencyAmountText.color = activeTextColor;
        }

        public void Redraw()
        {
            if (currency.Amount >= requiredAmount)
            {
                buttonImage.sprite = activeBackgroundSprite;
                currencyAmountText.color = activeTextColor;
            }
            else
            {
                buttonImage.sprite = unactiveBackgroundSprite;
                currencyAmountText.color = unactiveTextColor;
            }
        }
    }
}