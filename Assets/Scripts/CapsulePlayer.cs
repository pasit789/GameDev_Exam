using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pasit
{
    public class CapsulePlayer : MonoBehaviour
    {
        public float speed = 7f;
        public float gravity = -9.81f;
        private CharacterController controller;
        private Vector3 velocity;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            float x = 0f;
            float z = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
            }
#else
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
#endif

            Vector3 move = (transform.right * x + transform.forward * z).normalized;
            if (controller != null)
            {
                controller.Move(move * speed * Time.deltaTime);

                if (controller.isGrounded && velocity.y < 0)
                {
                    velocity.y = -2f;
                }

                velocity.y += gravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }

            // Falling out of map (Q.3 / Q.5 Losing condition: Fall from map)
            if (transform.position.y < -8f)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOverFromFall();
                }
            }
        }
    }
}
