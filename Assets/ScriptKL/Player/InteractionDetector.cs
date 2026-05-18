using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; //Closet Interactable
    public GameObject interactionIcon;
    public GameObject interactionButton;
    void Start()
    {
        interactionIcon.SetActive(false);
        interactionButton.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact();
        }

    }

    
    public void PerformInteraction()
    {

        interactableInRange.Interact();        
    }
    
    private void OnTriggerStay2D(Collider2D collision) 
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
            interactionButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange )
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
            interactionButton.SetActive(false);
        }
    }
}
