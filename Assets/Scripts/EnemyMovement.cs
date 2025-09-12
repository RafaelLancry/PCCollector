using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float playerCheckDistance = 0.35f; // Aumentei para detectar as bordas
    [SerializeField] float edgeCheckHorizontalOffset = 0.1f;
    [SerializeField] float edgeCheckDownDistance = 1.0f;
    [SerializeField] LayerMask obstacleLayerMask = 1 << 6; // Layer do Ground
    [SerializeField] LayerMask playerLayerMask = 1 << 8;   // Layer do Player
    [SerializeField] LayerMask groundLayerMask = 1 << 6;   // Layer do Ground

    Rigidbody2D myRigidbody;
    CapsuleCollider2D enemyBodyCollider;
    bool facingRight = true;
    float currentMoveSpeed;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        enemyBodyCollider = GetComponent<CapsuleCollider2D>();
        currentMoveSpeed = moveSpeed;

        facingRight = moveSpeed > 0;

        if (!myRigidbody) Debug.LogError($"{name}: Rigidbody2D ausente.");
        if (!enemyBodyCollider) Debug.LogError($"{name}: CapsuleCollider2D ausente.");
    }

    void Update()
    {
        if (ShouldTurn())
        {
            FlipDirection();
        }

        myRigidbody.linearVelocity = new Vector2(currentMoveSpeed, myRigidbody.linearVelocity.y);
    }

    bool ShouldTurn()
    {
        // Remove CheckForWallAhead() - agora usa apenas colisão física
        return CheckForPlayerAhead() || CheckForEdgeAhead();
    }

    bool CheckForPlayerAhead()
    {
        if (!enemyBodyCollider) return false; // Evita erros caso não exista collider

        // Direção que o inimigo está olhando
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Origem do raycast: exatamente no centro do inimigo, levemente acima do chão
        Vector2 origin = (Vector2)enemyBodyCollider.bounds.center + new Vector2(0f, 0.1f);

        // Raycast para detectar o player
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, playerCheckDistance, playerLayerMask);

        // Debug visual no SceneView
        Debug.DrawRay(origin, direction * playerCheckDistance, Color.yellow);

        // Se não bateu em nada, não há player à frente
        if (hit.collider == null)
            return false;

        // Distância até o player
        float distanceToPlayer = hit.distance;

        // Largura do inimigo para cálculo de proximidade
        float enemyHalfWidth = enemyBodyCollider.bounds.extents.x;

        // Verifica se o player está realmente à frente e próximo
        if (distanceToPlayer <= enemyHalfWidth + 0.1f)
        {
            float dx = hit.point.x - transform.position.x;
            bool playerIsAhead = (facingRight && dx > 0f) || (!facingRight && dx < 0f);
            return playerIsAhead;
        }

        return false;
    }



    bool CheckForEdgeAhead()
    {
        Vector2 center = enemyBodyCollider ? (Vector2)enemyBodyCollider.bounds.center : (Vector2)transform.position;
        float xOffset = (enemyBodyCollider ? enemyBodyCollider.bounds.extents.x : 0.2f) + edgeCheckHorizontalOffset;
        Vector2 origin = center + new Vector2(facingRight ? xOffset : -xOffset, 0f);

        // Raycast para detectar se há chão à frente
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDownDistance, groundLayerMask);
        Debug.DrawRay(origin, Vector2.down * edgeCheckDownDistance, Color.blue);

        // Se não há chão à frente, deve virar
        return hit.collider == null;
    }

    void FlipDirection()
    {
        currentMoveSpeed = -currentMoveSpeed;
        facingRight = !facingRight;
        FlipEnemyFacing();
    }

    void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(-transform.localScale.x, 1f);
    }

    // Mantém apenas para detecção de bordas se necessário
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Só vira se estiver saindo do chão (borda de plataforma)
            FlipDirection();
        }
    }

    // Adiciona colisão física para contato direto com paredes (mantém para paredes apenas)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Verifica se a colisão é na direção do movimento
            Vector2 contactPoint = collision.contacts[0].point;
            float contactDirection = contactPoint.x - transform.position.x;
            bool contactIsAhead = (facingRight && contactDirection > 0) || (!facingRight && contactDirection < 0);

            if (contactIsAhead)
            {
                FlipDirection();
            }
        }
    }
}
