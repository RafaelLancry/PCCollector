using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    Rigidbody2D myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        myRigidbody.linearVelocity = new Vector2(moveSpeed, 0f);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        moveSpeed = -moveSpeed;
        FlipEnemyFacing();
    }

    void OnCollisionExit2D (Collision2D collision)
    {
        if (myRigidbody.IsTouchingLayers(LayerMask.GetMask("Player")))
        {
            moveSpeed = -moveSpeed;
            FlipEnemyFacing();
        }
    }

    void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(-(Mathf.Sign(myRigidbody.linearVelocityX)), 1f);
    }
}
