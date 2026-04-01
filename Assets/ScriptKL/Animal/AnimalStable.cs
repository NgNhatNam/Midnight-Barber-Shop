using UnityEngine;
using System.Collections.Generic;
using DPUtils.System.DateTime;

public class AnimalStable : MonoBehaviour, IInteractable
{
    [Header("Save")]
    public string stableID;

    [Header("Setting")]
    public string stableName = "";
    public int maxCapacity = 5;
    public Transform spawnPoint;    
    public Collider2D boundary;     

    [Header("Animal Prefab for Stable")]
    public List<GameObject> animalPrefabs;

    
    public List<Animal> spawnedAnimals = new List<Animal>();

    public bool CanInteract() => true;

    public void Interact()
    {
        AnimalUIManager.Instance.OpenStableUI(this);
    }

    public void SpawnAnimalByIndex(int index)
    {
        if (spawnedAnimals.Count >= maxCapacity) return;

        Vector2 randomOffset = Random.insideUnitCircle * 0.7f;
        Vector3 finalPos = spawnPoint.position + (Vector3)randomOffset;

        GameObject newObj = Instantiate(animalPrefabs[index], finalPos, Quaternion.identity);
        Animal animalScript = newObj.GetComponent<Animal>();

        if (animalScript != null)
        {
            animalScript.moveArea = this.boundary;
            if (TimeManager.Instance != null)
            {
                animalScript.birthDayTotal = TimeManager.Instance.GetCurrentDateTime().TotalNumDays;
            }

            spawnedAnimals.Add(animalScript);
        }
    }

    public void RemoveAnimal(Animal animal)
    {
        if (spawnedAnimals.Contains(animal))
        {
            spawnedAnimals.Remove(animal);
            Destroy(animal.gameObject);
        }
    }

    public int GetCurrentCount() => spawnedAnimals.Count;

    public StableSaveData GetStableSaveData()
    {
        StableSaveData data = new StableSaveData();
        data.stableID = string.IsNullOrEmpty(stableID) ? gameObject.name : stableID;

        foreach (Animal animal in spawnedAnimals)
        {
            data.animals.Add(new AnimalSaveData
            {
                // Lưu tên prefab
                animalPrefabName = animal.gameObject.name.Replace("(Clone)", "").Trim(),
                position = animal.transform.position,
                birthDayTotal = animal.birthDayTotal, 
                lastHarvestTotalDays = animal.lastHarvestTotalDays 
            });
        }
        return data;
    }

    public void LoadStableSaveData(StableSaveData data)
    {
        // Xóa hết gia súc cũ đang có trong scene trước khi load
        foreach (Animal a in spawnedAnimals) Destroy(a.gameObject);
        spawnedAnimals.Clear();

        foreach (var aData in data.animals)
        {
            // Tìm prefab tương ứng trong list animalPrefabs dựa trên tên đã lưu
            GameObject prefab = animalPrefabs.Find(p => p.name == aData.animalPrefabName);
            if (prefab != null)
            {
                GameObject newObj = Instantiate(prefab, aData.position, Quaternion.identity);
                Animal animalScript = newObj.GetComponent<Animal>();

                // Khôi phục trạng thái
                animalScript.moveArea = this.boundary;
                animalScript.birthDayTotal = aData.birthDayTotal;
                animalScript.lastHarvestTotalDays = aData.lastHarvestTotalDays;

                spawnedAnimals.Add(animalScript);
            }
        }
    }

}