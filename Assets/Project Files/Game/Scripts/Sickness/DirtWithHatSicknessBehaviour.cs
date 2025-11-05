using UnityEngine;

namespace Watermelon
{
    public class DirtWithHatSicknessBehaviour : SicknessBehaviour
    {
        [SerializeField] ParticleSystem sicknessParticleSystem;
        [SerializeField] Material sicknessMaterial;

        [Space]
        [SerializeField] Renderer hatRenderer;

        private GameObject hatObject;

        private Vector3 hatPosition;
        private Quaternion hatRotation;
        private Vector3 hatScale;

        public override void Activate(AnimalBehaviour animalBehaviour)
        {
            base.Activate(animalBehaviour);

            hatObject = hatRenderer.gameObject;

            hatPosition = hatObject.transform.localPosition;
            hatRotation = hatObject.transform.localRotation;
            hatScale = hatObject.transform.localScale;

            // Get skinned mesh renderer and activate material
            animalBehaviour.SetMaterial(sicknessMaterial);

            // Activate particle
            sicknessParticleSystem.Play();

            // Place hat
            hatObject.SetActive(true);
            hatObject.transform.SetParent(animalBehaviour.transform);
            hatObject.transform.localPosition = hatPosition;
            hatObject.transform.localRotation = hatRotation;
            hatObject.transform.localScale = hatScale;
        }

        public override void DisableSickness()
        {
            animalBehaviour.ResetMaterial();

            // Stop particle
            sicknessParticleSystem.Stop();

            // Return sickness behaviour to pool
            hatObject.SetActive(false);
            hatObject.transform.SetParent(transform);
            hatObject.transform.localPosition = hatPosition;
            hatObject.transform.localRotation = hatRotation;
            hatObject.transform.localScale = hatScale;

            transform.SetParent(null);
            gameObject.SetActive(false);

            animalBehaviour = null;
        }

        public override void SetOutlineState(bool state)
        {
            Material material = hatRenderer.material;
            if (state)
            {
                material.EnableKeyword("OUUTLINE_ON");
            }
            else
            {
                material.DisableKeyword("OUUTLINE_ON");
            }

            material.SetShaderPassEnabled("SRPDefaultUnlit", state);
        }
    }
}