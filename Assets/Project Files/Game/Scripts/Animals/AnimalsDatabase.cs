using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Animals Database", menuName = "Data/Animals/Animals Database")]
    public class AnimalsDatabase : ScriptableObject
    {
        [SerializeField] Animal[] animals;
        public Animal[] Animals => animals;

        [SerializeField] GameObject[] visitorPrefabs;
        public GameObject[] VisitorPrefabs => visitorPrefabs;

        public void Init()
        {
            for (int i = 0; i < animals.Length; i++)
            {
                animals[i].Init();
            }
        }

        public void Unload()
        {
            for (int i = 0; i < animals.Length; i++)
            {
                animals[i].Unload();
            }
        }
    }
}