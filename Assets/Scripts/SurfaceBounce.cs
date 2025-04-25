using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SurfaceBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Moltiplicatore della velocità dopo il rimbalzo")]
    [SerializeField] private float bounceFactor = 1f;

    [Tooltip("Se true, legge il bounciness da PhysicsMaterial2D del collider")]
    [SerializeField] private bool useMaterialBounciness = true;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("Collisione con: " + collision.gameObject.name);

        if (!collision.gameObject.CompareTag("Map")) return;
        print("Rimbalzo con: " + collision.gameObject.name);

        // 1) Prendi la normale del primo punto di contatto
        Vector2 normal = collision.contacts[0].normal;
        // v' = Reflect(v, normal)
        Vector2 incoming = rb.velocity;
        Vector2 reflected = Vector2.Reflect(incoming, normal);  // :contentReference[oaicite:0]{index=0}

        float finalFactor = bounceFactor;

        // 2) Eventualmente usa il coefficiente di restituzione dal PhysicsMaterial2D
        if (useMaterialBounciness && col.sharedMaterial != null)
        {
            finalFactor *= col.sharedMaterial.bounciness;       // :contentReference[oaicite:1]{index=1}
        }

        // 3) Applica la nuova velocità rimbalzata
        rb.velocity = reflected * finalFactor;
    }
}
