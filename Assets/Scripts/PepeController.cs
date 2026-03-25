using UnityEngine;

public class PepeController: MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    EnemyController enemyController;

    bool isFacingRight;

    bool isFollowingPath;
    Vector3 pathStartPoint;
    Vector3 pathEndPoint;
    Vector3 pathMidPoint;
    float pathTimeStart;

    public float bezierTime = 1f;
    public float bezierDistance = 1f;
    public Vector3 bezierHeight = new Vector3(0f, 0.8f, 0f);
    

    public enum MoveDirection
    {
        Left,
        Right
    }
    [SerializeField] MoveDirection moveDirection = MoveDirection.Left;

    void Start() {
        enemyController = GetComponent<EnemyController>();
        animator = enemyController.GetComponent<Animator>();
        rb = enemyController.GetComponent<Rigidbody2D>();

        isFacingRight = true;
        if (moveDirection == MoveDirection.Left)
        {
            isFacingRight = false;
            enemyController.Flip();
        }
    }

    void Update()
    {
        if(enemyController.freezeEnemy)
        {
            pathTimeStart += Time.deltaTime;
            return;
        }

        animator.Play("Pepe_Flying");

        if (!isFollowingPath)
        {
            float distance = isFacingRight ? bezierDistance : -bezierDistance;
            pathStartPoint = rb.transform.position;
            pathEndPoint = new Vector3(pathStartPoint.x + distance, pathStartPoint.y, pathStartPoint.z);
            pathMidPoint = pathStartPoint + (pathEndPoint - pathStartPoint) / 2 + bezierHeight;
            pathTimeStart = Time.time;
            isFollowingPath = true;
        }
        else
        {
            float percentage = (Time.time - pathTimeStart) / bezierTime;
            rb.transform.position = UtilityFunctions.CalculateQuadraticBezierPoint(pathStartPoint, pathMidPoint, pathEndPoint, percentage);
            if (percentage >= 1f)
            {
                bezierHeight *= -1f;
                isFollowingPath = false;
            }
        }
    }

    public void SetMoveDirection(MoveDirection direction)
    {
        moveDirection = direction;
        if ((moveDirection == MoveDirection.Left && isFacingRight) || (moveDirection == MoveDirection.Right && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            enemyController.Flip();
        }
    }

    public void ResetFollowingPath()
    {
        isFollowingPath = false;
        
    }

}
