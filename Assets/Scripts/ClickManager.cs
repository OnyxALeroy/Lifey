using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        // 1. Make sure we actually have a mouse connected/available
        if (Mouse.current != null)
        {
            // 2. Check for the left mouse button press this exact frame
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 3. Check if the pointer is NOT over a UI element
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // Trigger your non-UI stuff here!
                    Debug.Log("Clicked on the game world / empty space!");
                }
                else
                {
                    // The click was consumed by the UI
                    Debug.Log("Clicked on a UI element.");
                }
            }
        }
    }
}
