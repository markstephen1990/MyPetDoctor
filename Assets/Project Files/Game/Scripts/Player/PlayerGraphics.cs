using UnityEngine;

namespace Watermelon
{
    public class PlayerGraphics : MonoBehaviour
    {
        [SerializeField] Transform storageTransform;

        private PlayerBehavior playerBehavior;

        // Storage
        public Transform StorageTransform => storageTransform;

        // Required components
        private PlayerAnimationHandler playerAnimationHandler;

        private Animator animator;
        public Animator Animator => animator;

        public void Init(PlayerBehavior playerBehavior)
        {
            this.playerBehavior = playerBehavior;

            // Get animator component
            animator = GetComponent<Animator>();

            // Get animator event handler component
            playerAnimationHandler = GetComponent<PlayerAnimationHandler>();
            playerAnimationHandler.Init(playerBehavior);
        }

        #region Editor
        [Button("Create and Init")]
        private void CreateAndInitRequiredObjects()
        {
            // Get animator component
            Animator tempAnimator = GetComponent<Animator>();

            if (tempAnimator != null)
            {
                if (tempAnimator.avatar != null && tempAnimator.avatar.isHuman)
                {
                    // Create storage object
                    GameObject storageObject = new GameObject("Storage");
                    storageObject.transform.SetParent(tempAnimator.GetBoneTransform(HumanBodyBones.LeftHand));
                    storageObject.transform.ResetLocal();

                    storageTransform = storageObject.transform;

                    PlayerAnimationHandler playerAnimationHandler = gameObject.GetComponent<PlayerAnimationHandler>();
                    if (playerAnimationHandler == null)
                        playerAnimationHandler = gameObject.AddComponent<PlayerAnimationHandler>();

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                else
                {
                    Debug.LogError("Avatar is missing or type isn't humanoid!");
                }
            }
            else
            {
                Debug.LogWarning("Animator component can't be found!");
            }
        }
        #endregion
    }
}