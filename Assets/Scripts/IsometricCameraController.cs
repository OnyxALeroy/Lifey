using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lifey
{
    public class IsometricCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float panSpeed = 15f;
        public float sprintMultiplier = 2f; // How much faster we go (2 = double speed)
        public float sprintTransitionSpeed = 5f; // Higher number = faster acceleration/deceleration

        [Header("Rotation Settings")]
        public Key rotateLeftKey = Key.Q; // Physical 'A' on AZERTY
        public Key rotateRightKey = Key.E;
        public float rotationDuration = 0.3f;

        [Tooltip("LayerMask for your blocks, so the raycast ignores things like particles or the player.")]
        public LayerMask blockLayerMask;
        public float maxRayDistance = 1000f;

        private bool isRotating = false;
        private Vector3 currentPivotPoint;
        private Vector3 rotationStartPosition;
        private bool hasValidPivot = false;

        private float currentSpeedMultiplier = 1f;

        // --------------------------------------------------------------------------------------------

        void Update()
        {
            if (Keyboard.current == null) return;

            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            if (isRotating) return;

            float h = 0f;
            float v = 0f;
            float y = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;

            if (Keyboard.current.spaceKey.isPressed) y += 1f;
            if (Keyboard.current.leftShiftKey.isPressed) y -= 1f;

            float targetMultiplier = Keyboard.current.leftCtrlKey.isPressed ? sprintMultiplier : 1f;
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetMultiplier, Time.deltaTime * sprintTransitionSpeed);

            if (h == 0 && v == 0 && y == 0) return;

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            Vector3 moveDirection = (forward * v + right * h + Vector3.up * y).normalized;
            transform.position += moveDirection * (panSpeed * currentSpeedMultiplier) * Time.deltaTime;
        }

        private void HandleRotation()
        {
            if (isRotating) return;

            if (Keyboard.current[rotateLeftKey].wasPressedThisFrame)
            {
                StartRotation(90f);
            }
            else if (Keyboard.current[rotateRightKey].wasPressedThisFrame)
            {
                StartRotation(-90f);
            }
        }

        private void StartRotation(float angle)
        {
            Vector3 pivotPoint;
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRayDistance, blockLayerMask))
            {
                pivotPoint = hit.point;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float distanceToGround;

                if (groundPlane.Raycast(ray, out distanceToGround))
                {
                    pivotPoint = ray.GetPoint(distanceToGround);
                }
                else
                {
                    hasValidPivot = false;
                    return;
                }
            }

            currentPivotPoint = pivotPoint;
            rotationStartPosition = transform.position;
            hasValidPivot = true;

            StartCoroutine(SmoothRotationRoutine(pivotPoint, angle));
        }

        private IEnumerator SmoothRotationRoutine(Vector3 pivotPoint, float targetAngle)
        {
            isRotating = true;
            float timeElapsed = 0f;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            Vector3 startOffset = startPosition - pivotPoint;

            while (timeElapsed < rotationDuration)
            {
                timeElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(timeElapsed / rotationDuration);

                t = t * t * (3f - 2f * t);

                float currentAngle = Mathf.Lerp(0, targetAngle, t);
                Quaternion currentYRotation = Quaternion.Euler(0, currentAngle, 0);

                transform.position = pivotPoint + (currentYRotation * startOffset);
                transform.rotation = currentYRotation * startRotation;

                yield return null;
            }

            Quaternion finalYRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.position = pivotPoint + (finalYRotation * startOffset);
            transform.rotation = finalYRotation * startRotation;

            isRotating = false;
        }

        private void OnDrawGizmos()
        {
            if (!hasValidPivot) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentPivotPoint, 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rotationStartPosition, currentPivotPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(currentPivotPoint, currentPivotPoint + Vector3.up * 200f);
            Gizmos.DrawLine(currentPivotPoint, currentPivotPoint + Vector3.down * 200f);
        }
    }
}
