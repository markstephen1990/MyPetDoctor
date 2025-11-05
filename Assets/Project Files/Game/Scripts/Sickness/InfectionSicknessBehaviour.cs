using UnityEngine;

namespace Watermelon
{
    public class InfectionSicknessBehaviour : SicknessBehaviour
    {
        [SerializeField] ParticleSystem sicknessParticleSystem;
        [SerializeField] Material sicknessMaterial;

        public override void Activate(AnimalBehaviour animalBehaviour)
        {
            base.Activate(animalBehaviour);

            // Get skinned mesh renderer and activate material
            animalBehaviour.SetMaterial(sicknessMaterial);

            // Activate particle
            sicknessParticleSystem.Play();
        }

        public override void DisableSickness()
        {
            animalBehaviour.ResetMaterial();

            // Stop particle
            sicknessParticleSystem.Stop();

            // Return sickness behaviour to pool
            transform.SetParent(null);
            gameObject.SetActive(false);

            animalBehaviour = null;
        }
    }
}