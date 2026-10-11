using UnityEngine;

namespace Pasit
{
    /// <summary>
    /// Interface for interactable objects in the game.
    /// Used by PlayerInteraction to trigger interaction instead of simple collision.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject actor);
        string GetInteractPrompt();
    }
}
