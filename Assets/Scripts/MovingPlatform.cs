using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float speed = 2f;  // unità/secondo
    [SerializeField] private float moveDistance = 5f;  // distanza massima dal punto iniziale

    private Vector3 leftPoint;
    private Vector3 rightPoint;
    private Vector3 currentTarget;

    private void Start()
    {
        Vector3 start = transform.position;
        leftPoint = start + Vector3.left * moveDistance;
        rightPoint = start + Vector3.right * moveDistance;
        currentTarget = rightPoint;
    }

    private void Update()
    {
        // spostati verso l'obiettivo corrente
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget,
            speed * Time.deltaTime
        );

        // se sei arrivato (o quasi), inverte il target
        if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
        {
            currentTarget = (currentTarget == rightPoint) ? leftPoint : rightPoint;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Player"))
            coll.collider.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Player"))
            coll.collider.transform.SetParent(null);
    }

}
