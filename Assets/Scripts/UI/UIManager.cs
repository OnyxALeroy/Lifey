using UnityEngine;

namespace Lifey.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Sub-managers")]
        [SerializeField] private SlotsManager slotsManager;

        // ----------------------------------------------------------------------------------------

        // Singleton Pattern
        public static UIManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else Destroy(gameObject);
        }

        private void Initialize()
        {
            slotsManager.Initialize();
        }
    }
}
