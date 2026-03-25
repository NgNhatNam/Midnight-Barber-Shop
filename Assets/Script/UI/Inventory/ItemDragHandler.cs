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

    /*
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
        ItemUI.Instance.Hide();
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Debug.Log("Begin Drag");

        originalParent = transform.parent; //Save OG parent

        transform.SetParent(transform.root); // Above other canvas

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Semi-transparent during drag

    }*/

    public void OnBeginDrag(PointerEventData eventData)
    {
        // CỰC KỲ QUAN TRỌNG CHO MOBILE:
        // Nếu người chơi bắt đầu kéo, phải ẩn ngay cái bảng menu Use/Sell đi
        if (ItemUI.Instance != null)
        {
            ItemUI.Instance.Hide();
        }


        // Logic cũ của bạn
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
        // 1. Reset trạng thái UI ngay lập tức
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 2. Tìm Slot bằng RaycastAll (Xuyên qua các lớp UI con)
        Slot dropSlot = FindSlotUnderPointer(eventData);
        Slot originalSlot = originalParent.GetComponent<Slot>();

        // 3. Xử lý các kịch bản Drop
        if (dropSlot != null)
        {
            if (dropSlot == originalSlot)
            {
                // Thả tại chỗ: Chỉ cần về vị trí cũ
                MoveToSlot(originalSlot);
            }
            else if (dropSlot.currentItem != null)
            {
                // Ô có vật phẩm: Kiểm tra xem có cộng dồn (Stack) được không
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
                    // Không cùng ID: Đổi chỗ (Swap)
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

    // --- CÁC HÀM HỖ TRỢ TỐI ƯU ---

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
        // 1. Xác định xem Item này vốn dĩ xuất phát từ đâu
        // Chúng ta kiểm tra xem originalParent (ô Slot cũ) nằm trong Inventory hay Toolbar

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

        // 2. Định nghĩa "Vùng An Toàn" dựa trên xuất xứ của Item
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

        // 1. CHUỘT PHẢI: Tách Stack
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Nếu chỉ có 1 cái thì không tách, mà hiện Menu luôn (hoặc không làm gì)
            if (item != null && item.quantity > 1)
            {
                SplitStack();
            }
            else
            {
                // Nếu chỉ có 1 cái, chuột phải cũng hiện Menu cho tiện
                ItemUI.Instance.Show(item, eventData.position);
            }
        }
        // 2. CHUỘT TRÁI (Hoặc Tap trên Mobile): Hiện Menu Use/Sell và Highlight Slot
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
        /*
        foreach(Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem == null) 
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }
        */
        // No Empty slot - return to stack
        item.AddToStack(splitAmount);
        Destroy(newItem);
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



/*
bool IsWithInInvetory(Vector2 mousePosition)
{
    GameObject invPanel = InventoryController.Instance.inventoryPanel;
    RectTransform invetoryRect = invPanel.GetComponent<RectTransform>();
    return RectTransformUtility.RectangleContainsScreenPoint(invetoryRect, mousePosition);
}
 public void OnEndDrag(PointerEventData eventData)
{
    canvasGroup.blocksRaycasts = true;
    canvasGroup.alpha = 1f;

    if (eventData.button != PointerEventData.InputButton.Left) return;

    Slot dropSlot = null;
    var results = new System.Collections.Generic.List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, results);

    foreach (var result in results)
    {
        // Quét xem dưới chuột có cái Slot nào không
        dropSlot = result.gameObject.GetComponent<Slot>();
        if (dropSlot == null) dropSlot = result.gameObject.GetComponentInParent<Slot>();

        if (dropSlot != null) break; 
    }

    Slot originalSlot = originalParent.GetComponent<Slot>();

    // Thả vào chính ô cũ (Tránh lỗi biến mất item)
    if (dropSlot == originalSlot)
    {
        transform.SetParent(originalParent);
        transform.localScale = Vector3.one; 
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        return;
    }

    if (dropSlot != null)
    {
        if (dropSlot.currentItem != null)
        {
            Item draggedItem = GetComponent<Item>();
            Item targetItem = dropSlot.currentItem.GetComponent<Item>();

            if (draggedItem.ID == targetItem.ID)
            {
                targetItem.AddToStack(draggedItem.quantity);
                originalSlot.currentItem = null;
                Destroy(gameObject);
                return;
            }
            else
            {
                // SWAP ITEM
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                dropSlot.currentItem.transform.localScale = Vector3.one;

                originalSlot.currentItem = dropSlot.currentItem;
                originalSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                transform.SetParent(dropSlot.transform);
                transform.localScale = Vector3.one; 
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // THẢ VÀO Ô TRỐNG
            originalSlot.currentItem = null;
            transform.SetParent(dropSlot.transform);
            transform.localScale = Vector3.one; 
            dropSlot.currentItem = gameObject;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        // Toolbar Select
        ToolbarController toolbar = FindAnyObjectByType<ToolbarController>();
        if (toolbar != null) toolbar.SelectSlot(dropSlot);
    }
    else
    {
        if (!IsWithInInvetory(eventData.position))
        {
            DropItem(originalSlot);
        }
        else
        {
            // SNAP BACK
            transform.SetParent(originalParent);
            transform.localScale = Vector3.one; 
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
}

    public void OnEndDrag(PointerEventData eventData)
{
    canvasGroup.blocksRaycasts = true; // Enables raycasts
    canvasGroup.alpha = 1f; //No longer transparent

    if (eventData.button != PointerEventData.InputButton.Left)
        return;

    Debug.Log("End Drag");
    Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //Slot where item dropped


    if (dropSlot == null)
    {
        GameObject dropItem = eventData.pointerEnter;
        if (dropItem != null)
        {
            dropSlot = dropItem.GetComponentInParent<Slot>();
        }
    }

    Slot originalSlot = originalParent.GetComponent<Slot>();

    if (dropSlot != null)
    {

        // Is a slot under drop point
        if (dropSlot.currentItem != null)
        {

            Item draggedItem = GetComponent<Item>();
            Item targetItem = dropSlot.currentItem.GetComponent<Item>();

            if (draggedItem.ID == targetItem.ID)
            {
                targetItem.AddToStack(draggedItem.quantity);
                originalSlot.currentItem = null;
                Destroy(gameObject);
                return;
            }
            else
            {
                //Slot has an item - swap item
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


                // Move item into drop slot
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Zero

            }
        }
        else
        {
            originalSlot.currentItem = null;
            // Move item into drop slot
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Zero
        }

        // THêm

        if (dropSlot != null)
        {
            ToolbarController toolbar = FindAnyObjectByType<ToolbarController>();
            if (toolbar != null)
            {
                toolbar.SelectSlot(dropSlot);
            }
        }
        //GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //================================================================

    }
    else
    {
        // Nếu như drop ngoài inventory
        if (!IsWithInInvetory(eventData.position))
        {
            // drop item 
            DropItem(originalSlot);
        }
        else
        {
            //Snap back to og slot
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Zero
        }
    }
}

bool IsWithInInvetory(Vector2 mousePosition)
{
    RectTransform invetoryRect = originalParent.parent.GetComponent<RectTransform>();
    return RectTransformUtility.RectangleContainsScreenPoint(invetoryRect, mousePosition);
}

void DropItem(Slot originalSlot)
{

    Item item = GetComponent<Item>();
    int quantity = item.quantity;
    if (quantity > 0)
    {
        item.RemoveFromStack();

        transform.SetParent(originalParent);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        quantity = 1;
    }
    else
    {
        originalSlot.currentItem = null;
    }

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
    GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);

    Item droppedItem = dropItem.GetComponent<Item>();
    droppedItem.quantity = 1;


    //Destroy the UI one
    if (quantity <= 1 && originalSlot.currentItem == null)
    {
        Destroy(gameObject);
    }
}
*/

