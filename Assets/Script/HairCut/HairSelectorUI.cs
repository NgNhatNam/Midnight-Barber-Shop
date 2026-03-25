using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HairSelectorUI : MonoBehaviour
{
    private List<HairEntry> availableStyles;
    public Image displayImage;
    private int currentIndex = 0;

    public void Setup(List<HairEntry> hairs)
    {
        availableStyles = hairs;
        ResetSelection();
    }

    public void NextHair()
    {
        if (availableStyles.Count == 0) return;
        currentIndex = (currentIndex + 1) % availableStyles.Count;
        UpdateDisplay();
    }

    public void PreviousHair()
    {
        if (availableStyles.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = availableStyles.Count - 1;
        UpdateDisplay();
    }

    public void ResetSelection()
    {
        currentIndex = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (availableStyles != null && availableStyles.Count > 0)
            displayImage.sprite = availableStyles[currentIndex].hairSprite;
    }

    public HairEntry GetSelectedHair() => availableStyles[currentIndex];
}