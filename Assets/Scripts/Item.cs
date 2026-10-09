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
                    GameManager.Instance.AddTime(3f);
                    break;
                case ItemType.ReduceTime:
                    GameManager.Instance.AddTime(-5f);
                    break;
                case ItemType.AddScore:
                    GameManager.Instance.AddScore(500);
                    break;
                case ItemType.ReduceScore:
                    GameManager.Instance.AddScore(-500);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
