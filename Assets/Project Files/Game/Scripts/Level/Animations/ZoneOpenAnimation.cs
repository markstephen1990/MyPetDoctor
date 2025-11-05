using System.Collections;
using System.Linq;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class ZoneOpenAnimation : BaseZoneOpenAnimation
    {
        [Header("Materials")]
        [SerializeField] Material environmentMaterial;
        [SerializeField] Material environmentDisabledMaterial;

        [Header("Refs")]
        [SerializeField] MeshRenderer[] floorMeshRenderers;
        [SerializeField] MeshRenderer[] wallsMeshRenderers;

        [Header("Active Environment")]
        [SerializeField] GameObject activeEnvironmentObject;
        [SerializeField] GameObject[] activeObjects;

        [Header("Disabled Environment")]
        [SerializeField] GameObject disabledEnvironmentObject;
        [SerializeField] GameObject[] disabledObjects;

        public override void OnZoneInitialised(Zone zone)
        {
            this.zone = zone;

            // Check if zone is opened
            if (zone.IsOpened)
            {
                // Activate floor
                for (int i = 0; i < floorMeshRenderers.Length; i++)
                {
                    floorMeshRenderers[i].material = environmentMaterial;
                }

                // Activate walls
                for (int i = 0; i < wallsMeshRenderers.Length; i++)
                {
                    wallsMeshRenderers[i].material = environmentMaterial;
                }

                // Activate environment
                activeEnvironmentObject.SetActive(true);
                disabledEnvironmentObject.SetActive(false);
            }
            // Zone isn't opened
            else
            {
                // Disable floor
                for (int i = 0; i < floorMeshRenderers.Length; i++)
                {
                    floorMeshRenderers[i].material = environmentDisabledMaterial;
                }

                // Disable walls
                for (int i = 0; i < wallsMeshRenderers.Length; i++)
                {
                    wallsMeshRenderers[i].material = environmentDisabledMaterial;
                }

                // Activate disabled environment
                disabledEnvironmentObject.SetActive(true);
                activeEnvironmentObject.SetActive(false);
            }
        }

        private IEnumerator DisableObjectsCoroutine()
        {
            Vector3[] disableObjectsScale = new Vector3[disabledObjects.Length];
            for (int i = 0; i < disabledObjects.Length; i++)
            {
                disableObjectsScale[i] = disabledObjects[i].transform.localScale;
            }

            float disableTime = 0;
            float easeTime = 0;
            Ease.IEasingFunction sineInEasing = Ease.GetFunction(Ease.Type.SineIn);
            Vector3 zeroScale = Vector3.zero;

            while (true)
            {
                easeTime = sineInEasing.Interpolate(disableTime);

                for (int i = 0; i < disabledObjects.Length; i++)
                {
                    disabledObjects[i].transform.localScale = Vector3.Lerp(disableObjectsScale[i], zeroScale, easeTime);
                }

                disableTime += Time.deltaTime / 0.3f;

                if (disableTime >= 1.0f)
                    break;

                yield return null;
            }

            for (int i = 0; i < disabledObjects.Length; i++)
            {
                disabledObjects[i].SetActive(false);
            }
        }

        private IEnumerator ActivateObjectsCoroutine()
        {
            Vector3 startScale = Vector3.zero;
            Vector3[] defaultScales = new Vector3[activeObjects.Length];
            float[] durations = new float[activeObjects.Length];
            float[] times = new float[activeObjects.Length];
            Ease.IEasingFunction backOutEasing = Ease.GetFunction(Ease.Type.BackOut);

            for (int i = 0; i < activeObjects.Length; i++)
            {
                defaultScales[i] = activeObjects[i].transform.localScale;

                durations[i] = Random.Range(0.5f, 1.0f);

                activeObjects[i].transform.localScale = Vector3.zero;
            }

            while (true)
            {
                bool allAnimationsCompleted = true;

                for (int i = 0; i < activeObjects.Length; i++)
                {
                    times[i] += Time.deltaTime / durations[i];

                    if (times[i] < 1.0f)
                    {
                        allAnimationsCompleted = false;

                        activeObjects[i].transform.localScale = Vector3.LerpUnclamped(startScale, defaultScales[i], backOutEasing.Interpolate(times[i]));
                    }
                }

                if (allAnimationsCompleted)
                    yield break;

                yield return null;
            }
        }

        private IEnumerator ZoneOpeninCoroutine()
        {
            // Enable floor mask
            for (int i = 0; i < floorMeshRenderers.Length; i++)
            {
                floorMeshRenderers[i].material = environmentMaterial;
            }

            // Enable walls
            for (int i = 0; i < wallsMeshRenderers.Length; i++)
            {
                wallsMeshRenderers[i].material = environmentMaterial;
            }

            // Activate environment
            activeEnvironmentObject.SetActive(true);
            disabledEnvironmentObject.SetActive(true);

            IEnumerator disableEnumerator = DisableObjectsCoroutine();
            IEnumerator activateEnumerator = ActivateObjectsCoroutine();

            while (disableEnumerator.MoveNext() | activateEnumerator.MoveNext())
            {
                yield return null;
            }

            yield return null;

            disabledEnvironmentObject.SetActive(false);

            yield return null;

            NavMeshController.RecalculateNavMesh(delegate { });
        }

        public override void OnZoneOpened()
        {
            StartCoroutine(ZoneOpeninCoroutine());
        }

        private void Reset()
        {
            environmentMaterial = RuntimeEditorUtils.GetAssetByName<Material>("Environment");
            environmentDisabledMaterial = RuntimeEditorUtils.GetAssetByName<Material>("Environment Grayscale");
        }

        public override void OnSceneSaving()
        {
            Zone zone = GetComponent<Zone>();
            if (zone != null)
            {
                Transform floorContainer = zone.FloorContainer;
                if(floorContainer != null)
                {
                    floorMeshRenderers = floorContainer.GetComponentsInChildren<MeshRenderer>();
                }
                else
                {
                    Debug.LogError(string.Format("{0}: Floor container is missing", gameObject.name));
                }

                Transform wallsContainer = zone.WallsContainer;
                if(wallsContainer != null)
                {
                    wallsMeshRenderers = wallsContainer.GetComponentsInChildren<MeshRenderer>().Where(x => x.gameObject.layer == PhysicsHelper.LAYER_WALLS).ToArray();
                }
                else
                {
                    Debug.LogError(string.Format("{0}: Walls container is missing", gameObject.name));
                }

                RuntimeEditorUtils.SetDirty(this);
            }
        }
    }
}