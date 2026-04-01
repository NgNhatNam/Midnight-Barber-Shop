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

        // Tìm script Health của người chơi
        Health playerHealth = FindAnyObjectByType<Health>();

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    // Item thì mới cần rewardID
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;

                case RewardType.Gold:
                    // Gold chỉ cần amount, bỏ qua rewardID (đúng ý bạn muốn)
                    if (playerHealth != null)
                    {
                        playerHealth.AddGold(reward.amount);
                        Debug.Log($"Nhận thưởng {reward.amount} Vàng!");
                    }
                    break;

                case RewardType.Experience:
                    // Tương tự Gold, chỉ cần amount
                    if (playerHealth != null)
                    {
                        playerHealth.AddExperience(reward.amount);
                        Debug.Log($"Nhận thưởng {reward.amount} EXP!");
                    }
                    break;

                case RewardType.Custom:
                    // Xử lý các loại thưởng đặc biệt khác nếu có
                    break;
            }
        }
    }

    /*
    public void GiveQuestReward(Quest quest)
    {
        if (quest == null || quest.questRewards == null) return;
        Health playerHealth = FindAnyObjectByType<Health>();

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type) 
            { 
                case RewardType.Item:
                    //GiveItemReward
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    break;
                case RewardType.Experience:
                    break;
                case RewardType.Custom:
                    //GiveItemReward
                    break;
            }
        }
    }*/
 
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
    


}
