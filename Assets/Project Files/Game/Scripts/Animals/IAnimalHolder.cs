namespace Watermelon
{
    public interface IAnimalHolder
    {
        public AnimalBehaviour AnimalBehaviour { get; }

        public void OnAnimalPicked(AnimalBehaviour animalBehaviour);
    }
}