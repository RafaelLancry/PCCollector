using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float playerCheckDistance = 0.6f; // Aumentei para detectar as bordas
    [SerializeField] float edgeCheckDistance = 0.001f;
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
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.1f;
        
        // Raycast para detectar o player com distância maior para pegar as bordas
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, playerCheckDistance, playerLayerMask);
        
        Debug.DrawRay(rayOrigin, rayDirection * playerCheckDistance, Color.yellow);
        
        if (hit.collider != null)
        {
            // Calcula a distância real até a borda do collider do player
            float distanceToPlayerEdge = hit.distance;
            float enemyHalfWidth = enemyBodyCollider.size.x * 0.5f;
            
            // Vira quando estiver próximo da borda do player (considerando tamanho do inimigo)
            if (distanceToPlayerEdge <= enemyHalfWidth + 0.1f)
            {
                float playerDirection = hit.point.x - transform.position.x;
                bool playerIsAhead = (facingRight && playerDirection > 0) || (!facingRight && playerDirection < 0);
                return playerIsAhead;
            }
        }
        
        return false;
    }

    bool CheckForEdgeAhead()
    {
        Vector2 rayDirection = Vector2.down;
        Vector2 rayOrigin = (Vector2)transform.position + (facingRight ? Vector2.right : Vector2.left) * edgeCheckDistance;
        
        // Raycast para detectar se há chão à frente
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, edgeCheckDistance, groundLayerMask);
        
        // Debug visual
        Debug.DrawRay(rayOrigin, rayDirection * edgeCheckDistance, Color.blue);
        
        // Se não há chão à frente, deve virar
        return hit.collider == null && !hit.collider.IsTouchingLayers(LayerMask.GetMask("Ground"));
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
