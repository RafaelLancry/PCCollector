using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float ladderJumpGrace = 0.20f;
    [SerializeField] Vector2 deathKick = new Vector2(0f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    // ---- Config do CapsuleCollider2D----
    [Header("Collider - Vivo")]
    [SerializeField] Vector2 aliveColliderOffset = new Vector2(-0.002412796f, -0.003392637f);
    [SerializeField] Vector2 aliveColliderSize = new Vector2(0.503624f, 0.9154408f);
    [SerializeField] CapsuleDirection2D aliveDirection = CapsuleDirection2D.Vertical;

    [Header("Collider - Morto")]
    [SerializeField] Vector2 deadColliderOffset = new Vector2(-0.002412796f, -0.078f);
    [SerializeField] Vector2 deadColliderSize = new Vector2(0.67f, 0.51f);
    [SerializeField] CapsuleDirection2D deadDirection = CapsuleDirection2D.Horizontal;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float gravitySacaleAtStart;
    float ladderDetachTimer = 0f;

    bool isAlive = true;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravitySacaleAtStart = myRigidbody.gravityScale;

        if (myBodyCollider)
        {
            myBodyCollider.direction = aliveDirection;
            myBodyCollider.offset = aliveColliderOffset;
            myBodyCollider.size = aliveColliderSize;
        }
    }

    void Update()
    {
        if (!isAlive) { return; }

        if (ladderDetachTimer > 0f) { ladderDetachTimer -= Time.deltaTime; }

        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnAttack(InputValue value)
    {
        if (!isAlive) { return; }
        Instantiate(bullet, gun.position, transform.rotation);
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        moveInput = value.Get<Vector2>();
        //Debug.Log(moveInput);
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        if (!value.isPressed) return;

        int groundOrLadder = LayerMask.GetMask("Ground", "Climbing");
        bool touchingSomething = myFeetCollider.IsTouchingLayers(groundOrLadder);
        bool onLadder = myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"));

        if (!touchingSomething) return;

        myRigidbody.gravityScale = gravitySacaleAtStart;
        myAnimator.SetBool("isClimbing", false);

        Vector2 v = myRigidbody.linearVelocity;
        v.y = jumpSpeed;
        myRigidbody.linearVelocity = v;

        if (onLadder) ladderDetachTimer = ladderJumpGrace;


    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.linearVelocityY);
        myRigidbody.linearVelocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocityX) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);

    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocityX) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.linearVelocityX), 1f);
        }
    }

    void ClimbLadder()
    {
        // Se acabamos de pular da escada, não aplicar lógica de escalar
        if (ladderDetachTimer > 0f)
            return;

        bool touchingLadder = myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"));

        if (!touchingLadder)
        {
            myRigidbody.gravityScale = gravitySacaleAtStart;
            myAnimator.SetBool("isClimbing", false);
            return;
        }

        // Ativar modo escalar apenas quando houver input vertical
        Vector2 climbVelocity = new Vector2(moveInput.x * runSpeed, moveInput.y * climbSpeed);
        myRigidbody.linearVelocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        // Só sobrescreve o Y quando de fato estiver escalando (input vertical)
        if (Mathf.Abs(moveInput.y) > Mathf.Epsilon)
        {
            climbVelocity.y = moveInput.y * climbSpeed;
            myAnimator.SetBool("isClimbing", true);
        }
        else
        {
            // Sem input vertical: preserve o Y atual para não matar o pulo
            // e considere que não está "escalando ativamente"
            myAnimator.SetBool("isClimbing", false);
        }

        myRigidbody.linearVelocity = climbVelocity;
    }

    void Die()
    {
        // se já morreu, evita repetir lógica
        if (!isAlive) return;

        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            isAlive = false;

            // animação e impulso
            myAnimator.SetTrigger("Dying");
            myRigidbody.linearVelocity = deathKick;

            // opcional: manter Dynamic por 1–2 frames se você quiser que o deathKick "empurre" antes de travar.
            // aqui seguimos o padrão: ficar Static
            myRigidbody.bodyType = RigidbodyType2D.Dynamic;

            // desabilita pés para não interferir com triggers de chão
            myFeetCollider.enabled = false;

            // aplicamos a configuração do collider do sprite morto <<<
            if (myBodyCollider)
            {
                myBodyCollider.direction = deadDirection;           // Horizontal
                myBodyCollider.offset = deadColliderOffset;      // (-0.002412806, -0.07815323)
                myBodyCollider.size = deadColliderSize;        // (0.9154412, 0.5088465)
            }

            // Chama o método para mudar para Static após 2 segundos
            StartCoroutine(SetRigidBodyStatic(1f));
        }
    }

    // Método auxiliar que espera um tempo antes de travar o Rigidbody
    private IEnumerator SetRigidBodyStatic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (myRigidbody != null)
        {
            myRigidbody.bodyType = RigidbodyType2D.Static;
        }
    }
    
    // Comentado o código para Respawn
    //  public void RestoreAliveCollider()
    // {
    //     if (myBodyCollider)
    //     {
    //         myBodyCollider.direction = aliveDirection;   // Vertical
    //         myBodyCollider.offset    = aliveColliderOffset;
    //         myBodyCollider.size      = aliveColliderSize;
    //         myBodyCollider.isTrigger = false;
    //     }
    // }
}
