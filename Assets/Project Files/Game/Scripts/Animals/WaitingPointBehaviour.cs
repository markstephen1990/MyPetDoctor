using UnityEngine;

namespace Watermelon
{
    public class WaitingPointBehaviour : MonoBehaviour, IAnimalHolder
    {
        private AnimalBehaviour animalBehaviour;
        public AnimalBehaviour AnimalBehaviour => animalBehaviour;

        private bool isAnimalPlaced;
        public bool IsAnimalPlaced => isAnimalPlaced;

        private bool isPointBusy;
        public bool IsPointBusy => isPointBusy;

        public void MarkAsBusy()
        {
            isPointBusy = true;
        }

        public void Place(AnimalBehaviour animalBehaviour)
        {
            this.animalBehaviour = animalBehaviour;

            animalBehaviour.transform.SetParent(null);
            animalBehaviour.transform.DOBezierMove(transform.position, Random.Range(3, 5), 0, 0, 0.5f).SetEasing(Ease.Type.SineIn);
            animalBehaviour.transform.DORotate(transform.rotation, 0.5f);

            isAnimalPlaced = true;
        }

        public void ResetPoint()
        {
            isPointBusy = false;
            isAnimalPlaced = false;

            animalBehaviour = null;
        }

        public void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (this.animalBehaviour == animalBehaviour)
            {
                ResetPoint();
            }
        }
    }
}