using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 20f;
    [SerializeField] float lifeTime = 3f; // autodestruir para não ficar eterno

    Rigidbody2D rb;
    Collider2D bulletCol;
    PlayerMovement player;
    float xSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletCol = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            var playerCols = player.GetComponentsInChildren<Collider2D>();
            foreach (var pc in playerCols)
                Physics2D.IgnoreCollision(bulletCol, pc, true);

            // Direção da bala baseada no flip do personagem
            xSpeed = Mathf.Sign(player.transform.localScale.x) * bulletSpeed;
        }

        // Evita lixo em cena
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        rb.linearVelocity = new Vector2(xSpeed, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
    }

}
