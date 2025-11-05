using UnityEngine;

namespace Watermelon
{
    public interface IAnimalCarrying
    {
        public Transform Transform { get; }

        public bool IsAnimalCarrying();
        public void CarryAnimal(AnimalBehaviour animalBehaviour);

        public AnimalBehaviour GetAnimal();
        public AnimalBehaviour GetAnimal(Animal.Type[] allowedAnimalTypes);

        public void RemoveAnimal(AnimalBehaviour animalBehaviour);
        public bool IsAnimalPickupAllowed();
    }

    public interface IItemCarrying
    {
        public Transform Transform { get; }

        public bool HasItem(Item.Type itemType);
        public ItemStorageCase AddItem(Item.Type itemType);
        public void RemoveItem(Item.Type itemType);
        public bool HasFreeSpace();
    }
}