using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.LevelSystem;

namespace Watermelon
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
    public class VisitorBehaviour : MonoBehaviour, IAnimalCarrying, IAnimalHolder, IDoorOpener
    {
        public static readonly int RUN_HASH = Animator.StringToHash("Run");
        public static readonly int PICK_HASH = Animator.StringToHash("Pick");

        private static readonly int MOVEMENT_SPEED_HASH = Animator.StringToHash("Movement Speed");

        [SerializeField] Animator visitorAnimator;
        [SerializeField] Transform carryingContainer;

        // Components
        private NavMeshAgent navMeshAgent;
        public NavMeshAgent NavMeshAgent => navMeshAgent;

        private VisitorAnimatorCallbackHandler visitorAnimatorCallbackHandler;

        // Carrying
        private bool isAnimalCarrying;

        private AnimalBehaviour targetAnimal;

        private AnimalBehaviour carryingAnimal;
        public AnimalBehaviour AnimalBehaviour => carryingAnimal;

        private TweenCase animalPlacingPositionTweenCase;
        private TweenCase animalPlacingRotationTweenCase;

        // Hands
        private bool isHandEnabled;
        private TweenCase handTweenCase;
        private int handLayerHash;

        // Movement
        private Coroutine movementCoroutine;
        private Vector3 movementTarget;
        private SimpleCallback movementCallback;
        private bool isRecalculationRequired;
        private bool isMoving;

        private Vector3 spawnPosition;
        public Vector3 SpawnPosition => spawnPosition;

        private Vector3 placePosition;
        public Vector3 PlacePosition => placePosition;

        // Spawner
        private AnimalSpawner animalSpawner;

        // AI
        private VisitorStateMachineController visitorStateMachineController;

        public Transform Transform => transform;

        private void Awake()
        {
            // Get components
            navMeshAgent = GetComponent<NavMeshAgent>();

            // Get layer hash
            handLayerHash = visitorAnimator.GetLayerIndex("Hands");

            // Initialise animator callback handler
            visitorAnimatorCallbackHandler = visitorAnimator.GetComponent<VisitorAnimatorCallbackHandler>();
            visitorAnimatorCallbackHandler.Init(this);

            // Disable NavMesh
            navMeshAgent.enabled = false;
        }

        public void Initialise(AnimalSpawner animalSpawner, Vector3 spawnPosition)
        {
            this.animalSpawner = animalSpawner;
            this.spawnPosition = spawnPosition;

            transform.position = spawnPosition;

            // Reset variables
            isAnimalCarrying = false;
            carryingAnimal = null;

            StopMovement();

            // Disable hands
            DisableHands();

            // Initialise state machine
            visitorStateMachineController = new VisitorStateMachineController();
            visitorStateMachineController.Initialise(this, VisitorStateMachineController.State.Waiting);
        }

        private void Update()
        {
            // Invoke state machine update method
            visitorStateMachineController.ActiveStateBehaviour.Update();

            visitorAnimator.SetFloat(MOVEMENT_SPEED_HASH, navMeshAgent.velocity.magnitude / navMeshAgent.speed);
        }

        public void StartDelivering()
        {
            NavMeshController.InvokeOrSubscribe(delegate
            {
                OnNavMeshUpdated();
            });
        }

        public void SetPlacePosition(Vector3 position)
        {
            placePosition = position;
        }

        public void TakeAnimal(AnimalBehaviour animalBehaviour)
        {
            targetAnimal = animalBehaviour;

            NavMeshController.InvokeOrSubscribe(delegate
            {
                OnNavMeshUpdated();
            });
        }

        #region Movement
        public void SetTargetPosition(Vector3 target, SimpleCallback callback)
        {
            isRecalculationRequired = true;
            movementTarget = target;
            movementCallback = callback;

            if (!isMoving)
                movementCoroutine = StartCoroutine(MovingCoroutine());
        }

        private IEnumerator MovingCoroutine()
        {
            isMoving = true;
            navMeshAgent.enabled = true;

            visitorAnimator.SetBool(RUN_HASH, true);

            do
            {
                if (isRecalculationRequired)
                {
                    navMeshAgent.SetDestination(movementTarget);
                }

                if (!navMeshAgent.pathPending && !navMeshAgent.hasPath)
                {
                    navMeshAgent.SetDestination(movementTarget);
                }

                yield return null;
            }
            while (Vector3.Distance(transform.position, movementTarget) > navMeshAgent.stoppingDistance);

            movementCallback?.Invoke();

            isMoving = false;
            navMeshAgent.enabled = false;

            visitorAnimator.SetBool(RUN_HASH, false);
        }

        public void StopMovement()
        {
            if (!isMoving)
                return;

            isMoving = false;
            navMeshAgent.enabled = false;

            if (movementCoroutine != null)
                StopCoroutine(movementCoroutine);

            movementTarget = transform.position;
            movementCallback = null;

            visitorAnimator.SetBool(RUN_HASH, false);
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

        public bool IsAnimalPickupAllowed()
        {
            return !isAnimalCarrying;
        }

        public bool IsAnimalCarrying()
        {
            return isAnimalCarrying;
        }

        public void CarryAnimal(AnimalBehaviour animalBehaviour)
        {
            if (isAnimalCarrying)
                return;

            isAnimalCarrying = true;

            EnableHands();

            animalBehaviour.transform.SetParent(carryingContainer);
            animalBehaviour.transform.localPosition = Vector3.zero;
            animalBehaviour.transform.localRotation = Quaternion.identity;
            animalBehaviour.transform.localScale = Vector3.zero;
            animalBehaviour.transform.DOScale(1.0f, 0.3f).SetEasing(Ease.Type.BackOut);

            animalBehaviour.SetAnimalHolder(this);

            carryingAnimal = animalBehaviour;
        }

        public AnimalBehaviour GetAnimal(Animal.Type[] allowedAnimalTypes)
        {
            if (isAnimalCarrying)
            {
                for (int i = 0; i < allowedAnimalTypes.Length; i++)
                {
                    if (allowedAnimalTypes[i] == carryingAnimal.Animal.AnimalType)
                    {
                        return carryingAnimal;
                    }
                }
            }

            return null;
        }

        public AnimalBehaviour GetAnimal()
        {
            if (isAnimalCarrying)
            {
                return carryingAnimal;
            }

            return null;
        }

        public void PlaceAnimalOnGround()
        {
            // Activate place animation
            visitorAnimator.SetTrigger(PICK_HASH);
        }

        public void OnPlaceAnimation()
        {
            if (!isAnimalCarrying)
                return;

            if (carryingAnimal != null)
            {
                // Store carrying animal behaviour
                AnimalBehaviour activeAnimal = carryingAnimal;

                // Remove carrying animal
                RemoveAnimal(activeAnimal);

                // Play place animation
                animalPlacingRotationTweenCase = activeAnimal.transform.DORotate(transform.rotation, 0.3f);
                animalPlacingPositionTweenCase = activeAnimal.transform.DOMove(transform.position + transform.forward * 2.5f, 0.3f).SetEasing(Ease.Type.SineOut).OnComplete(delegate
                {
                    // Call animal place method
                    activeAnimal.OnAnimalPlacedOnGround();

                    // Distable carrying animation
                    DisableHands();

                    Tween.DelayedCall(0.5f, delegate
                    {
                        // Go back to spawn point
                        visitorStateMachineController.SetState(VisitorStateMachineController.State.Leaving);
                    });
                });
            }

            isAnimalCarrying = false;
            carryingAnimal = null;
        }
        #endregion

        public void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (animalPlacingRotationTweenCase != null && !animalPlacingRotationTweenCase.IsCompleted)
                animalPlacingRotationTweenCase.Kill();

            if (animalPlacingPositionTweenCase != null && !animalPlacingPositionTweenCase.IsCompleted)
                animalPlacingPositionTweenCase.Kill();

            if (isAnimalCarrying && carryingAnimal == animalBehaviour)
            {
                if (carryingAnimal != null)
                {
                    carryingAnimal.transform.SetParent(null);

                    isAnimalCarrying = false;
                    carryingAnimal = null;
                }

                DisableHands();

                if (visitorStateMachineController.CurrentState != VisitorStateMachineController.State.Leaving)
                {
                    StopMovement();

                    Tween.NextFrame(delegate
                    {
                        visitorStateMachineController.SetState(VisitorStateMachineController.State.Leaving);
                    });
                }
            }
        }

        public void DisableVisitor()
        {
            // Play scale animation
            transform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn).OnComplete(delegate
            {
                // Reset carrying animal
                if (isAnimalCarrying)
                {
                    carryingAnimal.transform.SetParent(null);
                    carryingAnimal.transform.ResetGlobal();
                    carryingAnimal.gameObject.SetActive(false);
                    carryingAnimal.OnDisabled();
                }

                isAnimalCarrying = false;
                carryingAnimal = null;

                targetAnimal = null;

                // Let animal spawner know that visitor has disabled
                animalSpawner.OnVisitorDisabled(this);

                // Disable NavMesh component
                navMeshAgent.enabled = false;

                // Disable game object and return it to pool
                gameObject.SetActive(false);
            });
        }

        public void RemoveAnimal(AnimalBehaviour animalBehaviour)
        {
            if (isAnimalCarrying)
            {
                carryingAnimal.transform.SetParent(null);

                isAnimalCarrying = false;
                carryingAnimal = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Invoke state machine trigger method
            visitorStateMachineController.ActiveStateBehaviour.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            // Invoke state machine trigger method
            visitorStateMachineController.ActiveStateBehaviour.OnTriggerExit(other);
        }

        public void OnNavMeshUpdated()
        {
            navMeshAgent.enabled = true;

            if (targetAnimal != null)
            {
                VisitorPickingAnimalStateBehaviour visitorPickingAnimalStateBehaviour = (VisitorPickingAnimalStateBehaviour)visitorStateMachineController.GetState(VisitorStateMachineController.State.PickingAnimal);
                visitorPickingAnimalStateBehaviour.SetAnimal(targetAnimal);

                visitorStateMachineController.SetState(VisitorStateMachineController.State.PickingAnimal);
            }
            else
            {
                // Deliver animal
                visitorStateMachineController.SetState(VisitorStateMachineController.State.Delivering);
            }
        }

        private void OnDisable()
        {
            isAnimalCarrying = false;
            carryingAnimal = null;

            targetAnimal = null;

            // Disable NavMesh component
            navMeshAgent.enabled = false;
        }
    }
}