using UnityEngine;

namespace Watermelon
{
    public abstract class TableCallbackReciever : MonoBehaviour
    {
        protected TableBehaviour tableBehaviour;

        public virtual void Initialise(TableBehaviour tableBehaviour)
        {
            this.tableBehaviour = tableBehaviour;
        }

        public virtual void OnAnimalCured(SimpleCallback animalCureCallback)
        {
            animalCureCallback?.Invoke();
        }

        public virtual void OnAnimalPlaced(AnimalBehaviour animalBehaviour)
        {

        }

        public virtual void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {

        }
    }
}