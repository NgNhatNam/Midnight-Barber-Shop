using UnityEngine;


public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set; }

    public string ChestID { get; private set; }

    public GameObject itemPrefab;
    public Sprite openedSprite;

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject); 
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if(!CanInteract()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        //SetOpened
        SetOpened(true);
        //DropItem
        if (itemPrefab)
        {
            GameObject dropedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            dropedItem.transform.position += new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
        }
    }
    public void SetOpened(bool opened)
    {
        IsOpened = opened;
        if (IsOpened) {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
