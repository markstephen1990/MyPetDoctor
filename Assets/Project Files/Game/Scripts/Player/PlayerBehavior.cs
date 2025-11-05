using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class PlayerBehavior : MonoBehaviour, IAnimalCarrying, IItemCarrying, IDoorOpener
    {
        public static readonly Vector3 STORAGE_OFFSET = new Vector3(0.0289999992f, 1.09800005f, -0.230000004f);

        public static readonly int RUN_HASH = Animator.StringToHash("Run");
        public static readonly int MOVEMENT_MULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");

        private static PlayerBehavior playerBehavior;

        public static Vector3 Position { get => playerBehavior.transform.position; set => playerBehavior.transform.position = value; }
        public Transform Transform { get => playerBehavior.transform; }

        [SerializeField] NavMeshAgent agent;

        [Header("Camera")]
        [SerializeField] float cameraOffset;
        [SerializeField] float cameraOffsetSpeed;
        [SerializeField] float cameraOffsetResetSpeed;
        [SerializeField] float cameraOffsetResetDelay;

        [Header("Custom Rig")]
        [SerializeField] float aimSpeed;

        [Header("Particles")]
        [SerializeField] ParticleSystem stepParticleSystem;

        // Player Graphics
        private PlayerGraphics playerGraphics;
        public PlayerGraphics PlayerGraphics => playerGraphics;

        private GameObject playerPrefabGraphics;
        private Animator playerAnimator;

        // UI Components
        private IControlBehavior control;
        private UIGame mainUI;

        // Movement
        private bool isRunning;
        private float speed = 0;
        private float maxSpeed;
        private float acceleration;

        // Item system
        private int maxItemsAmount;

        private bool isItemsCarrying;

        private int carryingItemsAmount;
        public int CarryingItemsAmount => carryingItemsAmount;

        private List<ItemStorageCase> carryingItemsList;
        public List<ItemStorageCase> CarryingItemsList => carryingItemsList;

        private float carryingItemsHeight;

        // Carrying
        private int maxAnimalsAmount;

        private bool isAnimalCarrying;

        private AnimalBehaviour carryingAnimal;
        public AnimalBehaviour AnimalBehaviour => carryingAnimal;

        private Transform storageTransform;
        public Transform StorageTransform => storageTransform;

        private GameObject storageHolderPrefab;

        private static Pool storageHolderPool;
        public static Pool StorageHolderPool => storageHolderPool;

        private int carryingAnimalsAmount;
        public int CarryingAnimalsAmount => carryingAnimalsAmount;

        private float carryingAnimalsHeight;

        private List<AnimalStorageCase> carryingAnimalsList;
        public List<AnimalStorageCase> CarryingAnimalsList => carryingAnimalsList;

        private static float maxTextLastTime;

        // Hands
        private bool isHandEnabled;
        private TweenCase handTweenCase;
        private int handLayerHash;

        // Building
        private BuildingBehaviour activeBuilding;
        private bool isBuildingExchangeEnable;
        private Coroutine buildingExchangeCoroutine;

        // Purchase
        private IPurchaseObject activePurchaseObject;
        private Coroutine purchaseCoroutine;
        private bool isPurchaseParticleActive;
        private ParticleSystem moneyParticleSystem;
        private CurrencyType moneyParticleType;

        // Steps particle
        private Transform leftFootTransform;
        private Transform rightFootTransform;

        public bool IsDead { get; private set; }

        // Upgrades
        private MovementSpeedUpgrade movementSpeedUpgrade;
        private CharacterStrengthUpgrade characterStrengthUpgrade;   
        
        // Camera
        private Transform cameraTarget;
        public Transform CameraTarget => cameraTarget;

        private Vector3 cameraCurrentOffset;
        private float cameraResetTime;

        public void Init()
        {
            playerBehavior = this;

            // Create camera target
            GameObject cameraTargetObject = new GameObject("[CameraTarget]");
            cameraTargetObject.transform.ResetGlobal();

            cameraTarget = cameraTargetObject.transform;
            cameraTarget.position = transform.position;

            // Upgrades
            movementSpeedUpgrade = UpgradesController.GetUpgrade<MovementSpeedUpgrade>(UpgradeType.PlayerMovementSpeed);
            movementSpeedUpgrade.OnUpgraded += OnMovementSpeedUpgraded;

            characterStrengthUpgrade = UpgradesController.GetUpgrade<CharacterStrengthUpgrade>(UpgradeType.PlayerStrength);
            characterStrengthUpgrade.OnUpgraded += OnStrengthUpgraded;

            // Initialise storage
            carryingAnimalsAmount = 0;
            carryingAnimalsList = new List<AnimalStorageCase>();

            carryingItemsAmount = 0;
            carryingItemsList = new List<ItemStorageCase>();

            storageHolderPrefab = new GameObject("Storage Holder");
            storageHolderPool = new Pool(storageHolderPrefab, "StorageHolder", storageTransform);

            // Get UI components
            mainUI = UIController.GetPage<UIGame>();

            // Link control
            control = Control.CurrentControl;

            // Reset particle parent
            stepParticleSystem.transform.SetParent(null);

            // Initialise player graphics
            PlayerSkinData selectedSkin = (PlayerSkinData)SkinController.Instance.GetSelectedSkin<PlayerSkinsDatabase>();

            SetGraphics(selectedSkin.Prefab);

            SkinController.SkinSelected += OnSkinSelected;

            // Recalculate values
            RecalculateSpeed();
            RecalculateStrength();

            NavMeshController.InvokeOrSubscribe(delegate
            {
                OnNavMeshUpdated();
            });
        }

        private void OnDestroy()
        {
            SkinController.SkinSelected -= OnSkinSelected;

            movementSpeedUpgrade.OnUpgraded -= OnMovementSpeedUpgraded;
            characterStrengthUpgrade.OnUpgraded -= OnStrengthUpgraded;

            if (cameraTarget != null)
                Destroy(cameraTarget.gameObject);

            if (stepParticleSystem != null)
                Destroy(stepParticleSystem.gameObject);

            if (storageHolderPrefab != null)
                Destroy(storageHolderPrefab);

            if (storageHolderPool != null)
                PoolManager.DestroyPool(storageHolderPool);
        }

        public void Unload()
        {
            enabled = false;

            Destroy(gameObject);
        }

        private void RecalculateSpeed()
        {
            MovementSpeedUpgrade.MovementSpeedStage speedStage = movementSpeedUpgrade.GetCurrentStage();

            maxSpeed = speedStage.PlayerMovementSpeed;
            acceleration = speedStage.PlayerAcceleration;

            agent.speed = maxSpeed;
            agent.acceleration = acceleration;
        }

        private void RecalculateStrength()
        {
            CharacterStrengthUpgrade.CharacterStrengthUpgradeStage strengthUpgradeStage = characterStrengthUpgrade.GetCurrentStage();

            maxItemsAmount = strengthUpgradeStage.ItemsCapacity;
            maxAnimalsAmount = strengthUpgradeStage.AnimalsCapacity;
        }

        private void OnMovementSpeedUpgraded()
        {
            RecalculateSpeed();
        }

        private void OnStrengthUpgraded()
        {
            RecalculateStrength();
        }

        private void OnSkinSelected(ISkinData skinData)
        {
            if (skinData is PlayerSkinData)
            {
                PlayerSkinData selectedSkin = (PlayerSkinData)skinData;

                SetGraphics(selectedSkin.Prefab);
            }
        }

        public void SetGraphics(GameObject graphics)
        {
            // Check if graphics isn't exist already
            if(playerPrefabGraphics != graphics)
            {
                // Store prefab link
                playerPrefabGraphics = graphics;

                if(playerGraphics != null)
                {
                    for(int i = 0; i < carryingAnimalsAmount; i++)
                    {
                        carryingAnimalsList[i].StorageObject.transform.SetParent(null);
                    }

                    for (int i = 0; i < carryingItemsAmount; i++)
                    {
                        carryingItemsList[i].StorageObject.transform.SetParent(null);
                    }

                    isHandEnabled = false;

                    Destroy(playerGraphics.gameObject);
                }

                GameObject playerGraphicObject = Instantiate(graphics);
                playerGraphicObject.transform.SetParent(transform);
                playerGraphicObject.transform.localPosition = Vector3.zero;
                playerGraphicObject.transform.localRotation = Quaternion.identity;
                playerGraphicObject.SetActive(true);

                playerGraphics = playerGraphicObject.GetComponent<PlayerGraphics>();
                playerGraphics.Init(this);

                playerAnimator = playerGraphics.Animator;

                leftFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).GetChild(0);
                rightFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.RightFoot).GetChild(0);

                // Get layer hash
                handLayerHash = playerAnimator.GetLayerIndex("Hands");

                // Get storage transform
                storageTransform = playerGraphics.StorageTransform;

                if(carryingAnimalsAmount > 0)
                {
                    for (int i = 0; i < carryingAnimalsAmount; i++)
                    {
                        carryingAnimalsList[i].StorageObject.transform.SetParent(storageTransform);
                        carryingAnimalsList[i].StorageObject.transform.localRotation = Quaternion.identity;
                        carryingAnimalsList[i].StorageObject.transform.localPosition = STORAGE_OFFSET * i;
                    }

                    EnableHands(0);

                    RegroupAnimals();
                }

                if(carryingItemsAmount > 0)
                {
                    for (int i = 0; i < carryingItemsAmount; i++)
                    {
                        carryingItemsList[i].StorageObject.transform.SetParent(storageTransform);
                        carryingItemsList[i].StorageObject.transform.localRotation = Quaternion.identity;
                        carryingItemsList[i].StorageObject.transform.localPosition = STORAGE_OFFSET * i;
                    }

                    EnableHands(0);

                    RegroupItems();
                }
            }
        }

        public void SetGraphicsState(bool state)
        {
            playerGraphics.gameObject.SetActive(state);
        }

        #region Carrying
        private void EnableHands(float duration = 0)
        {
            if (isHandEnabled)
                return;

            if (handTweenCase != null && !handTweenCase.IsCompleted)
                handTweenCase.Kill();

            if (duration > 0)
            {
                handTweenCase = Tween.DoFloat(0, 1.0f, duration, (time) =>
                {
                    playerAnimator.SetLayerWeight(handLayerHash, time);
                }).OnComplete(delegate
                {
                    isHandEnabled = true;
                });
            }
            else
            {
                playerAnimator.SetLayerWeight(handLayerHash, 1.0f);

                isHandEnabled = true;
            }
        }

        private void DisableHands(float duration = 0)
        {
            if (!isHandEnabled)
                return;

            if (handTweenCase != null && !handTweenCase.IsCompleted)
                handTweenCase.Kill();

            if (duration > 0)
            {
                handTweenCase = Tween.DoFloat(1.0f, 0.0f, duration, (time) =>
                {
                    playerAnimator.SetLayerWeight(handLayerHash, time);
                }).OnComplete(delegate
                {
                    isHandEnabled = false;
                });
            }
            else
            {
                playerAnimator.SetLayerWeight(handLayerHash, 0.0f);

                isHandEnabled = false;
            }
        }

        public bool IsAnimalCarrying()
        {
            return isAnimalCarrying;
        }

        public void CarryAnimal(AnimalBehaviour animalBehaviour)
        {
            AddAnimal(animalBehaviour);
        }

        public bool IsAnimalPickupAllowed()
        {
            return carryingAnimalsAmount < maxAnimalsAmount;
        }

        public AnimalBehaviour GetAnimal(Animal.Type[] allowedAnimalTypes)
        {
            if (carryingAnimalsAmount > 0)
            {
                for (int i = carryingAnimalsAmount - 1; i >= 0; i--)
                {
                    for (int a = 0; a < allowedAnimalTypes.Length; a++)
                    {
                        if (carryingAnimalsList[i].IsPicked && allowedAnimalTypes[a] == carryingAnimalsList[i].AnimalBehaviour.Animal.AnimalType)
                        {
                            return carryingAnimalsList[i].AnimalBehaviour;
                        }
                    }
                }
            }

            return null;
        }

        public AnimalBehaviour GetAnimal()
        {
            if (carryingAnimalsAmount > 0)
            {
                for (int i = carryingAnimalsAmount - 1; i >= 0; i--)
                {
                    if (carryingAnimalsList[i].IsPicked)
                    {
                        return carryingAnimalsList[i].AnimalBehaviour;
                    }
                }
            }

            return null;
        }

        public void RemoveAnimal(AnimalBehaviour animalBehaviour)
        {
            if (carryingAnimalsAmount > 0)
            {
                for (int i = carryingAnimalsAmount - 1; i >= 0; i--)
                {
                    if (carryingAnimalsList[i].IsPicked && carryingAnimalsList[i].AnimalBehaviour == animalBehaviour)
                    {
                        carryingAnimalsList[i].AnimalBehaviour.transform.SetParent(null);
                        carryingAnimalsList[i].Reset();

                        carryingAnimalsAmount--;
                        carryingAnimalsList.RemoveAt(i);

                        if (carryingAnimalsAmount == 0)
                        {
                            // Disable hold animation
                            DisableHands(0.3f);

                            carryingAnimalsHeight = 0;
                            isAnimalCarrying = false;

                            mainUI.SetDropButtonState(false);
                        }

#if MODULE_HAPTIC
                        Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                        break;
                    }
                }

                RegroupAnimals();
            }
        }

        public void DropAnimals(bool disableHands = false)
        {
            if (carryingAnimalsAmount > 0)
            {
                for (int i = carryingAnimalsAmount - 1; i >= 0; i--)
                {
                    Vector3 dropPosition = transform.position + (Random.insideUnitSphere.SetY(0) * 5);

                    AnimalBehaviour storedAnimalBehaviour = carryingAnimalsList[i].AnimalBehaviour;
                    storedAnimalBehaviour.transform.SetParent(null);
                    storedAnimalBehaviour.transform.DOBezierMove(dropPosition, 3, 0, 0, 0.3f).OnComplete(delegate
                    {
                        storedAnimalBehaviour.OnAnimalDropped();
                    });

                    carryingAnimalsList[i].Reset();

                    carryingAnimalsAmount--;
                    carryingAnimalsList.RemoveAt(i);
                }
            }

            carryingAnimalsHeight = 0;
            isAnimalCarrying = false;

            if(disableHands)
            {
                // Disable hold animation
                DisableHands(0.3f);
            }
        }

        public AnimalStorageCase AddAnimal(AnimalBehaviour animalBehaviour)
        {
            if (isItemsCarrying)
                RemoveItems(false);

            EnableHands();

            GameObject storageObject = storageHolderPool.GetPooledObject();
            storageObject.transform.SetParent(storageTransform);
            storageObject.transform.localPosition = new Vector3(0, carryingAnimalsHeight, 0);
            storageObject.transform.localRotation = Quaternion.identity;
            storageObject.transform.localScale = Vector3.one;
            storageObject.SetActive(true);

            AnimalStorageCase storageCase = new AnimalStorageCase(animalBehaviour, storageObject);
            storageCase.SetIndex(carryingAnimalsAmount);

            // Play animation
            animalBehaviour.transform.SetParent(null);
            animalBehaviour.transform.DOBezierFollow(storageObject.transform, Random.Range(3, 5), 0, 0, 0.5f).SetEasing(Ease.Type.SineIn).OnComplete(delegate
            {
                animalBehaviour.transform.localPosition = storageObject.transform.position;
                animalBehaviour.transform.localRotation = storageObject.transform.rotation;

                storageCase.MarkAsPicked();
            });

            carryingAnimal = animalBehaviour;

            carryingAnimalsAmount++;
            carryingAnimalsList.Add(storageCase);

            carryingAnimalsHeight += animalBehaviour.GetCarryingHeight();

            isAnimalCarrying = true;

            mainUI.SetDropButtonState(true);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            return storageCase;
        }

        private void RegroupAnimals()
        {
            carryingAnimalsHeight = 0; 

            if (carryingAnimalsAmount > 0)
            {
                for (int i = 0; i < carryingAnimalsAmount; i++)
                {
                    carryingAnimalsList[i].SetIndex(i);
                    carryingAnimalsList[i].StorageObject.transform.localPosition = new Vector3(0, carryingAnimalsHeight, 0);

                    carryingAnimalsHeight += carryingAnimalsList[i].AnimalBehaviour.GetCarryingHeight();
                }

                carryingAnimal = carryingAnimalsList[0].AnimalBehaviour;
            }
        }
