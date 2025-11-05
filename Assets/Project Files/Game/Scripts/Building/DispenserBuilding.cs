using UnityEngine;

namespace Watermelon
{
    public class DispenserBuilding : MonoBehaviour, IInteractableZone
    {
        [SerializeField] Item.Type itemType;
        public Item.Type ItemType => itemType;

        [SerializeField] WaitingIndicatorBehaviour waitingIndicatorBehaviour;
        [SerializeField] InteractableZoneBehaviour interactableZoneBehaviour;

        [Space]
        [SerializeField] GameObject graphicsGameObject;
        [SerializeField] GameObject lockGameObject;
        [SerializeField] Transform rotateTransform;
        [SerializeField] SpriteRenderer iconSpriteRenderer;

        [Space]
        [SerializeField] float openTime;
        [SerializeField] Ease.Type openEasing;

        [SerializeField] float closeTime;
        [SerializeField] Ease.Type closeEasing;

        private bool isPlayerEnteredZone;
        private PlayerBehavior playerBehavior;

        private TweenCase openTweenCase;

        private bool isUnlocked = true;
        public bool IsUnlocked => isUnlocked;

        public void Init()
        {
            interactableZoneBehaviour.Initialise(this);

            waitingIndicatorBehaviour.gameObject.SetActive(false);

            Item item = ItemController.GetItem(itemType);
            iconSpriteRenderer.sprite = item.Icon;

            graphicsGameObject.SetActive(isUnlocked);
            lockGameObject.SetActive(!isUnlocked);
        }

        public void PickItem(IItemCarrying itemCarrying)
        {
            if (!isUnlocked)
                return;

            PlayOpenAnimation();

            itemCarrying.AddItem(itemType);
        }

        private void EnableDispenser()
        {
            waitingIndicatorBehaviour.gameObject.SetActive(true);
            waitingIndicatorBehaviour.Initialise(LevelController.ItemPickUpDuration, delegate
            {
                PlayOpenAnimation();

                playerBehavior.AddItem(itemType);

                AudioController.PlaySound(AudioController.AudioClips.itemPickUpSound);

                if (playerBehavior.HasFreeSpace())
                {
                    Tween.DelayedCall(0.1f, delegate
                    {
                        if (isPlayerEnteredZone)
                            EnableDispenser();
                    });
                }
                else
                {
                    PlayerBehavior.SpawnMaxText();
                }
            });
        }

        public void OnZoneEnter(PlayerBehavior playerBehavior)
        {
            if (!isUnlocked)
                return;

            this.playerBehavior = playerBehavior;

            isPlayerEnteredZone = true;

            if (playerBehavior.HasFreeSpace())
            {
                EnableDispenser();
            }
            else
            {
                PlayerBehavior.SpawnMaxText();
            }
        }

        public void OnZoneExit(PlayerBehavior playerBehavior)
        {
            if (!isUnlocked)
                return;

            isPlayerEnteredZone = false;

            waitingIndicatorBehaviour.Disable();
        }

        private void PlayOpenAnimation()
        {
            if (openTweenCase != null && !openTweenCase.IsCompleted)
                openTweenCase.Kill();

            rotateTransform.localRotation = Quaternion.identity;

            openTweenCase = rotateTransform.DOLocalRotate(Quaternion.Euler(-98, 0, 0), openTime).SetEasing(openEasing).OnComplete(delegate
            {
                openTweenCase = Tween.DelayedCall(0.1f, delegate
                {
                    openTweenCase = rotateTransform.DOLocalRotate(Quaternion.identity, closeTime).SetEasing(closeEasing);
                });
            });
        }

        public void SetUnlockState(bool state)
        {
            isUnlocked = state;

            //graphicsGameObject.SetActive(isUnlocked);
            lockGameObject.SetActive(!isUnlocked);
        }
    }
}