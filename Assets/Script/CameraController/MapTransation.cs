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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Nếu là Player: Có Fade Transition
        if (collision.CompareTag("Player"))
        {
            FadeTransition(collision.gameObject);
        }
        // 2. Nếu là NPC: Dịch chuyển ngay lập tức (Warp) để không lỗi NavMesh
        else if (collision.CompareTag("NPC"))
        {
            HandleNPCTransition(collision.gameObject);
        }
    }

    async void FadeTransition(GameObject player)
    {
        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeOut();

        // Cập nhật Camera (Confiner)
        UpdateCameraBounds();

        // Dịch chuyển Player 
        UpdatePosition(player);

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeIn();
    }

    private void HandleNPCTransition(GameObject npcGO)
    {
        UpdatePosition(npcGO); // Warp ở đây

        // Gọi lệnh cập nhật lại đường đi cho NPC
        NPC npcScript = npcGO.GetComponent<NPC>();
        if (npcScript != null)
        {
            npcScript.OnMapTeleported();
        }
    }

    private void UpdateCameraBounds()
    {
        if (confiner != null && mapBoundry != null)
        {
            confiner.BoundingShape2D = mapBoundry;
            confiner.InvalidateBoundingShapeCache();
        }
    }

    private void UpdatePosition(GameObject entity)
    {
        Vector3 targetPos;

        if (direction == Direction.Teleport)
        {
            targetPos = teleportTargetPosition.position;
        }
        else
        {
            Vector2 offset = entity.transform.position;
            switch (direction)
            {
                case Direction.Up: offset.y += addtivePos; break;
                case Direction.Down: offset.y -= addtivePos; break;
                case Direction.Left: offset.x -= addtivePos; break;
                case Direction.Right: offset.x += addtivePos; break;
            }
            targetPos = offset;
        }

        // KIỂM TRA: Nếu có NavMeshAgent thì phải dùng Warp
        NavMeshAgent agent = entity.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.Warp(targetPos); // Đây là lệnh quan trọng nhất cho NPC
        }
        else
        {
            entity.transform.position = targetPos;
        }
    }



    /*
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
    */

}