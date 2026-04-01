using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalUIManager : MonoBehaviour
{
    public static AnimalUIManager Instance { get; private set; }

    [Header("UI")]
    public GameObject uiPanel;
    public TMP_Text titleText;

    [Header("Store")]
    public Transform shopContainer; 
    public GameObject shopItemPrefab; 

    [Header("My Farm")]
    public Transform myFarmContainer; 
    public GameObject farmItemPrefab; 

    private AnimalStable currentStable;

    private void Awake() { Instance = this; uiPanel.SetActive(false); }

    public void OpenStableUI(AnimalStable stable)
    {
        currentStable = stable;
        uiPanel.SetActive(true);
        RefreshUI();
    }

    public void RefreshUI()
    {
        titleText.text = currentStable.stableName;

        Health playerHealth = FindAnyObjectByType<Health>();

        foreach (Transform child in shopContainer) Destroy(child.gameObject);
        foreach (Transform child in myFarmContainer) Destroy(child.gameObject);

        //  Danh sách SHOP 
        for (int i = 0; i < currentStable.animalPrefabs.Count; i++)
        {
            int index = i;
            Animal data = currentStable.animalPrefabs[index].GetComponent<Animal>();
            int buyPrice = data.basePrice;

            GameObject item = Instantiate(shopItemPrefab, shopContainer);

            // Ẩn nút Mổ ở bên Shop
            Transform slaughterBtnInShop = item.transform.Find("SlaughterButton");
            if (slaughterBtnInShop != null)
            {
                slaughterBtnInShop.gameObject.SetActive(false);
            }

            // Sử dụng SetupRow để gán cho nút chính (Nút Mua)
            SetupRow(item, data.animalName, $"Giá: {buyPrice}", "Mua", () => {
                if (playerHealth.Gold >= buyPrice)
                {
                    playerHealth.SpendGold(buyPrice);
                    currentStable.SpawnAnimalByIndex(index);
                    RefreshUI();
                }
                else
                {
                    Debug.Log("Không đủ tiền mua con vật này!");
                }
            });
        }

        //  Danh sách MY FARM 
        foreach (Animal animal in currentStable.spawnedAnimals)
        {
            GameObject item = Instantiate(farmItemPrefab, myFarmContainer);

            // Gán Tên và Giá chung
            item.transform.Find("NameText").GetComponent<TMP_Text>().text = animal.animalName;
            item.transform.Find("PriceText").GetComponent<TMP_Text>().text = $"Giá bán: {animal.GetSellPrice()}";

         
            Transform sellBtnTrans = item.transform.Find("SellButton");
            if (sellBtnTrans != null)
            {
                Button sellBtn = sellBtnTrans.GetComponent<Button>();
                sellBtn.GetComponentInChildren<TMP_Text>().text = "Bán";
                sellBtn.onClick.RemoveAllListeners();
                sellBtn.onClick.AddListener(() => {
                    playerHealth.AddGold(animal.GetSellPrice());
                    currentStable.RemoveAnimal(animal);
                    RefreshUI();
                });
            }

            
            Transform slaughterBtnTrans = item.transform.Find("SlaughterButton");
            if (slaughterBtnTrans != null)
            {
                Button sBtn = slaughterBtnTrans.GetComponent<Button>();
                int meatCount = animal.GetSlaughterMeatAmount(); 
                sBtn.GetComponentInChildren<TMP_Text>().text = $"Mổ ({meatCount})";

                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(() => {
                    RewardsController.Instance.GiveItemReward(animal.meatItemID, meatCount);
                    currentStable.RemoveAnimal(animal);
                    RefreshUI();
                });
            }
        }
    }

    // Hàm phụ để gán dữ liệu vào dòng cho gọn code
    void SetupRow(GameObject rowObj, string name, string price, string btnLabel, UnityEngine.Events.UnityAction action)
    {
        rowObj.transform.Find("NameText").GetComponent<TMP_Text>().text = name;
        rowObj.transform.Find("PriceText").GetComponent<TMP_Text>().text = price;
        Button btn = rowObj.GetComponentInChildren<Button>();
        btn.GetComponentInChildren<TMP_Text>().text = btnLabel;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void CloseUI() => uiPanel.SetActive(false);
}