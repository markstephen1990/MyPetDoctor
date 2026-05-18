namespace FernGames
{
    public interface IAnimalHolder
    {
        public AnimalBehaviour AnimalBehaviour { get; }

        public void OnAnimalPicked(AnimalBehaviour animalBehaviour);
    }
}