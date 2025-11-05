using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Sickness
    {
        [SerializeField] SicknessType sicknessType;
        public SicknessType SicknessType => sicknessType;

        [SerializeField] Item.Type requiredItem;
        public Item.Type RequiredItem => requiredItem;

        [SerializeField] AnimalBehaviourCase[] sicknessBehavioursOverride;

        [SerializeField] GameObject sicknessParticleObject;

        private int particleHash;
        public int ParticleHash => particleHash;

        public void Init()
        {
            // Register particle
            particleHash = ParticlesController.RegisterParticle(sicknessType.ToString() + requiredItem.ToString(), sicknessParticleObject);
        }

        public void Unload()
        {

        }

        public SicknessBehaviour GetSicknessBehaviour(Animal.Type animalType)
        {
            for (int i = 0; i < sicknessBehavioursOverride.Length; i++)
            {
                if (sicknessBehavioursOverride[i].AnimalType == animalType)
                {
                    GameObject sicknessObject = GameObject.Instantiate(sicknessBehavioursOverride[i].SicknessBehaviour.GetRandomItem().gameObject);

                    return sicknessObject.GetComponent<SicknessBehaviour>();
                }
            }

            Debug.LogError("Sickness with type " + sicknessType + " for " + animalType + " is missing!");

            return null;
        }

        [System.Serializable]
        public class AnimalBehaviourCase
        {
            [SerializeField] Animal.Type animalType;
            public Animal.Type AnimalType => animalType;

            [SerializeField] SicknessBehaviour[] sicknessBehaviours;
            public SicknessBehaviour[] SicknessBehaviour => sicknessBehaviours;
        }
    }
}