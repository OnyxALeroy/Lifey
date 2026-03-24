using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Lifey.UI;
using Lifey.Attributes;

namespace Lifey
{
    public class ClickManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private UIManager uIManager;

        [Space(10)]

        [Header("Raycast Settings")]
        [SerializeField] private LayerMask blockLayerMask;
        [SerializeField] private float maxRayDistance = 100f;

        [Space(10)]

        [Header("Interaction Settings")]
        [Tooltip("How fast blocks are placed/broken when holding the mouse button down (in seconds).")]
        [SerializeField] public float interactionCooldown = 0.2f;
        [SerializeField, ReadOnly] private float lastInteractionTime = 0f;

        private void Start()
        {
            mainCamera ??= Camera.main;
        }

        void Update()
        {
            if (Mouse.current != null)
            {
                if (Mouse.current != null)
                {
                    // We check if the pointer is over the UI first so we don't process anything if we're clicking menus
                    if (EventSystem.current.IsPointerOverGameObject()) return;

                    bool leftClickThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
                    bool leftHeld = Mouse.current.leftButton.isPressed;

                    bool rightClickThisFrame = Mouse.current.rightButton.wasPressedThisFrame;
                    bool rightHeld = Mouse.current.rightButton.isPressed;

                    // LEFT CLICK (Break) - Triggers instantly on click, OR repeatedly while held based on the cooldown
                    if (leftClickThisFrame || (leftHeld && Time.time >= lastInteractionTime + interactionCooldown))
                    {
                        PerformBlockRaycast(isBreaking: true);
                        lastInteractionTime = Time.time; // Reset the timer
                    }
                    // RIGHT CLICK (Place) - Triggers instantly on click, OR repeatedly while held based on the cooldown
                    else if (rightClickThisFrame || (rightHeld && Time.time >= lastInteractionTime + interactionCooldown))
                    {
                        PerformBlockRaycast(isBreaking: false);
                        lastInteractionTime = Time.time; // Reset the timer
                    }
                }
            }
        }

        private void PerformBlockRaycast(bool isBreaking)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            BlockData blockData = uIManager.GetSelectedBlock();

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, blockLayerMask))
            {
                if (isBreaking)
                {
                    // Nudge INWARDS to get the block we hit
                    Vector3 pointInsideBlock = hit.point - (hit.normal * 0.01f);
                    Vector3Int clickedBlockPos = new Vector3Int(
                        Mathf.FloorToInt(pointInsideBlock.x),
                        Mathf.FloorToInt(pointInsideBlock.y),
                        Mathf.FloorToInt(pointInsideBlock.z)
                    );

                    // Break the block (Set to Air / 0)
                    WorldManager.Instance.SetBlockAtGlobalPosition(clickedBlockPos, 0);
                }
                else if (blockData != null)
                {
                    // Nudge OUTWARDS to get the empty space attached to the face
                    Vector3 pointOutsideBlock = hit.point + (hit.normal * 0.01f);
                    Vector3Int adjacentAirPos = new Vector3Int(
                        Mathf.FloorToInt(pointOutsideBlock.x),
                        Mathf.FloorToInt(pointOutsideBlock.y),
                        Mathf.FloorToInt(pointOutsideBlock.z)
                    );

                    // Place the currently selected block
                    WorldManager.Instance.SetBlockAtGlobalPosition(adjacentAirPos, blockData.blockID);
                }
            }
        }
    }
}
