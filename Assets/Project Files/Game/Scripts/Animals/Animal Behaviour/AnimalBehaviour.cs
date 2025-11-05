using UnityEngine;
using UnityEngine.AI;

namespace Watermelon
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AnimalBehaviour : MonoBehaviour
    {
        private static readonly int PARTICLE_HEART_TRAIL_HASH = "Heart Trail".GetHashCode();

        public static readonly int RUN_HASH = Animator.StringToHash("Run");
        public static readonly int MOVEMENT_SPEED_HASH = Animator.StringToHash("Movement Speed");

        public const float MOVEMENT_SPEED_DEFAULT = 1.8f;
        public const float MOVEMENT_SEEED_RUN = 3.5f;

        [SerializeField] Animator animalAnimator;
        public Animator AnimalAnimator => animalAnimator;

        [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer => skinnedMeshRenderer;

        [SerializeField] Material defaultMaterial;

        private NavMeshAgent navMeshAgent;
        public NavMeshAgent NavMeshAgent => navMeshAgent;

        private Animal animal;
        public Animal Animal => animal;

        private Zone zone;
        public Zone Zone => zone;

        private AnimalSpawner animalSpawner;
        public AnimalSpawner AnimalSpawner => animalSpawner;

        private bool isMoving;

        private bool isOutlineActive;
        public bool IsOutlineActive => isOutlineActive;

        private Vector3 spawnPoint;
        public Vector3 SpawnPoint => spawnPoint;

        // Carrying
        private IAnimalHolder animalHolder;

        private bool isPicked;
        public bool IsPicked => isPicked;

        private WaitingIndicatorBehaviour activeWaitingIndicator;

        // Sickness
        private bool isCured;
        public bool IsCured => isCured;

        private Sickness activeSickness;
        public Sickness ActiveSickness => activeSickness;

        private SicknessBehaviour activeSicknessBehaviour;

        private ItemIndicatorBehaviour itemIndicatorBehaviour;
        private TableBehaviour tableBehaviour;

        // State machine
        private AnimalStateMachineController animalStateMachineController;
        public AnimalStateMachineController AnimalStateMachineController => animalStateMachineController;

        // AI
        private bool isBusy;
        public bool IsBusy => isBusy;

        private bool isMovementAllowed = true;
        public bool IsMovementAllowed => isMovementAllowed;

        private ParticleCase curedParticleCase;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;
        }

        public void Initialise(Animal animal, Zone zone, AnimalSpawner animalSpawner)
        {
            this.animal = animal;
            this.zone = zone;
            this.animalSpawner = animalSpawner;

            isPicked = false;
            isMoving = false;
            isBusy = false;
            isCured = false;

            animalHolder = null;

            DisableSickness();

            tableBehaviour = null;

            // Enable outline
            SetOutlineState(true);

            // Disable NavMesh component
            navMeshAgent.enabled = false;

            // Initialise state machine controller
            animalStateMachineController = new AnimalStateMachineController();
            animalStateMachineController.Initialise(this, AnimalStateMachineController.State.Carrying);
        }

        private void Update()
        {
            animalStateMachineController.ActiveStateBehaviour.Update();
        }

        public void SetSpawnPoint(Vector3 spawnPoint)
        {
            this.spawnPoint = spawnPoint;
        }

        public void SetAnimalHolder(IAnimalHolder animalHolder)
        {
            this.animalHolder = animalHolder;
        }

        public void OnAnimalPlacedOnGround()
        {
            isPicked = false;

            transform.localScale = Vector3.one;

            // Set waiting state
            animalStateMachineController.SetState(AnimalStateMachineController.State.Waiting);
        }

        public void MarkAsBusy()
        {
            isBusy = true;
        }

        public void OnAnimalPlacedOnTable(TableBehaviour tableBehaviour)
        {
            if (animalStateMachineController.CurrentState == AnimalStateMachineController.State.PlacedOnTable)
                return;

            isPicked = false;

            transform.localScale = Vector3.one;

            this.tableBehaviour = tableBehaviour;

            itemIndicatorBehaviour = ItemController.SpawnIndicator(tableBehaviour.transform.position + new Vector3(0, 5, 2), activeSickness.RequiredItem);

            // Set table state
            animalStateMachineController.SetState(AnimalStateMachineController.State.PlacedOnTable);

            // Disable outline
            SetOutlineState(false);

            TutorialController.OnAnimalPlacedOnTable(this, tableBehaviour);
        }

        public void OnAnimalDropped()
        {
            // Add animal to zone list
            zone.AddWaitingAnimal(this);

            // Set waiting state
            animalStateMachineController.SetState(AnimalStateMachineController.State.ReturnToWaiting);

            isPicked = false;
        }

        public void PickAnimal(IAnimalCarrying carrying)
        {
            if (isPicked)
                return;

            if (carrying != null)
            {
                // Set leaving state
                animalStateMachineController.SetState(AnimalStateMachineController.State.Picked);

                // Remove animal from zone list
                zone.RemoveWaitingAnimal(this);

                if (animalHolder != null)
                {
                    animalHolder.OnAnimalPicked(this);
                    animalHolder = null;
                }

                carrying.CarryAnimal(this);

                isPicked = true;
                isBusy = false;

                TutorialController.OnAnimalPicked(this);
            }
        }

        public void FollowVisitor(VisitorBehaviour visitorBehaviour)
        {
            // Set leaving state
            animalStateMachineController.SetState(AnimalStateMachineController.State.Leaving);
        }

        public void SetMovementSpeed(float movementSpeed)
        {
            navMeshAgent.speed = movementSpeed;

            animalAnimator.SetFloat(MOVEMENT_SPEED_HASH, movementSpeed);
        }

        #region Movement
        public void AllowMovement()
        {
            isMovementAllowed = true;
        }

        public void BlockMovement()
        {
            isMovementAllowed = false;

            if (isMoving)
            {
                isMoving = false;
                navMeshAgent.enabled = false;

                animalAnimator.SetBool(RUN_HASH, false);
            }
        }

        public void StartMovement()
        {
            if (isMoving || !isMovementAllowed)
                return;

            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;

            animalAnimator.SetBool(RUN_HASH, true);

            isMoving = true;
        }

        public void StopMovement()
        {
            if (!isMoving)
                return;

            isMoving = false;
            navMeshAgent.enabled = false;

            animalAnimator.SetBool(RUN_HASH, false);
        }
        #endregion

        #region Sickness
        public bool CanBeCured(IItemCarrying itemCarrying)
        {
            if (isCured)
                return false;

            if (activeSickness == null)
                return false;

            return (itemCarrying != null && itemCarrying.HasItem(activeSickness.RequiredItem));
        }

        public void CureAnimal(IItemCarrying itemCarrying)
        {
            if (isCured)
                return;

            isCured = true;

            itemCarrying.RemoveItem(activeSickness.RequiredItem);

            OnAnimalCured();

            tableBehaviour.OnAnimalCured(itemCarrying.Transform);
            itemIndicatorBehaviour.Disable();

            isBusy = false;
        }

        public void OnAnimalCured()
        {
            DisableSickness(true);

            zone.OnMoneyAdded(animal.Reward);

            Tween.DelayedCall(1.0f, delegate
            {
                // Jump and run away
                transform.SetParent(null);
                transform.DOMoveY(0, 0.5f).SetEasing(Ease.Type.BackIn);
                transform.DOMoveXZ(transform.position.x + (transform.forward.x * 4.4f), transform.position.z + (transform.forward.z * 4.4f), 0.5f).OnComplete(delegate
                {
                    zone.OnAnimalCured(this);

                    curedParticleCase = ParticlesController.PlayParticle(PARTICLE_HEART_TRAIL_HASH).SetTarget(transform, new Vector3(0, 0.5f, 0)).SetDuration(4.0f);
                });
            });

            TutorialController.OnAnimalCured(this);
        }

        public void OnAnimalCuredAndPicked()
        {

        }

        public void OnDisabled()
        {
            DisableSickness();

            // Disable NavMesh component
            navMeshAgent.enabled = false;

            if(curedParticleCase != null)
            {
                curedParticleCase.ForceDisable();
            }
        }


        public void SetSickness(Sickness sickness)
        {
            activeSicknessBehaviour = sickness.GetSicknessBehaviour(animal.AnimalType);
            activeSicknessBehaviour.transform.SetParent(transform);
            activeSicknessBehaviour.transform.ResetLocal();
            activeSicknessBehaviour.gameObject.SetActive(true);
            activeSicknessBehaviour.Activate(this);

            activeSickness = sickness;

            SetOutlineState(isOutlineActive);
        }

        private void DisableSickness(bool playCureParticle = false)
        {
            if (activeSicknessBehaviour != null)
            {
                if (playCureParticle)
                    activeSicknessBehaviour.PlayCureParticle();

                activeSicknessBehaviour.DisableSickness();

                Destroy(activeSicknessBehaviour.gameObject);

                activeSicknessBehaviour = null;
            }

            activeSickness = null;

            SetOutlineState(isOutlineActive);
        }
        #endregion

        #region Waiting Indicator
        public void CreateWaitingIndicator(IAnimalCarrying carrying)
        {
            if (isCured)
                return;

            activeWaitingIndicator = LevelController.CreateWaitingIndicator(transform.position + new Vector3(0, 3, 2));
            activeWaitingIndicator.Initialise(LevelController.AnimalPickUpDuration, delegate
            {
                if (carrying.IsAnimalPickupAllowed())
                {
                    PickAnimal(carrying);

                    AudioController.PlaySound(AudioController.AudioClips.animalPickUpSound);
                }
                else
                {
                    PlayerBehavior.SpawnMaxText();
                }
            });
        }

        public void DestroyWaitingIndicator()
        {
            if (activeWaitingIndicator != null)
            {
                if (activeWaitingIndicator.IsActive)
                {
                    activeWaitingIndicator.Disable();
                    activeWaitingIndicator = null;
                }
            }
        }
        #endregion

        #region Material
        public void ResetMaterial()
        {
            skinnedMeshRenderer.sharedMaterial = defaultMaterial;
        }

        public void SetMaterial(Material material)
        {
            skinnedMeshRenderer.sharedMaterial = material;
        }
        #endregion

        #region Outline
        public void SetOutlineState(bool state)
        {
            isOutlineActive = state;

            Material material = skinnedMeshRenderer.material;
            if(state)
            {
                material.EnableKeyword("OUUTLINE_ON");
            }
            else
            {
                material.DisableKeyword("OUUTLINE_ON");
            }

            material.SetShaderPassEnabled("SRPDefaultUnlit", state);

            if (activeSicknessBehaviour != null)
                activeSicknessBehaviour.SetOutlineState(state);
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            animalStateMachineController.ActiveStateBehaviour.OnTriggerEnter(other);
        }
        private void OnTriggerExit(Collider other)
        {
            animalStateMachineController.ActiveStateBehaviour.OnTriggerExit(other);
        }

        public float GetCarryingHeight()
        {
            if (activeSicknessBehaviour != null)
            {
                return animal.CarryingHeight + activeSicknessBehaviour.ExtraCarryingHeight;
            }

            return animal.CarryingHeight;
        }
    }
}