using UnityEngine;

public class Grid : MonoBehaviour
{
    public float gridSize = 1f;

    public GameObject thingToPlaceDown;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            PlaceItem();
        }
    }

    private void PlaceItem()
    {
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        Vector3 snappedMousePos = new Vector2(Mathf.Round(mousePos.x / gridSize) * gridSize, Mathf.Round(mousePos.y / gridSize) * gridSize);

        Instantiate(thingToPlaceDown, snappedMousePos, Quaternion.identity);
    }
}
