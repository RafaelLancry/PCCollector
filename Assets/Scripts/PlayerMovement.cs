using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float ladderJumpGrace = 0.20f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);

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
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.linearVelocity = deathKick;
            myRigidbody.bodyType = RigidbodyType2D.Static;

            myFeetCollider.enabled = false;
        }
    }
}
