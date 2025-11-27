using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Health playerHealth;
    
    private Item itemData;


    [SerializeField]
     float minDropDistance = 2f;
    [SerializeField]
     float maxDropDistance = 3f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        playerHealth = FindAnyObjectByType<Health>();
        itemData = GetComponent<Item>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ItemUI.Instance.Show(this, eventData.position);
        }
    }

    public void UseItem()
    {
        Debug.Log("Dùng item: " + itemData.ID);

        playerHealth.Heal(itemData.amountHP);
        playerHealth.HealMN(itemData.amountMN);
        playerHealth.IncreaseStress(itemData.amountST);

        Debug.Log("Máu: " + playerHealth.HP + "Mana: " + playerHealth.MN + "Stress: " + playerHealth.Stress );
        RemoveFromInventory();
    }

    public void SellItem()
    {
        Debug.Log("Bán item: " + itemData.ID + " giá " + itemData.price);

        playerHealth.AddGold(itemData.price);

        RemoveFromInventory();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Debug.Log("Begin Drag");

        originalParent = transform.parent; //Save OG parent

        transform.SetParent(transform.root); // Above other canvas

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Semi-transparent during drag

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Debug.Log("Dragging");
        transform.position = eventData.position; // Follow the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Debug.Log("End Drag");


        canvasGroup.blocksRaycasts = true; // Enables raycasts
        canvasGroup.alpha = 1f; //No longer transparent

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //Slot where item dropped

        if (dropSlot == null)
        {   
            GameObject dropItem = eventData.pointerEnter;
            if(dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }
        
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null) {

            if (dropSlot.currentItem != null) 
            { 
                //Slot has an item - swap item

                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            else
            {
                originalSlot.currentItem = null;
            }
            
            // Move item into drop slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
        }
        else
        {
            // Nếu như drop ngoài inventory
            if (!IsWithInInvetory(eventData.position))
            {
                // drop item 
                DropItem(originalSlot);
            }

            else { 
                //Snap back to og slot
                transform.SetParent(originalParent);
            }
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Zero
  
    } 

    bool IsWithInInvetory(Vector2 mousePosition)
    {
        RectTransform invetoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(invetoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot)
    {
        originalSlot.currentItem = null;

        //Find player
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null) {
            Debug.LogError("Missing 'Player' tag");
            return;
        }
        
        //Random Drop position
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;
        
        //Instantiate drop item
        Instantiate(gameObject, dropPosition, Quaternion.identity);

        //Destroy the UI one
        Destroy(gameObject);
    }

    void RemoveFromInventory()
    {
        /*
        Slot slot = transform.parent.GetComponent<Slot>();
        if (slot != null)
            slot.currentItem = null;
        */
        Destroy(gameObject); // xoá UI item
    }
}
