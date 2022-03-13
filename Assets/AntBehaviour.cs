using UnityEngine;

public enum AntState { SearchingFood, SearchingHome };

public class AntBehaviour : MonoBehaviour
{
    Vector2 direction = Vector2.up;
    Vector2 desiredDirection = Vector2.up;
    Rigidbody2D rigidbody;

    [SerializeField]
    AntState currentState = AntState.SearchingFood;

    [SerializeField, Range(0, 40f)]
    private float wanderStrength = 1f;

    [SerializeField, Range(0, 180f)]
    private float wanderRadius = 120f;

    [SerializeField, Range(0, 20f)]
    private float sightRadius = 1f;

    [SerializeField, Range(0, 180f)]
    private float sightAngle = 40f;

    [SerializeField, Range(0, 20f)]
    private float obstacleAvoidRadius = .2f;

    [SerializeField, Range(0, 180f)]
    private float obstacleAvoidAngle = 80f;

    [SerializeField, Range(0, 1f)]
    private float turningSpeed = 0.2f;

    [SerializeField, Range(0, 20f)]
    private float speed = 1f;

    private LayerMask foodFerromone;
    private LayerMask homeFerromone;
    private LayerMask foodMask;
    private LayerMask homeMask;
    private LayerMask obstacleMask;
    private FoodPocket foodPocket;

    public AntState CurrentState { get { return currentState; } }

