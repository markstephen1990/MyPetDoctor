using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon
{
    public class UpgradeUIPanel : MonoBehaviour
    {
        private const string LEVEL = "LVL {0}";

        [SerializeField] Image iconImage;
        [SerializeField] Image buttonImage;
        [SerializeField] Sprite buttonActiveSprite;
        [SerializeField] Sprite buttonDisableSprite;
        [SerializeField] Image currencyImage;

        [Space]
        [SerializeField] GameObject adsButtonObject;

        [Space]
        [SerializeField] GameObject maxObject;

        [Space]
        [SerializeField] TextMeshProUGUI priceText;
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] TextMeshProUGUI nameText;

        private Currency currency;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        private CanvasGroup priceCanvasGroup;

        private BaseUpgrade upgrade;
        public BaseUpgrade Upgrade => upgrade;

        private UISecretaryWindow uiSecretaryWindow;

        public void Initialise(UISecretaryWindow uiSecretaryWindow, BaseUpgrade upgrade)
        {
            this.uiSecretaryWindow = uiSecretaryWindow;
            this.upgrade = upgrade;

            // Get component
            canvasGroup = GetComponent<CanvasGroup>();
            priceCanvasGroup = priceText.GetComponent<CanvasGroup>();

            // Redraw panel
            Redraw();

            // Set name
            nameText.text = upgrade.Title;
            iconImage.sprite = upgrade.CurrentStage.PreviewSprite;
        }

        public void Redraw()
        {
            // Doctor is opened
            if (upgrade.Upgrades.Length <= upgrade.UpgradeLevel + 1)
            {
                // Disable button
                buttonImage.gameObject.SetActive(false);
                adsButtonObject.gameObject.SetActive(false);

                // Enable hired panel
                maxObject.SetActive(true);

                // Reset panel transparent
                canvasGroup.alpha = 1.0f;
            }
            else
            {
                // Enable button
                buttonImage.gameObject.SetActive(true);
                adsButtonObject.gameObject.SetActive(true);

                // Enable hired panel
                maxObject.SetActive(false);

                Currency currency = CurrencyController.GetCurrency(upgrade.Upgrades[upgrade.UpgradeLevel + 1].CurrencyType);
                currencyImage.sprite = currency.Icon;

                int price = upgrade.Upgrades[upgrade.UpgradeLevel + 1].Price;
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

                // Set price
                priceText.text = CurrencyHelper.Format(price);
            }

            // Set level
            levelText.text = string.Format(LEVEL, upgrade.UpgradeLevel + 1);
        }

        public void PurchaseButton()
        {
            if (upgrade.Upgrades.Length >= upgrade.UpgradeLevel + 1)
            {
                Currency currency = CurrencyController.GetCurrency(upgrade.Upgrades[upgrade.UpgradeLevel + 1].CurrencyType);
                int price = upgrade.Upgrades[upgrade.UpgradeLevel + 1].Price;
                if (currency.Amount >= price)
                {
                    AudioController.PlaySound(AudioController.AudioClips.buttonSound);

                    CurrencyController.Substract(currency.CurrencyType, price);

                    upgrade.UpgradeStage();

                    uiSecretaryWindow.OnUpgraded();
                }
            }
        }

        public void PurchaseAdButton()
        {
            if (upgrade.Upgrades.Length >= upgrade.UpgradeLevel + 1)
            {
                AudioController.PlaySound(AudioController.AudioClips.buttonSound);

                AdsManager.ShowRewardBasedVideo((reward) =>
                {
                    if (reward)
                    {
                        upgrade.UpgradeStage();

                        uiSecretaryWindow.OnUpgraded();
                    }
                });
            }
        }
    }
}