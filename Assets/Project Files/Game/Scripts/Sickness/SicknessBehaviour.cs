using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SicknessBehaviour : MonoBehaviour
    {
        [SerializeField] float extraCarryingHeight;
        public float ExtraCarryingHeight => extraCarryingHeight;

        protected AnimalBehaviour animalBehaviour;

        public virtual void Activate(AnimalBehaviour animalBehaviour)
        {
            this.animalBehaviour = animalBehaviour;
        }

        public virtual void DisableSickness()
        {

        }

        public void PlayCureParticle()
        {
            // Play sickness particle
            if(animalBehaviour != null && animalBehaviour.ActiveSickness != null)
                ParticlesController.PlayParticle(animalBehaviour.ActiveSickness.ParticleHash).SetPosition(transform.position);
        }

        public virtual void SetOutlineState(bool state)
        {

        }
    }
}