#endregion

        #region Items
        public ItemStorageCase AddItem(Item.Type itemType)
        {
            Item item = ItemController.GetItem(itemType);
            if(item != null)
            {
                if (isAnimalCarrying)
                    DropAnimals(false);

                EnableHands();

                GameObject storageObject = storageHolderPool.GetPooledObject();
                storageObject.transform.SetParent(storageTransform);
                storageObject.transform.localPosition = new Vector3(0, carryingItemsHeight, 0);
                storageObject.transform.localRotation = Quaternion.identity;
                storageObject.SetActive(true);

                GameObject itemGameObject = item.Pool.GetPooledObject();
                itemGameObject.transform.position = storageObject.transform.position;
                itemGameObject.transform.rotation = storageObject.transform.rotation;
                itemGameObject.transform.localScale = Vector3.zero;
                itemGameObject.transform.DOScale(1.0f, 0.1f).SetEasing(Ease.Type.BackOut);
                itemGameObject.SetActive(true);

                ItemStorageCase storageCase = new ItemStorageCase(itemGameObject, itemType, item, storageObject);
                storageCase.SetIndex(carryingItemsAmount);
                storageCase.MarkAsPicked();

                carryingItemsAmount++;
                carryingItemsList.Add(storageCase);

                isItemsCarrying = true;

                carryingItemsHeight += item.ModelHeight;

                TutorialController.OnItemPicked(item);

                mainUI.SetDropButtonState(true);

#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                return storageCase;
            }

            return null;
        }

        public bool HasFreeSpace()
        {
            return carryingItemsAmount < maxItemsAmount;
        }

        public bool HasItem(Item.Type itemType)
        {
            if (carryingItemsAmount > 0)
            {
                for (int i = carryingItemsAmount - 1; i >= 0; i--)
                {
                    if (carryingItemsList[i].ItemType == itemType)
                        return true;
                }
            }

            return false;
        }

        public void RemoveItem(Item.Type itemType)
        {
            if (carryingItemsAmount > 0)
            {
                for (int i = carryingItemsAmount - 1; i >= 0; i--)
                {
                    if (carryingItemsList[i].ItemType == itemType)
                    {
                        carryingItemsList[i].Reset();

                        carryingItemsAmount--;
                        carryingItemsList.RemoveAt(i);

                        if(carryingItemsAmount == 0)
                        {
                            carryingItemsHeight = 0;
                            isItemsCarrying = false;

                            DisableHands(0.3f);

                            mainUI.SetDropButtonState(false);
                        }
                        else
                        {
                            RegroupItems();
                        }

#if MODULE_HAPTIC
                        Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

                        break;
                    }
                }
            }
        }

        public void RegroupItems()
        {
            carryingItemsHeight = 0;

            if (carryingItemsAmount > 0)
            {
                for (int i = 0; i < carryingItemsAmount; i++)
                {
                    carryingItemsList[i].SetIndex(i);
                    carryingItemsList[i].StorageObject.transform.localPosition = new Vector3(0, carryingItemsHeight, 0);

                    carryingItemsHeight += carryingItemsList[i].Item.ModelHeight;
                }
            }
        }

        public void RemoveItems(bool disableHands = false)
        {
            if (carryingItemsAmount > 0)
            {
                for (int i = carryingItemsAmount - 1; i >= 0; i--)
                {
                    carryingItemsList[i].Reset();

                    carryingItemsAmount--;
                    carryingItemsList.RemoveAt(i);
                }
            }

            carryingItemsHeight = 0;
            isItemsCarrying = false;

            if (disableHands)
            {
                // Disable hold animation
                DisableHands(0.3f);
            }
        }
        #endregion

        #region Building
        public void SetActiveBuilding(BuildingBehaviour building)
        {
            activeBuilding = building;
        }

        public void ResetBuildingLink(BuildingBehaviour building)
        {
            if (activeBuilding == building)
                activeBuilding = null;
        }
        #endregion

        #region Purchase
        public void SetPurchaseObject(IPurchaseObject purchaseObject)
        {
            if(activePurchaseObject != null)
                StopCoroutine(purchaseCoroutine);

            activePurchaseObject = purchaseObject;

            purchaseCoroutine = StartCoroutine(PurchaseCoroutine());
        }

        private float moneyPlaceSoundTime;

        private IEnumerator PurchaseCoroutine()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

            yield return waitForSeconds;

            int stepSize = 1;

            waitForSeconds = new WaitForSeconds(0.025f);
            while (!activePurchaseObject.IsOpened)
            {
                yield return waitForSeconds;

                int allowedAmount = stepSize; 
                int placeDifference = activePurchaseObject.Price.Amount - activePurchaseObject.PlacedCurrencyAmount; 
                int currentAmount = CurrencyController.Get(activePurchaseObject.Price.CurrencyType);

                if (placeDifference < allowedAmount)
                    allowedAmount = placeDifference;

                if (currentAmount < allowedAmount)
                    allowedAmount = currentAmount;

                if(allowedAmount != 0 && currentAmount >= allowedAmount)
                {
                    if(!isPurchaseParticleActive)
                    {
                        CurrencyType tempCurrencyType = activePurchaseObject.Price.CurrencyType;
                        if (moneyParticleSystem == null || moneyParticleType != tempCurrencyType)
                        {
                            if(moneyParticleSystem != null)
                                Destroy(moneyParticleSystem.gameObject);

                            Currency currency = CurrencyController.GetCurrency(tempCurrencyType);

                            GameObject particleObject = Instantiate(currency.Data.PurchaseParticlePrefab);
                            particleObject.transform.SetParent(transform);
                            particleObject.transform.ResetLocal();

                            moneyParticleSystem = particleObject.GetComponent<ParticleSystem>();

                            moneyParticleType = tempCurrencyType;
                        }

                        moneyParticleSystem.Play();

                        isPurchaseParticleActive = true;
                    }

                    if(Time.time > moneyPlaceSoundTime)
                    {
                        AudioController.PlaySound(AudioController.AudioClips.moneyPlaceSound);

                        moneyPlaceSoundTime = Time.time + 0.3f;
                    }

                    CurrencyController.Substract(activePurchaseObject.Price.CurrencyType, allowedAmount);

                    activePurchaseObject.PlaceCurrency(allowedAmount);

                    stepSize *= 2;

                    if (activePurchaseObject.PlacedCurrencyAmount >= activePurchaseObject.Price.Amount)
                        break;
                }
                else
                {
                    // Destroy money particle
                    if (moneyParticleSystem != null)
                        moneyParticleSystem.Stop();

                    isPurchaseParticleActive = false;
                }
            }

            activePurchaseObject.OnPurchaseCompleted();

            ResetPurchaseObject(activePurchaseObject);
        }

        public void ResetPurchaseObject(IPurchaseObject purchaseObject)
        {
            if(activePurchaseObject == purchaseObject)
            {
                // Destroy money particle
                if (moneyParticleSystem != null)
                    moneyParticleSystem.Stop();

                isPurchaseParticleActive = false;

                StopCoroutine(purchaseCoroutine);

                activePurchaseObject = null;
            }
        }
        #endregion

        public void LevelStart(Vector3 startPos)
        {
            transform.position = startPos;
            agent.Warp(startPos);
        }

        private void Update()
        {
            if (control.IsMovementInputNonZero && control.MovementInput.sqrMagnitude > 0.1f)
            {
                if (!isRunning)
                {
                    isRunning = true;

                    playerGraphics.Animator.SetBool(RUN_HASH, true);

                    speed = 0;
                }

                float maxAlowedSpeed = control.MovementInput.magnitude * maxSpeed;

                if (speed > maxAlowedSpeed)
                {
                    speed -= acceleration * Time.deltaTime;
                    if (speed < maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }
                else
                {
                    speed += acceleration * Time.deltaTime;
                    if (speed > maxAlowedSpeed)
                    {
                        speed = maxAlowedSpeed;
                    }
                }

                transform.position += control.MovementInput * Time.deltaTime * speed;
                playerGraphics.Animator.SetFloat(MOVEMENT_MULTIPLIER_HASH, speed / maxSpeed);

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(control.MovementInput.normalized), 0.2f); 
                
                // Camera offset
                cameraCurrentOffset = new Vector3(Mathf.Clamp(cameraCurrentOffset.x + (control.MovementInput.normalized.x * cameraOffsetSpeed * Time.deltaTime * Mathf.Abs(control.MovementInput.normalized.x * cameraOffset - cameraCurrentOffset.x)), -cameraOffset, cameraOffset), 0, Mathf.Clamp(cameraCurrentOffset.z + (control.MovementInput.normalized.z * cameraOffsetSpeed * Time.deltaTime * Mathf.Abs(control.MovementInput.normalized.z * cameraOffset - cameraCurrentOffset.z)), -cameraOffset, cameraOffset));

                cameraResetTime = Time.time + cameraOffsetResetDelay;
            }
            else
            {
                if (isRunning)
                {
                    isRunning = false;

                    playerGraphics.Animator.SetBool(RUN_HASH, false);
                }

                if (Time.time >= cameraResetTime)
                {
                    cameraCurrentOffset = new Vector3(Mathf.Lerp(cameraCurrentOffset.x, 0, cameraOffsetResetSpeed * Time.deltaTime), 0, Mathf.Lerp(cameraCurrentOffset.z, 0, cameraOffsetResetSpeed * Time.deltaTime));
                }
            }

            // Recalculate carrying animals position
            if(isAnimalCarrying)
            {
                for(int i = 0; i < carryingAnimalsAmount; i++)
                {
                    if(carryingAnimalsList[i].IsPicked)
                    {
                        carryingAnimalsList[i].AnimalBehaviour.transform.position = carryingAnimalsList[i].StorageObject.transform.position;
                        carryingAnimalsList[i].AnimalBehaviour.transform.rotation = carryingAnimalsList[i].StorageObject.transform.rotation;
                    }
                }
            }

            // Recalculate carrying items position
            if (isItemsCarrying)
            {
                for (int i = 0; i < carryingItemsAmount; i++)
                {
                    if (carryingItemsList[i].IsPicked)
                    {
                        carryingItemsList[i].ItemObject.transform.position = carryingItemsList[i].StorageObject.transform.position;
                        carryingItemsList[i].ItemObject.transform.rotation = carryingItemsList[i].StorageObject.transform.rotation;
                    }
                }
            }

            cameraTarget.position = transform.position + cameraCurrentOffset;
        }

        public void Warp(Transform destination)
        {
            agent.Warp(destination.position);
            transform.rotation = destination.rotation;
        }

        public static PlayerBehavior GetBehavior()
        {
            return playerBehavior;
        }

        public static void DropItemsAndAnimals()
        {
            if (playerBehavior.isItemsCarrying)
                playerBehavior.RemoveItems(true);

            if (playerBehavior.isAnimalCarrying)
                playerBehavior.DropAnimals(true);
        }

        public void LeftFootParticle()
        {
            if (!isRunning)
                return;

            stepParticleSystem.transform.position = leftFootTransform.position - transform.forward * 0.4f;
            stepParticleSystem.Play();
        }

        public void RightFootParticle()
        {
            if (!isRunning)
                return;

            stepParticleSystem.transform.position = rightFootTransform.position - transform.forward * 0.4f;
            stepParticleSystem.Play();
        }

        public static void SpawnMaxText()
        {
            if (Time.time < maxTextLastTime)
                return;

            maxTextLastTime = Time.time + 1.0f;

            FloatingTextController.SpawnFloatingText("Max", "MAX", playerBehavior.transform.position + new Vector3(0, 9, -1), Quaternion.Euler(45, 0, 0), 1.0f, Color.white);
        }

        public void OnNavMeshUpdated()
        {
            agent.enabled = true;
        }
    }
}