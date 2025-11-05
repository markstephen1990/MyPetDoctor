using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Watermelon
{
    public class DoctorUIPanel : MonoBehaviour
    {
        [SerializeField] Image buttonImage;
        [SerializeField] Sprite buttonActiveSprite;
        [SerializeField] Sprite buttonDisableSprite;
        [SerializeField] Image currencyImage;

        [Space]
        [SerializeField] GameObject adsButtonObject;

        [Space]
        [SerializeField] GameObject hiredObject;

        [Space]
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] Image iconImage;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        private CanvasGroup priceCanvasGroup;

        private int id;
        private int price;

        private NurseSettings nurseSettings;
        private Currency currency;

        private UISecretaryWindow uiSecretaryWindow;

        public void Initialise(UISecretaryWindow uiSecretaryWindow, int id, NurseSettings nurseSettings, int openedIndex)
        {
            this.uiSecretaryWindow = uiSecretaryWindow;
            this.id = id;
            this.nurseSettings = nurseSettings;

            currency = CurrencyController.GetCurrency(nurseSettings.CurrencyType);

            price = nurseSettings.Price;

            // Get component
            canvasGroup = GetComponent<CanvasGroup>();
            priceCanvasGroup = priceText.GetComponent<CanvasGroup>();

            // Redraw panel
            Redraw(openedIndex);

            // Set price
            priceText.text = CurrencyHelper.Format(price);

            // Set price currency icon
            currencyImage.sprite = currency.Icon;

            // Set name
            nameText.text = nurseSettings.Title;

            // Set icon
            iconImage.sprite = nurseSettings.Preview;
        }

        public void Redraw(int openedIndex)
        {
            // Doctor is opened
            if (openedIndex > id)
            {
                // Disable button
                buttonImage.gameObject.SetActive(false);
                adsButtonObject.gameObject.SetActive(false);

                // Enable hired panel
                hiredObject.SetActive(true);

                // Reset panel transparent
                canvasGroup.alpha = 1.0f;
            }
            // Doctor is locked
            else if (openedIndex < id)
            {
                // Disable button
                buttonImage.gameObject.SetActive(false);
                adsButtonObject.gameObject.SetActive(false);

                // Enable hired panel
                hiredObject.SetActive(false);

                // Make panel transparent a bit
                canvasGroup.alpha = 0.6f;
            }
            // Doctor is ready to buy
            else if (openedIndex == id)
            {
                // Enable button
                buttonImage.gameObject.SetActive(true);
                adsButtonObject.gameObject.SetActive(true);

                // Enable hired panel
                hiredObject.SetActive(false);

                if (currency.Amount >= price)
                {
                    buttonImage.sprite = buttonActiveSprite;
                    priceCanvasGroup.alpha = 1.0f;
                }
                else
                {
                    buttonImage.sprite = buttonDisableSprite;
                    priceCanvasGroup.alpha = 0.6f;
                }

                // Reset panel transparent
                canvasGroup.alpha = 1.0f;
            }
        }

        public float GetPanelAlpha(int openedIndex)
        {
            if (openedIndex < id)
                return 0.6f;

            return 1.0f;
        }

        public void PurchaseButton()
        {
            if (currency.Amount >= price)
            {
                uiSecretaryWindow.OnNursePurchased();

                CurrencyController.Substract(currency.CurrencyType, price);

                AudioController.PlaySound(AudioController.AudioClips.buttonSound);

                uiSecretaryWindow.OnUpgraded();
            }
        }

        public void PurchaseAdButton()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            AdsManager.ShowRewardBasedVideo((reward) =>
            {
                if (reward)
                {
                    uiSecretaryWindow.OnNursePurchased();
                }
            });
        }
    }
}