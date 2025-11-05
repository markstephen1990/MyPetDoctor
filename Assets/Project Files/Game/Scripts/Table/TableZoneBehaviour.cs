using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class TableZoneBehaviour : MonoBehaviour, IPurchaseObject, ISceneSavingCallback
    {
        [UniqueID]
        [SerializeField] string id;
        public string ID => id;

        [SerializeField] CurrencyAmount price;
        public CurrencyAmount Price => price;

        [SerializeField] TableBehaviour[] tableBehaviours;

        [Space]
        [Group("Refs")]
        [SerializeField] GameObject walkableZone;
        [Group("Refs")]
        [SerializeField] Transform purchaseZoneTransform;

        [Space]
        [Group("Refs")]
        [SerializeField] GameObject[] environmentObjects;

        [Header("Lock")]
        [Group("Refs")]
        [SerializeField] GameObject lockObject;
        [Group("Refs")]
        [SerializeField] GameObject solidLockContainer;
        [Group("Refs")]
        [SerializeField] GameObject purchaseLockContainer;

        [Space]
        [Group("Refs")]
        [SerializeField] Transform purchaseLeftSideTransform;
        [Group("Refs")]
        [SerializeField] Transform purchaseRightSideTransform;

        [Space]
        [Group("Refs")]
        [SerializeField] float scaleZSize = 1.0f;

        [Group("Purchasing")]
        [SerializeField] bool isAllowedAdOpening = true;
        [Group("Purchasing")]
        [SerializeField] TableZoneBehaviour nextTableZoneBehaviour;

        private PurchaseAreaBehaviour purchaseAreaBehaviour;

        public bool IsAllowedAdOpening => isAllowedAdOpening;

        private Zone zone;
        public Zone Zone => zone;

        private bool isOpened;
        public bool IsOpened => isOpened || price.Amount == 0;

        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        private int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        public Transform Transform => transform;

        public TableBehaviour[] TableBehaviours => tableBehaviours;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            isUnlocked = true;

            if (IsOpened)
            {
                lockObject.SetActive(false);

                walkableZone.SetActive(true);

                for (int i = 0; i < tableBehaviours.Length; i++)
                {
                    tableBehaviours[i].Initialise(zone, this, false);
                }

                if (nextTableZoneBehaviour != null)
                {
                    nextTableZoneBehaviour.ActivatePurchase();
                }

                for (int i = 0; i < environmentObjects.Length; i++)
                {
                    environmentObjects[i].SetActive(true);
                }
            }
            else
            {
                walkableZone.SetActive(false);

                lockObject.SetActive(true);
            }
        }

        public void OnTableOpened(TableBehaviour tableBehaviour)
        {

        }

        public void Unlock()
        {
            isUnlocked = true;

            if (purchaseAreaBehaviour != null)
                purchaseAreaBehaviour.SetBlockState(false);
        }

        public void Lock()
        {
            isUnlocked = false;

            if (purchaseAreaBehaviour != null)
                purchaseAreaBehaviour.SetBlockState(true);
        }

        #region Purchase
        public void PlaceCurrency(int amount)
        {
            if (!IsOpened)
            {
                // Request game save
                SaveController.MarkAsSaveIsRequired();

                // Adjust place amount
                placedCurrencyAmount += amount;

                // Redraw purchase area amount
                purchaseAreaBehaviour.SetAmount(price.Amount - placedCurrencyAmount);
            }
        }

        public void OnPurchaseCompleted()
        {
            if (IsOpened)
                return;

            isOpened = true;

            if (isAllowedAdOpening)
            {
                UIController.GetPage<UIGame>().DisableZoneAdButton();
            }

            SaveController.MarkAsSaveIsRequired();

            if (nextTableZoneBehaviour != null)
            {
                nextTableZoneBehaviour.ActivatePurchaseWithAnimation();
            }

            // Unlock zone with animation
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, 0.7f);

            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);


            purchaseLeftSideTransform.DOScaleX(0, 0.14f).SetEasing(Ease.Type.CubicIn);
            purchaseRightSideTransform.DOScaleX(0, 0.14f).SetEasing(Ease.Type.CubicIn);

            purchaseLockContainer.transform.DOScaleZ(0, 0.2f).SetEasing(Ease.Type.QuadOut).OnComplete(delegate
            {
                lockObject.SetActive(false);

                walkableZone.SetActive(true);

                for (int i = 0; i < tableBehaviours.Length; i++)
                {
                    int index = i;

                    Tween.DelayedCall((index + 1) * 0.06f, delegate
                    {
                        tableBehaviours[index].Initialise(zone, this, true);
                    });
                }

                for (int i = 0; i < environmentObjects.Length; i++)
                {
                    Vector3 defaultScale = environmentObjects[i].transform.localScale;

                    environmentObjects[i].SetActive(true);
                    environmentObjects[i].transform.localScale = Vector3.zero;
                    environmentObjects[i].transform.DOScale(defaultScale, 0.4f).SetEasing(Ease.Type.BackOut);
                }

                NavMeshController.RecalculateNavMesh(delegate { });
            });

            purchaseAreaBehaviour.DisableWithAnimation();

            TutorialController.OnTableZoneUnlocked(this);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
        }

        public void ActivatePurchase()
        {
            if (IsOpened)
                return;

            // Create purchase zone
            purchaseAreaBehaviour = LevelController.CreateTableAreaPurchaseZone();
            purchaseAreaBehaviour.TransformInitialise(purchaseZoneTransform.position, Quaternion.identity, new Vector2(115, 90), 0.06f, 4.5f);
            purchaseAreaBehaviour.Initialise(this, false, isAllowedAdOpening);
            purchaseAreaBehaviour.Enable();

            // Enable lock object
            lockObject.SetActive(true);

            solidLockContainer.SetActive(false);
            purchaseLockContainer.SetActive(true);

            // Reset transforms
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, 0.7f);
            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);
        }

        public void ActivatePurchaseWithAnimation()
        {
            if (IsOpened)
                return;

            // Create purchase zone
            purchaseAreaBehaviour = LevelController.CreateTableAreaPurchaseZone();
            purchaseAreaBehaviour.TransformInitialise(purchaseZoneTransform.position, Quaternion.identity, new Vector2(115, 90), 0.06f, 4.5f);
            purchaseAreaBehaviour.Initialise(this, false, isAllowedAdOpening);
            purchaseAreaBehaviour.EnableWithAnimation();

            // Enable lock object
            lockObject.SetActive(true);

            solidLockContainer.SetActive(false);
            purchaseLockContainer.SetActive(true);

            // Reset transforms
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, scaleZSize);
            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);

            // Play container animation
            Tween.DelayedCall(0.2f, delegate
            {
                purchaseLockContainer.transform.DOScaleZ(0.7f, 0.1f);
            });
        }
        #endregion

        #region Load/Save
        public void Load(SaveData save)
        {
            isOpened = save.IsOpened;
            placedCurrencyAmount = save.PlacedCurrencyAmount;

            if (save.TablesSaveData != null)
            {
                for (int i = 0; i < save.TablesSaveData.Length; i++)
                {
                    if (tableBehaviours.IsInRange(i))
                    {
                        tableBehaviours[i].Load(save.TablesSaveData[i]);
                    }
                }
            }
        }

        public SaveData Save()
        {
            TableSaveData[] tablesSaveData = new TableSaveData[tableBehaviours.Length];
            for (int i = 0; i < tablesSaveData.Length; i++)
            {
                tablesSaveData[i] = tableBehaviours[i].Save();
            }

            return new SaveData(id, placedCurrencyAmount, isOpened, tablesSaveData);
        }
        #endregion

        public void OnPlayerEntered(PlayerBehavior playerBehavior)
        {

        }

        public void OnPlayerExited(PlayerBehavior playerBehavior)
        {

        }

        #region Editor
        public void OnSceneSaving()
        {
            tableBehaviours = transform.GetComponentsInChildren<TableBehaviour>(true);

            RuntimeEditorUtils.SetDirty(this);
        }

        public void SetUnlockedState()
        {
            if(!tableBehaviours.IsNullOrEmpty())
            {
                for(int i = 0; i < tableBehaviours.Length; i++)
                {
                    tableBehaviours[i].gameObject.SetActive(true);
                }
            }

            if (walkableZone != null)
                walkableZone.SetActive(true);

            if (lockObject != null)
                lockObject.SetActive(false);
        }

        public void SetLockedState()
        {
            if (!tableBehaviours.IsNullOrEmpty())
            {
                for (int i = 0; i < tableBehaviours.Length; i++)
                {
                    tableBehaviours[i].gameObject.SetActive(false);
                }
            }

            if (walkableZone != null)
                walkableZone.SetActive(false);

            if (lockObject != null)
                lockObject.SetActive(true);
        }
        #endregion

        [System.Serializable]
        public class SaveData
        {
            [SerializeField] string id;
            public string ID => id;

            [SerializeField] int placedCurrencyAmount;
            public int PlacedCurrencyAmount => placedCurrencyAmount;

            [SerializeField] bool isOpened;
            public bool IsOpened => isOpened;

            [SerializeField] TableSaveData[] tablesSaveData;
            public TableSaveData[] TablesSaveData => tablesSaveData;

            public SaveData(string id, int placedCurrencyAmount, bool isOpened, TableSaveData[] tablesSaveData)
            {
                this.id = id;
                this.placedCurrencyAmount = placedCurrencyAmount;
                this.isOpened = isOpened;
                this.tablesSaveData = tablesSaveData;
            }
        }
    }
}