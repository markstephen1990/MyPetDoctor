using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class NavigationController : MonoBehaviour
    {
        public const float PATH_UPDATE_TIME = 0.1f;

        [SerializeField] GameObject arrowPrefab;

        private static List<ArrowCase> activeArrows = new List<ArrowCase>();

        private static Pool arrowPool;

        public void Init()
        {
            arrowPool = new Pool(arrowPrefab, "Nivagation Arrow");

            activeArrows = new List<ArrowCase>();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < activeArrows.Count; i++)
            {
                activeArrows[i].DisableArrow();
            }

            if (arrowPool != null)
                PoolManager.DestroyPool(arrowPool);

            activeArrows = null;
            arrowPool = null;
        }

        public static void Unload()
        {
            arrowPool.ReturnToPoolEverything(true);

            for(int i = 0; i < activeArrows.Count; i++)
            {
                activeArrows[i].DisableArrow();
            }

            activeArrows.Clear();
        }

        public static ArrowCase RegisterArrow(Transform parent, Vector3 target, Color color, float hideDistance = -1, bool autoDisable = false)
        {
            ArrowCase arrowCase = new ArrowCase(parent, arrowPool.GetPooledObject(), color, null, target, hideDistance, autoDisable);

            activeArrows.Add(arrowCase);

            return arrowCase;
        }

        public static ArrowCase RegisterArrow(Transform parent, Transform target, Color color, float hideDistance = -1, bool autoDisable = false)
        {
            ArrowCase arrowCase = new ArrowCase(parent, arrowPool.GetPooledObject(), color, target, Vector3.zero, hideDistance, autoDisable);

            activeArrows.Add(arrowCase);

            return arrowCase;
        }

        private void LateUpdate()
        {
            if (activeArrows.Count > 0)
            {
                for (int i = 0; i < activeArrows.Count; i++)
                {
                    activeArrows[i].LateUpdate();

                    if (activeArrows[i].IsTargetReached)
                    {
                        activeArrows.RemoveAt(i);

                        i--;
                    }
                }
            }
        }

        public class ArrowCase
        {
            private Vector3 targetPosition;
            private Transform targetTransform;

            private Transform parentTransform;

            private Transform arrowTransform;
            private bool autoDisable;
            private float hideDistance;

            public bool IsTargetReached { get; private set; }

            private MaterialPropertyBlock propertyBlock;
            private MeshRenderer rendererRef;

            private float currentDistanceToTarget;

            public ArrowCase(Transform parentTransform, GameObject arrowObject, Color color, Transform targetTransform, Vector3 targetPosition, float hideDistance = -1, bool autoDisable = false)
            {
                this.parentTransform = parentTransform;

                // Get transform refference and reset parent
                arrowTransform = arrowObject.transform;
                arrowTransform.SetParent(null);

                this.hideDistance = hideDistance;
                this.autoDisable = autoDisable;

                // Set target position
                this.targetTransform = targetTransform;
                if (targetTransform != null)
                {
                    this.targetPosition = targetTransform.position;
                }
                else
                {
                    this.targetPosition = targetPosition;
                }

                // Initialise property block and change arrow color
                propertyBlock = new MaterialPropertyBlock();

                // Get mesh renderer
                rendererRef = arrowTransform.GetChild(0).GetComponent<MeshRenderer>();
                rendererRef.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                rendererRef.SetPropertyBlock(propertyBlock);

                // Prepare arrow position and rotation
                arrowTransform.position = parentTransform.position;
                arrowTransform.localScale = Vector3.zero;
                arrowTransform.rotation = Quaternion.LookRotation((this.targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up);

                // Enable arrow object
                arrowObject.SetActive(true);

                // Do scale animation
                arrowTransform.DOScale(1.0f, 0.4f).SetEasing(Ease.Type.SineIn);
            }

            public void UpateData(Vector3 targetPosition, float hideDistance = -1f, bool autoDisable = false)
            {
                this.targetPosition = targetPosition;
                this.hideDistance = hideDistance;
                this.autoDisable = autoDisable;

                targetTransform = null;
            }

            public void UpateData(Transform targetTransform, float hideDistance = -1f, bool autoDisable = false)
            {
                this.targetTransform = targetTransform;
                this.hideDistance = hideDistance;
                this.autoDisable = autoDisable;

                targetPosition = targetTransform.position;
            }

            public void LateUpdate()
            {
                // Fix arrow transform to player position
                arrowTransform.position = parentTransform.position;

                if (IsTargetReached)
                    return;

                currentDistanceToTarget = Vector3.Distance(arrowTransform.position, targetPosition);

                // Check if possible to disable
                if (autoDisable && currentDistanceToTarget < hideDistance)
                {
                    DisableArrow();
                }

                // Check if hide state changed
                if (hideDistance != -1 && rendererRef.enabled && currentDistanceToTarget < hideDistance)
                {
                    rendererRef.enabled = false;
                }

                if (hideDistance != -1 && !rendererRef.enabled && currentDistanceToTarget > hideDistance)
                {
                    rendererRef.enabled = true;
                }

                if (!rendererRef.enabled)
                    return;

                // Rotate arrow to target
                arrowTransform.rotation = Quaternion.Lerp(arrowTransform.rotation, Quaternion.LookRotation((targetPosition - parentTransform.position).SetY(0).normalized, Vector3.up), 0.075f);

                if (targetTransform != null)
                    targetPosition = targetTransform.position;
            }

            public void DisableArrow()
            {
                if (IsTargetReached)
                    return;

                IsTargetReached = true;

                if(arrowTransform != null) arrowTransform.DOScale(0.0f, 0.4f).SetEasing(Ease.Type.SineOut).OnComplete(delegate
                {
                    arrowTransform.gameObject.SetActive(false);
                });
            }
        }
    }
}