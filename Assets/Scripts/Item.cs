using UnityEngine;

namespace Pasit
{
    public class Item : MonoBehaviour, IInteractable
    {
        public enum ItemType { AddTime, ReduceTime, AddScore, ReduceScore }
        public ItemType type;

        [Header("Interaction System (Stage 2)")]
        [Tooltip("If true, requires manual interaction (e.g. Press E) instead of collision check.")]
        public bool requireInteraction = false;

        private void Update()
        {
            if (type == ItemType.AddScore || type == ItemType.AddTime)
            {
                transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Stage 2 requirement: Use interaction system instead of collision check
            if (requireInteraction) return;

            if (other.CompareTag("Player"))
            {
                CollectItem();
            }
        }

        /// <summary>
        /// Interaction method called by PlayerInteraction system (Press E).
        /// </summary>
        public void Interact(GameObject actor)
        {
            CollectItem();
        }

        public string GetInteractPrompt()
        {
            switch (type)
            {
                case ItemType.AddTime:
                    return "[E] COLLECT (+3s TIME)";
                case ItemType.ReduceTime:
                    return "[E] INTERACT (-5s TIME HAZARD)";
                case ItemType.AddScore:
                    return "[E] COLLECT (+500 SCORE)";
                case ItemType.ReduceScore:
                    return "[E] INTERACT (-500 SCORE HAZARD)";
                default:
                    return "[E] INTERACT";
            }
        }

        public void CollectItem()
        {
            switch (type)
            {
                case ItemType.AddTime:
                    if (GameManager.Instance != null) GameManager.Instance.AddTime(3f);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayItemSFX();
                    break;
                case ItemType.ReduceTime:
                    if (GameManager.Instance != null) GameManager.Instance.AddTime(-5f);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayObstacleSFX();
                    break;
                case ItemType.AddScore:
                    if (GameManager.Instance != null) GameManager.Instance.AddScore(500);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayItemSFX();
                    break;
                case ItemType.ReduceScore:
                    if (GameManager.Instance != null) GameManager.Instance.AddScore(-500);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayObstacleSFX();
                    break;
            }
            Destroy(gameObject);
        }
    }
}
