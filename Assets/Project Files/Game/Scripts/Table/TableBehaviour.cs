using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class TableBehaviour : MonoBehaviour, IAnimalHolder, IPurchaseObject
    {
        private static readonly int PARTICLE_CONFETTI_HASH = "Table Confetti".GetHashCode();

        [SerializeField] CurrencyAmount price;
        public CurrencyAmount Price => price;

        [Group("Refs")]
        [SerializeField] GameObject graphicsObject;
        [Group("Refs")]
        [SerializeField] Transform storageContainer;

        [Header("Purchase Zone")]
        [Group("Refs")]
        [SerializeField] Vector3 purchaseZonePosition;
        [Group("Refs")]
        [SerializeField] Vector2 purchaseZoneSize;

        [Header("Refferences")]
        [Group("Refs")]
        [SerializeField] Animal.Type[] uniqueAnimalTypes;

        private AnimalBehaviour animalBehaviour;
        public AnimalBehaviour AnimalBehaviour => animalBehaviour;

        private Zone zone;
        public Zone Zone => zone;

        private bool isAnimalPlaced;
        public bool IsAnimalPlaced => isAnimalPlaced;

        private bool isOpened;
        public bool IsOpened => isOpened || price.Amount == 0;

        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        private bool isBusy;
        public bool IsBusy => isBusy;

        private int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        public Transform Transform => transform;

        private PurchaseAreaBehaviour purchaseAreaBehaviour;
        private TableZoneBehaviour tableZoneBehaviour;

        private TableCallbackReciever callbackReciever;

        public void Initialise(Zone zone, TableZoneBehaviour tableZoneBehaviour, bool openingAnimation)
        {
            this.zone = zone;
            this.tableZoneBehaviour = tableZoneBehaviour;

            isUnlocked = true;
            isBusy = false;
            isAnimalPlaced = false;

            animalBehaviour = null;

            if (IsOpened)
            {
                // Enable table object
                gameObject.SetActive(true);

                // Activate graphics object
                graphicsObject.SetActive(true);
                graphicsObject.transform.localScale = Vector3.one;

                isUnlocked = true;
                isOpened = true;
            }
            else
            {
                // Disable table object
                gameObject.SetActive(false);

                // Create table purchase zone
                CreatePurchaseZone(openingAnimation);
            }

            callbackReciever = GetComponent<TableCallbackReciever>();
            if (callbackReciever != null)
                callbackReciever.Initialise(this);
        }

        public void ActivateWithAnimation()
        {
            if (IsOpened)
                return;

            isOpened = true;

            // Invoke OnTableOpened method in table zone script
            tableZoneBehaviour.OnTableOpened(this);

            // Enable table object
            gameObject.SetActive(true);

            // Enable graphcs object
            graphicsObject.SetActive(true);

            // Reset transform values
            graphicsObject.transform.localScale = Vector3.zero;

            // Play particle effect
            ParticlesController.PlayParticle(PARTICLE_CONFETTI_HASH).SetPosition(transform.position);

            // Play open animation
            graphicsObject.transform.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut).OnComplete(delegate
            {
                // Add table to NavMesh
                NavMeshController.RecalculateNavMesh(delegate { });
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isUnlocked)
            {
                if (IsOpened)
                {
                    if (!isAnimalPlaced)
                    {
                        if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
                        {
                            IAnimalCarrying carrying = other.GetComponent<IAnimalCarrying>();
                            if (carrying != null)
                            {
                                PlaceAnimal(carrying, true);
                            }
                        }
                    }
                }
            }
        }

        public void OnPlayerEntered(PlayerBehavior playerBehavior)
        {

        }

        public void OnPlayerExited(PlayerBehavior playerBehavior)
        {

        }

        #region Purchase Zone
        private void CreatePurchaseZone(bool animation)
        {
            // Disable already existing zone
            if (purchaseAreaBehaviour != null)
            {
                // Disable purchase area
                purchaseAreaBehaviour.Disable();
                purchaseAreaBehaviour = null;
            }

            // Create purchase zone
            purchaseAreaBehaviour = LevelController.CreateTablePurchaseZone();
            purchaseAreaBehaviour.TransformInitialise(transform.position + purchaseZonePosition, Quaternion.identity, purchaseZoneSize, 0.06f, 4.5f);
            purchaseAreaBehaviour.Initialise(this, !isUnlocked);

            if (animation)
            {
                // Enable with animation
                purchaseAreaBehaviour.EnableWithAnimation();
            }
            else
            {
                // Enable immediatly
                purchaseAreaBehaviour.Enable();
            }
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
        #endregion

        #region AI
        public void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (callbackReciever != null)
                callbackReciever.OnAnimalPicked(animalBehaviour);
        }

        public void PlaceAnimal(IAnimalCarrying carrying, bool playSound = false)
        {
            if (isAnimalPlaced)
                return;

            AnimalBehaviour animalBehaviour = null;
            if (!uniqueAnimalTypes.IsNullOrEmpty())
            {
                animalBehaviour = carrying.GetAnimal(uniqueAnimalTypes);
            }
            else
            {
                animalBehaviour = carrying.GetAnimal();
            }

            if (animalBehaviour != null)
            {
                carrying.RemoveAnimal(animalBehaviour);

                isAnimalPlaced = true;

                // Add animal to table list
                zone.AddTableAnimal(animalBehaviour);

                // Play animation
                animalBehaviour.transform.SetParent(null);
                animalBehaviour.transform.DORotate(storageContainer.rotation, 0.5f);
                animalBehaviour.transform.DOBezierFollow(storageContainer, Random.Range(3, 5), 0, 0, 0.5f).SetEasing(Ease.Type.SineIn).OnComplete(delegate
                {
                    animalBehaviour.transform.SetParent(storageContainer);
                    animalBehaviour.transform.localPosition = Vector3.zero;
                    animalBehaviour.transform.localRotation = Quaternion.identity;
                });

                animalBehaviour.SetAnimalHolder(this);
                animalBehaviour.OnAnimalPlacedOnTable(this);

                if (playSound)
                    AudioController.PlaySound(AudioController.AudioClips.animalPlaceSound);

                this.animalBehaviour = animalBehaviour;

                if (callbackReciever != null)
                    callbackReciever.OnAnimalPlaced(animalBehaviour);
            }
        }

        public virtual void OnAnimalCured(Transform character)
        {
            isBusy = false;

            if(callbackReciever != null)
            {
                callbackReciever.OnAnimalCured(() =>
                {
                    // Remove animal from table list
                    zone.RemoveTableAnimal(animalBehaviour);

                    ResetTable();
                });
            }
            else
            {
                // Remove animal from table list
                zone.RemoveTableAnimal(animalBehaviour);

                ResetTable();
            }
        }

        public void ResetTable()
        {
            isAnimalPlaced = false;
            isBusy = false;

            animalBehaviour = null;
        }

        public void MarkAsBusy()
        {
            isBusy = true;
        }
        #endregion

        #region Purchase
        public void PlaceCurrency(int amount)
        {
            if (!IsOpened)
            {
                SaveController.MarkAsSaveIsRequired();

                placedCurrencyAmount += amount;

                purchaseAreaBehaviour.SetAmount(price.Amount - placedCurrencyAmount);
            }
        }

        public void OnPurchaseCompleted()
        {
            ActivateWithAnimation();

            purchaseAreaBehaviour.DisableWithAnimation();

            AudioController.PlaySound(AudioController.AudioClips.animalCureSound);
            AudioController.PlaySound(AudioController.AudioClips.tableOpenSound);

            TutorialController.OnTableUnlocked(this);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
        }
        #endregion

        #region Load/Save
        public void Load(TableSaveData save)
        {
            isOpened = save.IsOpened;
            placedCurrencyAmount = save.PlacedCurrencyAmount;
            isUnlocked = save.IsUnlocked;
        }

        public TableSaveData Save()
        {
            return new TableSaveData(placedCurrencyAmount, isOpened, isUnlocked);
        }
        #endregion


    }
}