using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;

namespace Watermelon
{
    public class UISecretaryWindow : UIPage
    {
        private const int TAB_HIRE = 0;
        private const int TAB_UPGRADES = 1;

        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 95);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);

        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] Button closeButton;

        [Header("Tabs")]
        [SerializeField] Sprite tabSelectedSprite;
        [SerializeField] Sprite tabDisabledSprite;

        [SerializeField] Color tabSelectedColor = Color.white;
        [SerializeField] Color tabDisabledColor = Color.white;

        [Space]
        [SerializeField] Tab[] tabs;

        [Header("Hire")]
        [SerializeField] GameObject doctorUIPrefab;

        [Header("Upgrade")]
        [SerializeField] GameObject upgradeUIPrefab;

        private UIGame mainPage;
        private Zone zone;
        private Currency mainCurrency;

        // Hire
        private NurseSpawner nurseSpawner;
        private DoctorUIPanel[] doctorUIPanels;
        private Pool doctorUIPool;

        // Upgrades
        private UpgradeUIPanel[] upgradeUIPanels;
        private Pool upgradesUIPool;
        private BaseUpgrade[] upgrades;

        // Pages
        private int currentTab = -1;

        public override void Init()
        {
            doctorUIPool = new Pool(doctorUIPrefab, doctorUIPrefab.name, tabs[TAB_HIRE].TabContainerObject.transform);
            upgradesUIPool = new Pool(upgradeUIPrefab, upgradeUIPrefab.name, tabs[TAB_UPGRADES].TabContainerObject.transform);

            // DO INIT
            mainPage = UIController.GetPage<UIGame>();
            mainCurrency = CurrencyController.GetCurrency(CurrencyType.Coins);

            upgrades = UpgradesController.ActiveUpgrades;

            // Create UI panels
            upgradeUIPanels = new UpgradeUIPanel[upgrades.Length];
            for (int i = 0; i < upgradeUIPanels.Length; i++)
            {
                GameObject upgradeUIObject = upgradesUIPool.GetPooledObject();
                upgradeUIObject.transform.ResetLocal();
                upgradeUIObject.SetActive(true);

                UpgradeUIPanel upgradeUIPanel = upgradeUIObject.GetComponent<UpgradeUIPanel>();
                upgradeUIPanel.Initialise(this, upgrades[i]);

                upgradeUIPanels[i] = upgradeUIPanel;
            }

            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            doctorUIPool?.Destroy();
            upgradesUIPool?.Destroy();
        }

        #region Tabs
        public void ActivateTab(int tabIndex, bool animation = false)
        {
            if (currentTab != tabIndex)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    tabs[i].TabContainerObject.SetActive(false);
                    tabs[i].TabImage.sprite = tabDisabledSprite;
                    tabs[i].TabText.color = tabDisabledColor;
                }

                tabs[tabIndex].TabContainerObject.SetActive(true);
                tabs[tabIndex].TabImage.sprite = tabSelectedSprite;
                tabs[tabIndex].TabText.color = tabSelectedColor;
            }

            currentTab = tabIndex;

            Redraw(animation);
        }

        public void Redraw(bool animation)
        {
            if (currentTab == TAB_HIRE)
            {
                for (int i = 0; i < doctorUIPanels.Length; i++)
                {
                    doctorUIPanels[i].Redraw(nurseSpawner.OpenedNurses);

                    if (animation)
                    {
                        int index = i;

                        doctorUIPanels[i].CanvasGroup.alpha = 0.0f;

                        Tween.DelayedCall(0.2f + i * 0.09f, delegate
                        {
                            doctorUIPanels[index].CanvasGroup.DOFade(doctorUIPanels[index].GetPanelAlpha(nurseSpawner.OpenedNurses), 0.6f).SetEasing(Ease.Type.SineOut);
                        });
                    }
                    else
                    {
                        doctorUIPanels[i].CanvasGroup.alpha = 1.0f;
                    }
                }
            }
            else
            {
                for (int i = 0; i < upgradeUIPanels.Length; i++)
                {
                    upgradeUIPanels[i].Redraw();

                    if (animation)
                    {
                        int index = i;

                        upgradeUIPanels[i].CanvasGroup.alpha = 0.0f;

                        Tween.DelayedCall(0.2f + i * 0.09f, delegate
                        {
                            upgradeUIPanels[index].CanvasGroup.DOFade(1.0f, 0.6f).SetEasing(Ease.Type.SineOut);
                        });
                    }
                    else
                    {
                        upgradeUIPanels[i].CanvasGroup.alpha = 1.0f;
                    }
                }
            }
        }
        #endregion

        public void ApplyNurseSettings(Zone zone)
        {
            this.zone = zone;

            nurseSpawner = zone.NurseSpawner;

            // Return everything to pool
            doctorUIPool.ReturnToPoolEverything();

            // Create UI panels
            doctorUIPanels = new DoctorUIPanel[nurseSpawner.NurseSettings.Length];
            for (int i = 0; i < doctorUIPanels.Length; i++)
            {
                GameObject doctorUIObject = doctorUIPool.GetPooledObject();
                doctorUIObject.transform.ResetLocal();
                doctorUIObject.SetActive(true);

                DoctorUIPanel doctorUIPanel = doctorUIObject.GetComponent<DoctorUIPanel>();
                doctorUIPanel.Initialise(this, i, nurseSpawner.NurseSettings[i], nurseSpawner.OpenedNurses);

                doctorUIPanels[i] = doctorUIPanel;
            }
        }

        public override void PlayHideAnimation()
        {
            Control.CurrentControl.OnMovementInputActivated -= OnJoystickTouched;

            mainCurrency.OnCurrencyChanged -= OnCurrencyAmountChanged;

            fadeImage.DOFade(0, 0.5f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.5f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                currentTab = -1;

                UIController.OnPageClosed(this);
            });
        }

        public override void PlayShowAnimation()
        {
            Control.CurrentControl.OnMovementInputActivated += OnJoystickTouched;

            mainCurrency.OnCurrencyChanged += OnCurrencyAmountChanged;

            Control.CurrentControl.ResetControl();

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(0.25f, 0.5f);

            // Reset panel position
            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.5f).SetEasing(Ease.Type.CircOut);

            ActivateTab(TAB_HIRE, true);

            UIController.OnPageOpened(this);
        }

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            for (int i = 0; i < doctorUIPanels.Length; i++)
            {
                doctorUIPanels[i].Redraw(nurseSpawner.OpenedNurses);
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                upgradeUIPanels[i].Redraw();
            }
        }

        public void OnUpgraded()
        {
            for (int i = 0; i < doctorUIPanels.Length; i++)
            {
                doctorUIPanels[i].Redraw(nurseSpawner.OpenedNurses);
            }

            for (int i = 0; i < upgrades.Length; i++)
            {
                upgradeUIPanels[i].Redraw();
            }

            SaveController.MarkAsSaveIsRequired();
        }

        private void OnJoystickTouched()
        {
            UIController.HidePage<UISecretaryWindow>();
        }

        public void OnNursePurchased()
        {
            zone.UnlockNurse();

            for (int i = 0; i < upgrades.Length; i++)
            {
                upgradeUIPanels[i].Redraw();
            }

            for (int i = 0; i < doctorUIPanels.Length; i++)
            {
                doctorUIPanels[i].Redraw(nurseSpawner.OpenedNurses);
            }

            SaveController.MarkAsSaveIsRequired();
        }

        #region Buttons
        public void TabHireButton()
        {
            ActivateTab(TAB_HIRE, true);

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void TabUpgradesButton()
        {
            ActivateTab(TAB_UPGRADES, true);

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        public void OnCloseButtonClicked()
        {
            UIController.HidePage<UISecretaryWindow>();
        }
        #endregion

        [Serializable]
        private class Tab
        {
            [SerializeField] Image tabImage;
            public Image TabImage => tabImage;

            [SerializeField] TextMeshProUGUI tabText;
            public TextMeshProUGUI TabText => tabText;

            [SerializeField] GameObject tabContainerObject;
            public GameObject TabContainerObject => tabContainerObject;
        }
    }
}