using System;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;
using Watermelon.SkinStore;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] Joystick joystick;

        [Space]
        [SerializeField] CurrencyUIController currenciesUIController;

        [Space]
        [SerializeField] Button dropButton;
        [SerializeField] Button storeButton;
        [SerializeField] Button iapStoreButton;
        [SerializeField] Transform settingsButtonTransform;

        [Space]
        [SerializeField] RectTransform safeAreaRectTransform;

        [Header("World Ad Button")]
        [SerializeField] TableZoneAdButtonBehaviour tableZoneAdButtonBehaviour;

        public Joystick Joystick => joystick;

        public CurrencyUIController CurrenciesUIController => currenciesUIController;

        public override void Init()
        {
            joystick.Init(canvas);

            currenciesUIController.Init(CurrencyController.Currencies);

            dropButton.onClick.AddListener(() => DropButton());
            storeButton.onClick.AddListener(() => OnStoreButtonClicked());
            iapStoreButton.onClick.AddListener(() => OnIAPStoreButtonClicked());

            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            settingsButtonTransform.localScale = Vector3.zero;
            settingsButtonTransform.DOScale(Vector3.one, 0.3f).SetCustomEasing(Ease.GetCustomEasingFunction("Light Bounce"));

            storeButton.transform.localScale = Vector3.zero;
            storeButton.transform.DOScale(Vector3.one, 0.3f, 0.08f).SetCustomEasing(Ease.GetCustomEasingFunction("Light Bounce"));

            iapStoreButton.transform.localScale = Vector3.zero;
            iapStoreButton.transform.DOScale(Vector3.one, 0.3f, 0.16f).SetCustomEasing(Ease.GetCustomEasingFunction("Light Bounce"));

            UIController.OnPageOpened(this);
        }

        #region Drop Button
        public void DropButton()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            PlayerBehavior.DropItemsAndAnimals();

            SetDropButtonState(false);
        }

        public void SetDropButtonState(bool state)
        {
            dropButton.gameObject.SetActive(state);
        }
#endregion

        public void OnStoreButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.ShowPage<UISkinStore>();
        }

        private void OnIAPStoreButtonClicked()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.ShowPage<UIStore>();
        }

        public void ActivateZoneAdButton(IPurchaseObject purchaseObject)
        {
            tableZoneAdButtonBehaviour.Initialise(purchaseObject);
        }

        public void DisableZoneAdButton()
        {
            tableZoneAdButtonBehaviour.Hide();
        }
    }
}