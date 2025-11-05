using UnityEngine;

namespace Watermelon
{
    public class AnimalStorageCase
    {
        private int index;
        public int Index => index;

        private AnimalBehaviour animalBehaviour;
        public AnimalBehaviour AnimalBehaviour => animalBehaviour;

        private bool isPicked;
        public bool IsPicked => isPicked;

        private GameObject storageObject;
        public GameObject StorageObject => storageObject;

        public Transform Transform => animalBehaviour.transform;

        public AnimalStorageCase(AnimalBehaviour animalBehaviour, GameObject storageObject)
        {
            this.animalBehaviour = animalBehaviour;
            this.storageObject = storageObject;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void MarkAsPicked()
        {
            isPicked = true;
        }

        public void Reset()
        {
            storageObject.transform.SetParent(null);
            storageObject.SetActive(false);
        }
    }
}