    void Awake()
    {
        foodMask = LayerMask.GetMask("Food");
        homeMask = LayerMask.GetMask("Home");
        foodFerromone = LayerMask.GetMask("FoodFerromone");
        homeFerromone = LayerMask.GetMask("HomeFerromone");
        obstacleMask = LayerMask.GetMask("Obstacle");
        foodPocket = GetComponent<FoodPocket>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = Vector2.up;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(transform.up);

        Vector2 newDir;

        if (!SenseObstacle(obstacleMask, out newDir))
        {
            switch (currentState)
            {
                case AntState.SearchingFood:
                    if (Sense(foodMask, out newDir))
                    {

                    }
                    else if (Sense(foodFerromone, out newDir))
                    {

                    }

                    break;

                case AntState.SearchingHome:

                    if (Sense(homeMask, out newDir))
                    {

                    }
                    else if (Sense(homeFerromone, out newDir))
                    {

                    }

                    break;
            }
        }

        if (newDir == Vector2.zero)
            desiredDirection = RandomWalk();
        else
            desiredDirection = newDir;


        //Normalize vector
        desiredDirection = desiredDirection.normalized;
        // interpolate between current direction and desiredDirection
        direction = Vector2.Lerp(direction, desiredDirection, turningSpeed).normalized;
        //set direction
        transform.up = direction;
        //set speed
        rigidbody.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int objectMask = other.gameObject.layer;
        if (objectMask == 8)
        {
            handleFoodTrigger(other.gameObject);
        }
        else if (objectMask == 10)
        {
            handleHomeTrigger(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int objectMask = collision.collider.gameObject.layer;
        if (objectMask == 9)
        {
            handleObstacleTrigger(collision);
        }
        if (objectMask == 7)
        {
            Physics2D.IgnoreCollision(collision.collider.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    void handleFoodTrigger(GameObject collider)
    {
        Debug.Log("Got Food");
        foodPocket.addFood(collider);
        currentState = AntState.SearchingHome;
        desiredDirection = RandomWalk();
    }

    void handleObstacleTrigger(Collision2D collision)
    {
        Debug.Log("Wall Collide");
        Vector3 collideDir = collision.contacts[0].normal;
        desiredDirection = collideDir;
        direction = collideDir;
    }

    void handleHomeTrigger(GameObject collider)
    {
        Debug.Log("Got Home");
        foodPocket.removeFood(collider);
        currentState = AntState.SearchingFood;
        desiredDirection = RandomWalk();
    }


    bool Sense(LayerMask mask, out Vector2 dir)
    {
        //sense 3 regions infront of the ant, the size of the regions depend on sight angle and sight radius
        float anglePart = sightAngle * 2.0f / 3.0f * Mathf.Deg2Rad;
        float alpha = anglePart / 2.0f;
        float r = (sightRadius * Mathf.Sin(alpha)) / (Mathf.Sin(alpha) + 1);
        float distance = sightRadius - r;

        // calculate 3 midpoints of the circles
        //mid point
        Vector3 dirMid = distance * transform.up;
        Debug.DrawLine(transform.position, transform.position + dirMid, Color.green);
        Vector3 circleMid = transform.position + dirMid;
        
        //left point
        Vector3 dirLeft = MathUtilities.RotateRound(distance * transform.up, Vector3.zero, new Vector3(0, 0, 1), anglePart * Mathf.Rad2Deg);
        Debug.DrawLine(transform.position, transform.position + dirLeft, Color.green);
        Vector3 circleLeft = transform.position + dirLeft;

        //right point
        Vector3 dirRight = MathUtilities.RotateRound(distance * transform.up, Vector3.zero, new Vector3(0, 0, 1), -anglePart * Mathf.Rad2Deg);
        Debug.DrawLine(transform.position, transform.position + dirRight, Color.green);
        Vector3 circleRight = transform.position + dirRight;

        //Debug Draw
        //Debug.DrawCircle(circleMid, r, 20, Color.red);
        //Debug.DrawCircle(circleLeft, r, 20, Color.red);
        //Debug.DrawCircle(circleRight, r, 20, Color.red);
        Debug.DrawCircle(transform.position, sightRadius, 10, Color.green);

        //mid collider
        int midCollider = Physics2D.OverlapCircleAll(circleMid, r, mask).Length;
        //left collider
        int leftCollider = Physics2D.OverlapCircleAll(circleLeft, r, mask).Length;
        //right collider
        int rightCollider = Physics2D.OverlapCircleAll(circleRight, r, mask).Length;

        //default value
        dir = Vector2.zero;

        if (leftCollider == 0 && rightCollider == 0 && midCollider == 0)
            return false;

        //set direction to where most ferromones are
        if (rightCollider > leftCollider)
        {
            if (rightCollider > midCollider)
            {
                dir = new Vector2(dirRight.x,dirRight.y);
                Debug.DrawCircle(circleRight, r, 20, Color.red);
            }
            else
            {
                dir = new Vector2(dirMid.x, dirMid.y);
                Debug.DrawCircle(circleMid, r, 20, Color.red);
            }
        }
        else
        {
            if (leftCollider > midCollider)
            {
                dir = new Vector2(dirLeft.x, dirLeft.y);
                Debug.DrawCircle(circleLeft, r, 20, Color.red);
            }
            else
            {
                dir = new Vector2(dirMid.x, dirMid.y);
                Debug.DrawCircle(circleMid, r, 20, Color.red);
            }
        }

        dir += dir == Vector2.zero ? dir : dir + Random.insideUnitCircle * r;

        return true;
    }

    bool SenseObstacle(LayerMask mask, out Vector2 dir)
    {
        //sense 3 regions infront of the ant, the size of the regions depend on sight angle and sight radius
        float anglePart = obstacleAvoidAngle * 2.0f / 3.0f * Mathf.Deg2Rad;
        float alpha = anglePart / 2.0f;
        float r = (obstacleAvoidRadius * Mathf.Sin(alpha)) / (Mathf.Sin(alpha) + 1);
        float distance = obstacleAvoidRadius - r;

        // calculate 3 midpoints of the circles
        //mid point
        Vector3 dirMid = distance * transform.up;
        Vector3 circleMid = transform.position + dirMid;

        //left point
        Vector3 dirLeft = MathUtilities.RotateRound(distance * transform.up, Vector3.zero, new Vector3(0, 0, 1), anglePart * Mathf.Rad2Deg);
        Vector3 circleLeft = transform.position + dirLeft;

        //right point
        Vector3 dirRight = MathUtilities.RotateRound(distance * transform.up, Vector3.zero, new Vector3(0, 0, 1), -anglePart * Mathf.Rad2Deg);
        Vector3 circleRight = transform.position + dirRight;

        //Debug Draw
        //Debug.DrawCircle(circleMid, r, 20, Color.red);
        //Debug.DrawCircle(circleLeft, r, 20, Color.red);
        //Debug.DrawCircle(circleRight, r, 20, Color.red);
        Debug.DrawCircle(transform.position, sightRadius, 10, Color.green);

        //mid collider
        int midCollider = Physics2D.OverlapCircleAll(circleMid, r, mask).Length;
        //left collider
        int leftCollider = Physics2D.OverlapCircleAll(circleLeft, r, mask).Length;
        //right collider
        int rightCollider = Physics2D.OverlapCircleAll(circleRight, r, mask).Length;

        //default value
        dir = Vector2.zero;

        if (leftCollider == 0 && rightCollider == 0 && midCollider == 0)
            return false;

        if(midCollider > 0)
        {
            if (leftCollider > 0)
            {
                dir = new Vector2(dirRight.x, dirRight.y) + Random.insideUnitCircle * r;
                Debug.DrawCircle(circleRight, r, 20, Color.red);
            }
            else if ( rightCollider > 0)
            {
                dir = new Vector2(dirLeft.x, dirLeft.y) + Random.insideUnitCircle * r;
                Debug.DrawCircle(circleLeft, r, 20, Color.red);
            }
            else
            {
                dir = Random.value >= 0.5f? new Vector2(dirLeft.x, dirLeft.y) + Random.insideUnitCircle * r : new Vector2(dirRight.x, dirRight.y) + Random.insideUnitCircle * r;
            }
        }
        else if(leftCollider > 0)
        {
            dir = new Vector2(dirRight.x, dirRight.y) + Random.insideUnitCircle * r;
            Debug.DrawCircle(circleRight, r, 20, Color.red);
        }
        else if (rightCollider > 0)
        {
            dir = new Vector2(dirLeft.x, dirLeft.y) + Random.insideUnitCircle * r;
            Debug.DrawCircle(circleLeft, r, 20, Color.red);
        }
        return true;
    }

    private Vector2 RandomWalk()
    {

        if(Mathf.Abs(Vector2.Angle(transform.up,desiredDirection)) < wanderStrength)
        {
            float anglePart = sightAngle * 2.0f / 3.0f * Mathf.Deg2Rad;
            float alpha = anglePart / 2.0f;
            float r = (sightRadius * Mathf.Sin(alpha)) / (Mathf.Sin(alpha) + 1);
            float distance = sightRadius - r;

            Vector3 dirMid = distance * transform.up;

            Vector2  dir = new Vector2(dirMid.x, dirMid.y) + Random.insideUnitCircle;
            return dir;
        }
        return desiredDirection;
    }
}
