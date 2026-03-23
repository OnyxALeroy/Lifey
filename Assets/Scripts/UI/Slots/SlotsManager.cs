using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lifey;
using Lifey.Attributes;

namespace Lifey.UI
{
    public class SlotsManager : MonoBehaviour
    {
        [Header("Placable Item Slots")]
        [SerializeField] private int placableItemSlotAmount = 5;
        [SerializeField] private GameObject placableItemSlotPrefab;
        [SerializeField] private GameObject placableItemSlotsHolder;
        [SerializeField, ReadOnly] private List<PlacableItemSlot> placableItemSlots = new List<PlacableItemSlot>();

        public void Initialize()
        {
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            while (!BlockDatabase.IsReady)
            {
                yield return null;
            }

            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            for (int i = 0; i < placableItemSlotAmount; i++)
            {
                GameObject slotInstance = Instantiate(placableItemSlotPrefab, placableItemSlotsHolder.transform);
                PlacableItemSlot slot = slotInstance.GetComponent<PlacableItemSlot>();
                slot.Initialize(this, i + 1);

                slot.SetBlock(BlockDatabase.GetBlock(i + 1));
                placableItemSlots.Add(slot);
            }
        }

        // ----------------------------------------------------------------------------------------

        public void OnPlacableItemSlotSelected(int index)
        {
            for (int i = 0; i < placableItemSlotAmount; i++)
            {
                placableItemSlots[i].SetActive(i + 1 == index);
            }
        }
    }
}
