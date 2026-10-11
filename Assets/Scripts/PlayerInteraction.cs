using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pasit
{
    /// <summary>
    /// Handles item interactions for Stage 2 Humanoid Player.
    /// Detects nearby IInteractable objects within range and triggers interaction upon pressing 'E'.
    /// Also checks if player falls off the level to trigger Game Over.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactRange = 2.8f;
        public LayerMask interactLayer = ~0; // All layers by default

        [Header("UI Reference")]
        public TextMeshProUGUI promptText;
        public GameObject promptUI;

        private IInteractable currentInteractable;

        private void Start()
        {
            // Auto-find prompt text if not assigned
            if (promptText == null)
            {
                var allTMPs = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var tmp in allTMPs)
                {
                    if (tmp.gameObject.name.Contains("Prompt") || tmp.gameObject.name.Contains("Interact"))
                    {
                        promptText = tmp;
                        if (promptUI == null) promptUI = tmp.transform.parent.gameObject;
                        break;
                    }
                }
            }

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
            else if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            FindClosestInteractable();
            HandleInteractionInput();
            CheckFallDeath();
        }

        private void FindClosestInteractable()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, interactLayer);
            IInteractable closest = null;
            float closestDist = float.MaxValue;

            foreach (var col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null) interactable = col.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }

            currentInteractable = closest;
            UpdatePromptUI();
        }

        private void UpdatePromptUI()
        {
            if (currentInteractable != null)
            {
                string text = currentInteractable.GetInteractPrompt();
                if (promptText != null)
                {
                    promptText.text = text;
                    promptText.gameObject.SetActive(true);
                }
                if (promptUI != null)
                {
                    promptUI.SetActive(true);
                }
            }
            else
            {
                if (promptText != null) promptText.gameObject.SetActive(false);
                if (promptUI != null) promptUI.SetActive(false);
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;

            bool interactPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
            {
                interactPressed = true;
            }
            else if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame))
            {
                interactPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
            {
                interactPressed = true;
            }
#endif

            if (interactPressed)
            {
                IInteractable target = currentInteractable;
                currentInteractable = null;
                UpdatePromptUI();
                target.Interact(gameObject);
            }
        }

        private void CheckFallDeath()
        {
            if (transform.position.y < -8f)
            {
                if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                {
                    GameManager.Instance.GameOverFromFall();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
