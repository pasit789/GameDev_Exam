using UnityEngine;

namespace Pasit
{
    public class Item : MonoBehaviour
    {
        public enum ItemType { AddTime, ReduceTime, AddScore, ReduceScore }
        public ItemType type;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CollectItem();
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
