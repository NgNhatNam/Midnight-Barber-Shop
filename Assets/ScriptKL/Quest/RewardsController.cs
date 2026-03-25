using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest == null || quest.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type) 
            { 
                case RewardType.Item:
                    //GiveItemReward
                    GiveItemReward(reward.rewardID, reward.amount);

                    break;
                case RewardType.Gold:
                    //GiveItemReward
                    break;
                case RewardType.Experience:
                    //GiveItemReward
                    break;
                case RewardType.Custom:
                    //GiveItemReward
                    break;
            }
        }
    }

    public void GiveItemReward(int itemID, int amount)
    {
        if (ItemDictionary.Instance == null) return;

        GameObject prefab = ItemDictionary.Instance.GetItemPrefab(itemID);
        if (prefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            // Tạo bản sao từ Prefab
            GameObject newItem = Instantiate(prefab);
            newItem.SetActive(false); // Tạm ẩn để không hiện giữa map

            // Thử thêm bản sao này vào Inventory
            if (InventoryController.Instance.AddItem(newItem))
            {
                // Nếu thêm thành công thì mới hiện PopUp từ bản sao
                newItem.GetComponent<Item>().ShowPopUp();
            }
            else
            {
                // Nếu rương đầy thì Cho rơi ra đất
                newItem.SetActive(true);
                newItem.transform.position = transform.position + Vector3.down;
                newItem.transform.position += new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
            }
        }
    }

    /*
    public void GiveItemReward(int itemID, int amount)
    {

        var itemPrefab = FindFirstObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);
        //var itemPrefab = ItemDictionary.Instance.GetItemPrefab(itemID);

        if (itemPrefab == null) return;

        for (int i = 0; i < amount; i++) 
        {
            if (!InventoryController.Instance.AddItem(itemPrefab))
            {
                GameObject dropedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
                dropedItem.transform.position += new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
                
            }
            else
            {
                itemPrefab.GetComponent<Item>().ShowPopUp();
            }
        }
    }*/


}
