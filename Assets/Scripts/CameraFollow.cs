using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;

    private void LateUpdate()
    {
        var player = GameManager.Instance.player;
        if (player == null) return;

        // 1) Creo la posizione di destinazione solo sull'asse X
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            transform.position.y,
            transform.position.z
        );

        // 2) Interpolo la posizione corrente verso targetPos e assegno
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }
}
