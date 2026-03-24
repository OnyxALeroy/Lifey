using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Lifey.UI;

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

        private void Start()
        {
            mainCamera ??= Camera.main;
        }

        void Update()
        {
            if (Mouse.current != null)
            {
                // Left Click -> Break Block
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        PerformBlockRaycast(isBreaking: true);
                    }
                }

                // Right Click -> Place Block
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        PerformBlockRaycast(isBreaking: false);
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
