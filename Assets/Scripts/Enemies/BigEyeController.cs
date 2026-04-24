using UnityEngine;

public class BigEyeController: MonoBehaviour
{
    Animator animator;
    BoxCollider2D boxCollider;
    Rigidbody2D rb;
    EnemyController enemyController;

    bool isFacingRight;
    bool isGrounded;
    bool isJumping;

    float jumpTimer;
    float jumpDelay = 0.25f;

    int jumpPatternIndex;
    int[] jumpPattern;
    int[][] jumpPatterns = new int[][] {
        new int[1] { 1 },           // High Jump
        new int[2] { 0, 1 },        // Low Jump, High Jump
        new int[3] { 0, 0, 1 }      // Low Jump, Low Jump, High Jump
    };
    int jumpVelocityIndex;
    Vector2 jumpVelocity;
    Vector2[] jumpVelocities =
    {
        new Vector2(1.0f, 3.0f), // Low jump
        new Vector2(0.75f, 4.0f), // High jump
    };

    public AudioClip jumpLandedSound;

    public enum BigEyeColors
    {
        Blue,
        Orange,
        Red,
    }
    [SerializeField] BigEyeColors bigEyeColor = BigEyeColors.Blue;
    [SerializeField] RuntimeAnimatorController racBigEyeBlue;
    [SerializeField] RuntimeAnimatorController racBigEyeOrange;
    [SerializeField] RuntimeAnimatorController racBigEyeRed;


    [SerializeField] bool enableAI;

    public enum MoveDirections 
    {
        Left,
        Right,
    }
    [SerializeField] MoveDirections moveDirection = MoveDirections.Left;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        boxCollider = enemyController.GetComponent<BoxCollider2D>();
        rb = enemyController.GetComponent<Rigidbody2D>();

        isFacingRight = true;
        if (moveDirection == MoveDirections.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }

        SetColor(bigEyeColor);
        jumpPattern = null;
    }

    void FixedUpdate()
    {
        isGrounded = false;
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.05f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        Vector3 boxOrigin = boxCollider.bounds.center;
        boxOrigin.y = boxCollider.bounds.min.y + (boxCollider.bounds.extents.y / 4f);
        Vector3 boxSize = boxCollider.bounds.size;
        boxSize.y = boxCollider.bounds.size.y / 4f;
        raycastHit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.down, raycastDistance, layerMask);
        if (raycastHit.collider != null)
        {            
            isGrounded = true;
            if (isJumping)
            {
                SoundManager.Instance.Play(jumpLandedSound);
                isJumping = false;
            } 
        }
        raycastColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxOrigin + new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(boxOrigin - new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(boxOrigin - new Vector3(boxCollider.bounds.extents.x, boxCollider.bounds.extents.y / 4f + raycastDistance), Vector2.right * (boxCollider.bounds.extents.x * 2), raycastColor);
    }

    void Update()
    {
        if (enemyController.freezeEnemy)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            Debug.DrawLine(transform.position, player.transform.position, Color.blue);
        }
        if (enableAI)
        {            
            if (isGrounded)
            {
                animator.Play("BigEye_Grounded");
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                jumpTimer -= Time.deltaTime;
                if (jumpTimer < 0)
                {
                    if (jumpPattern == null)
                    {
                        jumpPatternIndex = 0;
                        jumpPattern = jumpPatterns[Random.Range(0, jumpPatterns.Length)];
                    }
                    jumpVelocityIndex = jumpPattern[jumpPatternIndex];
                    jumpVelocity = jumpVelocities[jumpVelocityIndex];
                    if(player.transform.position.x <= transform.position.x)
                    {
                        jumpVelocity.x *= -1;
                    }
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity.y);
                    jumpTimer = jumpDelay;
                    if(++jumpPatternIndex > jumpPattern.Length - 1)
                    {
                        jumpPattern = null;
                    }
                }
            }
            else
            {
                animator.Play("BigEye_Jumping");
                rb.linearVelocity = new Vector2(jumpVelocity.x, rb.linearVelocity.y);
                isJumping = true;
                if (jumpVelocity.x < 0)
                {
                    if (isFacingRight)
                    {
                        isFacingRight = !isFacingRight;
                        enemyController.Flip();
                    }
                }
                else
                {
                    if (!isFacingRight)
                    {
                        isFacingRight = !isFacingRight;
                        enemyController.Flip();
                    }
                }
            }
        }
    }

    public void EnableAI(bool enable)
    {
        // enable enemy ai logic
        enableAI = enable;
    }


    public void SetColor(BigEyeColors color)
    {
        bigEyeColor = color;
        SetAnimatorColor();

    }

    void SetAnimatorColor()
    {
        switch (bigEyeColor)
        {
            case BigEyeColors.Blue:
                animator.runtimeAnimatorController = racBigEyeBlue;
                break;
            case BigEyeColors.Orange:
                animator.runtimeAnimatorController = racBigEyeOrange;
                break;
            case BigEyeColors.Red:
                animator.runtimeAnimatorController = racBigEyeRed;
                break;
        }
    }
}
