using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class MapTransation : MonoBehaviour
{
    [SerializeField] BoxCollider2D mapBoundry;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    CinemachineConfiner2D confiner;
    [SerializeField] float addtivePos = 2f;

    enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();

    }



    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        confiner.BoundingShape2D = mapBoundry;
        confiner.InvalidateBoundingShapeCache();
        UpdatePlayerPosition(gameObject);

        // Teleport Player
        Vector3 targetPos = teleportTargetPosition.position;
        collision.transform.position = targetPos;

        // Cập nhật confiner
        if (confiner != null && mapBoundry != null)
        {
            confiner.BoundingShape2D = mapBoundry;
            confiner.InvalidateBoundingShapeCache();
        }
    }
    */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        FadeTransition(collision.gameObject);
    
    }

    async void FadeTransition(GameObject player)
    {
        await ScreenFader.Instance.FadeOut();

        // 2. Cập nhật Camera (Confiner)
        if (confiner != null && mapBoundry != null)
        {
            confiner.BoundingShape2D = mapBoundry;
            confiner.InvalidateBoundingShapeCache();
        }

        // Dịch chuyển Player 
        UpdatePlayerPosition(player);
        await ScreenFader.Instance.FadeIn();
    }

    private void UpdatePlayerPosition(GameObject player)
    {

        if (direction == Direction.Teleport) {
        
            player.transform.position = teleportTargetPosition.position;

            return;

        }

        Vector2 additivePos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                additivePos.y += 1;
                break;
            case Direction.Down:
                additivePos.y += -1;
                break;
            case Direction.Left:
                additivePos.x -= 1;
                break;
            case Direction.Right:
                additivePos.x += 1;
                break;
        }

        player.transform.position = additivePos;
    }
    
}