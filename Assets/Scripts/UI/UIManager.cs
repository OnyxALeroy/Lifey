using System.IO.Pipes;
using UnityEngine;

namespace Lifey
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PlacableItemButton[] placableItemButtons = new PlacableItemButton[5];

        // ----------------------------------------------------------------------------------------

        // Singleton Pattern
        public static UIManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }
}
