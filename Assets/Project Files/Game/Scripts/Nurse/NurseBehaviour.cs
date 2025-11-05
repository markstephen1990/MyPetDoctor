using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class NurseBehaviour : MonoBehaviour, IAnimalCarrying, IAnimalHolder, IItemCarrying, IDoorOpener
    {
        public static readonly int IDLE_HASH = Animator.StringToHash("Idle");
        public static readonly int RUN_HASH = Animator.StringToHash("Run");

        public static readonly int MOVEMENT_MULTIPLIER_HASH = Animator.StringToHash("Movement Multiplier");

        private const int MAX_ITEMS_AMOUNT = 2;

        [SerializeField] Animator visitorAnimator;
        [SerializeField] Transform carryingContainer;

        // Components
        private NavMeshAgent navMeshAgent;
        private CapsuleCollider capsuleCollider;

        // Carrying
        private bool isAnimalCarrying;

        private AnimalBehaviour carryingAnimal;
        public AnimalBehaviour AnimalBehaviour => carryingAnimal;

        private int carryingAnimalsAmount;
        public int CarryingAnimalsAmount => carryingAnimalsAmount;

        private List<AnimalStorageCase> carryingAnimalsList;
        public List<AnimalStorageCase> CarryingAnimalsList => carryingAnimalsList;

        private float carryingAnimalsHeight;

        // Hands
        private bool isHandEnabled;
        private TweenCase handTweenCase;
        private int handLayerHash;

        // Movement
        private Vector3 movementTarget;
        private bool isMoving;

        // Item system
        private bool isItemsCarrying;
        public bool IsItemCarrying => isItemsCarrying;

        private float carryingItemsHeight;

        private int carryingItemsAmount;
        public int CarryingItemsAmount => carryingItemsAmount;

        private List<ItemStorageCase> carryingItemsList;
        public List<ItemStorageCase> CarryingItemsList => carryingItemsList;

        // State machine
        private NurseStateMachineController nurseStateMachineController;

        public Animator VisitorAnimator => visitorAnimator;
        public NavMeshAgent NavMeshAgent => navMeshAgent;

        private Zone zone;
        public Zone Zone => zone;

        public Transform Transform => transform;

        private void Awake()
        {
            // Get components
            navMeshAgent = GetComponent<NavMeshAgent>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            // Get layer hash
            handLayerHash = visitorAnimator.GetLayerIndex("Hands");
        }

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            // Initialise storage
            carryingAnimalsAmount = 0;
            carryingAnimalsList = new List<AnimalStorageCase>();

            carryingItemsAmount = 0;
            carryingItemsList = new List<ItemStorageCase>();

            isAnimalCarrying = false;
            carryingAnimalsHeight = 0;

            // Disable hands
            DisableHands();

            // Initialise state machine controller
            nurseStateMachineController = new NurseStateMachineController();
            nurseStateMachineController.Initialise(this, NurseStateMachineController.State.Waiting);

            NavMeshController.InvokeOrSubscribe(delegate
            {
                OnNavMeshUpdated();
            });
        }

        private void Update()
        {
            nurseStateMachineController.ActiveStateBehaviour.Update();
        }

        public void RecalculateMoveSpeed()
        {
            navMeshAgent.acceleration = LevelController.NurseAcceleration;
            navMeshAgent.speed = LevelController.NurseMovementSpeed;
            navMeshAgent.angularSpeed = LevelController.NurseAngularSpeed;

            visitorAnimator.SetFloat(MOVEMENT_MULTIPLIER_HASH, LevelController.NurseBlendTreeMultiplier);
        }

        #region Movement
        public void SetTargetPosition(Vector3 target)
        {
            navMeshAgent.SetDestination(target);

            movementTarget = target;

            visitorAnimator.ResetTrigger(IDLE_HASH);
            visitorAnimator.SetTrigger(RUN_HASH);

            isMoving = true;
        }

        public void StopMovement()
        {
            if (!isMoving)
                return;

            isMoving = false;

            visitorAnimator.ResetTrigger(RUN_HASH);
            visitorAnimator.SetTrigger(IDLE_HASH);
        }
        #endregion

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
                    visitorAnimator.SetLayerWeight(handLayerHash, time);
                }).OnComplete(delegate
                {
                    isHandEnabled = true;
                });
            }
            else
            {
                visitorAnimator.SetLayerWeight(handLayerHash, 1.0f);

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
                    visitorAnimator.SetLayerWeight(handLayerHash, time);
                }).OnComplete(delegate
                {
                    isHandEnabled = false;
                });
            }
            else
            {
                visitorAnimator.SetLayerWeight(handLayerHash, 0.0f);

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
            return carryingAnimalsAmount < 5;
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

                            isAnimalCarrying = false;
                        }

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
                    Vector3 dropPosition = transform.position + Random.insideUnitSphere.SetY(0) * 5;

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

            isAnimalCarrying = false;
            carryingAnimalsHeight = 0;

            if (disableHands)
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

            GameObject storageObject = PlayerBehavior.StorageHolderPool.GetPooledObject();
            storageObject.transform.SetParent(carryingContainer);
            storageObject.transform.localPosition = new Vector3(0, carryingAnimalsHeight, 0);
            storageObject.transform.localRotation = Quaternion.identity;
            storageObject.transform.localScale = Vector3.one;
            storageObject.SetActive(true);

            AnimalStorageCase storageCase = new AnimalStorageCase(animalBehaviour, storageObject);
            storageCase.SetIndex(carryingAnimalsAmount);

            // Play animation
            animalBehaviour.transform.SetParent(null);
            animalBehaviour.transform.DORotate(storageObject.transform.rotation, 0.5f);
            animalBehaviour.transform.DOBezierFollow(storageObject.transform, Random.Range(3, 5), 0, 0, 0.5f).SetEasing(Ease.Type.SineIn).OnComplete(delegate
            {
                animalBehaviour.transform.SetParent(storageObject.transform);
                animalBehaviour.transform.localPosition = Vector3.zero;
                animalBehaviour.transform.localRotation = Quaternion.identity;

                storageCase.MarkAsPicked();
            });

            carryingAnimal = animalBehaviour;

            carryingAnimalsAmount++;
            carryingAnimalsList.Add(storageCase);

            carryingAnimalsHeight += animalBehaviour.GetCarryingHeight();

            isAnimalCarrying = true;

            return storageCase;
        }

        private void RegroupAnimals()
        {
            carryingAnimalsHeight = 0;

            if (carryingAnimalsAmount > 0)
            {
                for (int i = 0; i < carryingAnimalsAmount; i++)
                {
                    if (carryingAnimalsList[i].Index != i)
                    {
                        carryingAnimalsList[i].SetIndex(i);
                        carryingAnimalsList[i].StorageObject.transform.localPosition = new Vector3(0, carryingAnimalsHeight, 0);

                        carryingAnimalsHeight += carryingAnimalsList[i].AnimalBehaviour.GetCarryingHeight();
                    }
                }

                carryingAnimal = carryingAnimalsList[0].AnimalBehaviour;
            }
        }
        #endregion

        #region Items
        public Item.Type GetItem()
        {
            if (carryingItemsAmount > 0)
                return carryingItemsList[carryingItemsAmount - 1].ItemType;

            return Item.Type.None;
        }

        public ItemStorageCase AddItem(Item.Type itemType)
        {
            Item item = ItemController.GetItem(itemType);
            if (item != null)
            {
                if (isAnimalCarrying)
                    DropAnimals(false);

                EnableHands();

                GameObject storageObject = PlayerBehavior.StorageHolderPool.GetPooledObject();
                storageObject.transform.SetParent(carryingContainer);
                storageObject.transform.localPosition = new Vector3(0, carryingItemsHeight, 0);
                storageObject.transform.localRotation = Quaternion.identity;
                storageObject.SetActive(true);

                GameObject itemGameObject = item.Pool.GetPooledObject();
                itemGameObject.transform.SetParent(storageObject.transform);
                itemGameObject.transform.ResetLocal();
                itemGameObject.transform.localScale = Vector3.zero;
                itemGameObject.transform.DOScale(1.0f, 0.2f).SetEasing(Ease.Type.BackOut);
                itemGameObject.SetActive(true);

                ItemStorageCase storageCase = new ItemStorageCase(itemGameObject, itemType, item, storageObject);
                storageCase.SetIndex(carryingItemsAmount);

                carryingItemsAmount++;
                carryingItemsList.Add(storageCase);

                isItemsCarrying = true;

                carryingItemsHeight += item.ModelHeight;

                return storageCase;
            }

            return null;
        }

        public bool HasFreeSpace()
        {
            return carryingItemsAmount < MAX_ITEMS_AMOUNT;
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

                        if (carryingItemsAmount == 0)
                        {
                            carryingItemsHeight = 0;
                            isItemsCarrying = false;

                            DisableHands(0.3f);
                        }
                        else
                        {
                            RegroupItems();
                        }

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

        public void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (isAnimalCarrying && carryingAnimal == animalBehaviour)
            {
                isAnimalCarrying = false;

                StopMovement();

                DisableHands(0.2f);

                Tween.DelayedCall(0.8f, delegate
                {
                    // Disable visitor
                    DisableVisitor();
                });
            }
        }

        public void DisableVisitor()
        {
            isAnimalCarrying = false;
            carryingAnimal = null;

            DisableHands();

            transform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }

        private void OnDisable()
        {
            isAnimalCarrying = false;
            carryingAnimal = null;

            navMeshAgent.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            nurseStateMachineController.ActiveStateBehaviour.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            nurseStateMachineController.ActiveStateBehaviour.OnTriggerExit(other);
        }

        public void OnNavMeshUpdated()
        {
            navMeshAgent.enabled = true;
        }
    }
}