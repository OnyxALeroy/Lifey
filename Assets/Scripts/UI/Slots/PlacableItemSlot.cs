using System;
using UnityEngine;
using UnityEngine.UI;
using Lifey.Attributes;
using UnityEngine.InputSystem;

namespace Lifey.UI
{
    [RequireComponent(typeof(RectTransform)), RequireComponent(typeof(Image)), RequireComponent(typeof(Button))]
    public class PlacableItemSlot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite unactiveSprite;

        [Header("References")]
        [SerializeField] private BlockData defaultBlockData;
        [SerializeField] private BlockData blockData;

        [Space(10)]

        [Header("Information")]
        [SerializeField, ReadOnly] private SlotsManager slotsManager;
        [SerializeField, ReadOnly] private int index = -1;
        [SerializeField, ReadOnly] private Key keybind;
        [SerializeField, ReadOnly] private Button button;

        private readonly Key[] numberKeys = {
                Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
                Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
            };

        public void Initialize(SlotsManager sm, int i)
        {
            button = GetComponent<Button>();
            slotsManager = sm;
            index = i;
            blockData = defaultBlockData;
            SetActive(false);

            // Map the integer to the corresponding top-row number key
            // KeyCode.Alpha0 is 48, Alpha1 is 49, etc. So we can do a simple cast.
            if (i >= 0 && i <= 9)
            {
                keybind = numberKeys[i];
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    $"Index {i} is out of range for a single number keybind."
                );
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnPressed);
            // button.onClick.AddListener(() => OnPressed());
        }

        // ----------------------------------------------------------------------------------------

        public void SetActive(bool active)
        {
            isActive = active;
            if (active)
            {
                gameObject.GetComponent<Image>().sprite = activeSprite;
            }
            else
            {
                gameObject.GetComponent<Image>().sprite = unactiveSprite;
            }
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current[keybind].wasPressedThisFrame)
            {
                OnPressed();
            }
        }

        public void OnPressed()
        {
            Debug.Log($"Action triggered for index {index}! (Keybind: {keybind})");
            slotsManager.OnPlacableItemSlotSelected(index);
        }
    }
}
