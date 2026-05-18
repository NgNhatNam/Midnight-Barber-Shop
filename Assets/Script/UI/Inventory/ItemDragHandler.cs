using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    //private Health playerHealth;

    //private Item itemData;

    private InventoryController inventoryController;

    [SerializeField]
    float minDropDistance = 2f;
    [SerializeField]
    float maxDropDistance = 3f;

    void Start()
    {
        inventoryController = InventoryController.Instance;
        canvasGroup = GetComponent<CanvasGroup>();
        //playerHealth = FindAnyObjectByType<Health>();
        //itemData = GetComponent<Item>();
    }

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ItemUI.Instance != null)
        {
            ItemUI.Instance.Hide();
        }


        if (eventData.button != PointerEventData.InputButton.Left) return;


        // --- PHẦN THÊM VÀO ĐỂ HIGHLIGHT KHI DRAG ---
        Slot currentSlot = GetComponentInParent<Slot>();
        if (currentSlot != null)
        {
            ToolbarController toolbar = FindAnyObjectByType<ToolbarController>();
            if (toolbar != null)
            {
                toolbar.SelectSlot(currentSlot);
            }
        }

        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
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
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Tìm Slot bằng RaycastAll (Xuyên qua các lớp UI con)
        Slot dropSlot = FindSlotUnderPointer(eventData);
        Slot originalSlot = originalParent.GetComponent<Slot>();

        // Xử lý các kịch bản Drop
        if (dropSlot != null)
        {
            if (dropSlot == originalSlot)
            {
                // Thả tại chỗ: Chỉ cần về vị trí cũ
                MoveToSlot(originalSlot);
            }
            else if (dropSlot.currentItem != null)
            {
                // Ô có vật phẩm: Kiểm tra xem có Stack được không
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem != null && targetItem != null && draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                    return; // Kết thúc sớm vì item đã bị xóa
                }
                else
                {
                    // Không cùng ID: Đổi chỗ Swap
                    SwapItems(originalSlot, dropSlot);
                }
            }
            else
            {
                // Thả vào ô trống
                originalSlot.currentItem = null;
                MoveToSlot(dropSlot);
            }

            // Cập nhật Toolbar Highlight (Dùng Instance để tối ưu hiệu suất)
            if (ToolbarController.Instance != null)
                ToolbarController.Instance.SelectSlot(dropSlot);
        }
        else
        {
            // 4. Thả ra ngoài: Kiểm tra xem có rơi vật phẩm ra đất không
            if (!IsWithInInvetory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                MoveToSlot(originalSlot);
            }
        }
    }


    private Slot FindSlotUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            // BỎ QUA chính cái Item đang bị kéo (để nó không tự chặn mình)
            if (result.gameObject == gameObject) continue;

            // Tìm Slot ở object chạm trúng hoặc cha của nó
            Slot slot = result.gameObject.GetComponentInParent<Slot>();
            if (slot != null) return slot;
        }
        return null;
    }

    private void MoveToSlot(Slot targetSlot)
    {
        transform.SetParent(targetSlot.transform);
        transform.localScale = Vector3.one; // Đảm bảo Scale chuẩn 1:1
        targetSlot.currentItem = gameObject;
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private void SwapItems(Slot originalSlot, Slot dropSlot)
    {
        GameObject itemInDropSlot = dropSlot.currentItem;

        // Đưa item đang có ở ô đích về ô cũ
        itemInDropSlot.transform.SetParent(originalSlot.transform);
        itemInDropSlot.transform.localScale = Vector3.one;
        originalSlot.currentItem = itemInDropSlot;
        itemInDropSlot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Đưa item đang kéo vào ô đích mới
        MoveToSlot(dropSlot);
    }

    bool IsWithInInvetory(Vector2 mousePosition)
    {
        // Xác định xem Item này vốn dĩ xuất phát từ đâu

        ToolbarController toolbar = ToolbarController.Instance;
        if (toolbar == null) toolbar = FindAnyObjectByType<ToolbarController>();

        GameObject invPanel = InventoryController.Instance.inventoryPanel;
        RectTransform invRect = invPanel.GetComponent<RectTransform>();

        // Kiểm tra xem cái ô cũ (originalParent) có phải là con của Toolbar không
        bool isFromToolbar = false;
        if (toolbar != null)
        {
            // Kiểm tra xem originalParent có nằm trong các con của Toolbar không
            isFromToolbar = originalParent.IsChildOf(toolbar.transform);
        }

        if (isFromToolbar)
        {
            // Nếu kéo từ Toolbar, vùng an toàn là Toolbar Rect
            RectTransform toolbarRect = toolbar.GetComponent<RectTransform>();
            return RectTransformUtility.RectangleContainsScreenPoint(toolbarRect, mousePosition);
        }
        else
        {
            // Nếu kéo từ Inventory chính, vùng an toàn là Inventory Rect
            return RectTransformUtility.RectangleContainsScreenPoint(invRect, mousePosition);
        }
    }

    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        if (item == null) return;

        // Xác định vị trí drop 
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        Vector2 dropPosition = (Vector2)playerTransform.position + Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);

        // Tạo vật phẩm ngoài thế giới (Nên dùng Prefab vật phẩm rơi thay vì copy chính cái UI)
        GameObject droppedObj = Instantiate(gameObject, dropPosition, Quaternion.identity);

        // Xóa các Component UI không cần thiết ở cái item ngoài đất để tránh lỗi
        Destroy(droppedObj.GetComponent<ItemDragHandler>());
        Destroy(droppedObj.GetComponent<CanvasGroup>());

        Item droppedItemScript = droppedObj.GetComponent<Item>();
        droppedItemScript.quantity = 1; // Vật phẩm rơi ra luôn là 1

        // Xử lý item còn lại trong Inventory
        if (item.quantity > 1)
        {
            // Nếu còn nhiều, chỉ trừ 1 và trả cái UI này về ô cũ
            item.RemoveFromStack(1);
            transform.SetParent(originalParent);
            transform.localScale = Vector3.one;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            // Nếu chỉ còn 1, xóa hẳn cái UI này trong Inventory
            originalSlot.currentItem = null;
            Destroy(gameObject);
        }

        InventoryController.Instance.RebuildItemCounts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Nếu đang dragging thì không nhận Click
        if (eventData.dragging) return;

        Item item = GetComponent<Item>();

        // CHUỘT PHẢI: Tách Stack
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Nếu chỉ có 1 cái thì không tách, mà hiện Menu luôn hoặc không làm gì
            if (item != null && item.quantity > 1)
            {
                SplitStack();
            }
            else
            {
                // Nếu chỉ có 1 cái, chuột phải cũng hiện Menu 
                ItemUI.Instance.Show(item, eventData.position);
            }
        }
        // CHUỘT TRÁI (Hoặc Tap trên Mobile): Hiện Menu Use/Sell và Highlight Slot
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Highlight Slot
            Slot parentSlot = GetComponentInParent<Slot>();
            if (parentSlot != null)
            {
                ToolbarController toolbar = FindAnyObjectByType<ToolbarController>();
                if (toolbar != null) toolbar.SelectSlot(parentSlot);
            }

            // Hiện bảng Menu
            if (ItemUI.Instance != null)
            {
                ItemUI.Instance.Show(item, eventData.position);
            }
        }
    }

    public void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);

        GameObject newItem = item.CloneItem(splitAmount);

        if (inventoryController == null || newItem == null) return;

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.transform.localScale = Vector3.one; // THÊM DÒNG NÀY
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }
        
        // No Empty slot - return to stack
        item.AddToStack(splitAmount);
        Destroy(newItem);
    } 
        

    void RemoveFromInventory()
    {
       
        Destroy(gameObject); // xoá UI item
    }
}


