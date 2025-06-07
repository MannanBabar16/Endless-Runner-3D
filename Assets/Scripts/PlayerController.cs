using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider capsule;

    [Header("Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 1f;

    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float turnOffset = 3f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnPosition;

    [Header("Power-Up Settings")]
    [SerializeField] private float invisibilityDuration = 5f;
    [SerializeField] private float invisibilitySpeedMultiplier = 1.5f;
    [SerializeField] private float magnetDuration = 5f;
    [SerializeField] private float magnetRadius = 5f;
    [SerializeField] private LayerMask coinLayer;

    private Vector2 startTouchPosition;
    private Vector3 targetPosition;
    private bool isGrounded = false;
    private bool isSliding = false;
    private float slideTimer = 0f;

    private bool isInvisible = false;
    private bool isMagnetActive = false;
    private float invisibilityTimer = 0f;
    private float magnetTimer = 0f;

    // Cached original values
    private float originalHeight;
    private Vector3 originalCenter;
    private float slideHeight = 1f;

    // Invisibility
    private Material playerMaterial;
    private Color originalColor;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        transform.position = spawnPosition;
        targetPosition = transform.position;

        if (capsule)
        {
            originalHeight = capsule.height;
            originalCenter = capsule.center;
        }

        playerMaterial = GetComponentInChildren<Renderer>().material;
        originalColor = playerMaterial.color;
    }

    private void Update()
    {
        HandleTouchInput();

        // Smooth lane switching (X-axis)
        float newX = Mathf.MoveTowards(transform.position.x, targetPosition.x, turnSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Forward movement (slide speed when sliding)
        float speed = isSliding ? slideSpeed : forwardSpeed;
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);

        // Slide timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f) EndSlide();
        }

        // Invisibility timer
        if (isInvisible)
        {
            invisibilityTimer -= Time.deltaTime;
            if (invisibilityTimer <= 0f) EndInvisibility();
        }

        // Magnet timer
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;
            if (magnetTimer <= 0f) isMagnetActive = false;
            else AttractCoins();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startTouchPosition = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 delta = touch.position - startTouchPosition;

            if (delta.magnitude > swipeThreshold)
            {
                bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

                if (isHorizontal)
                {
                    float x = targetPosition.x;
                    if (delta.x > 0 && x < turnOffset) targetPosition.x += turnOffset;
                    else if (delta.x < 0 && x > -turnOffset) targetPosition.x -= turnOffset;
                }
                else
                {
                    if (delta.y > 0 && isGrounded)
                    {
                        Jump();
                    }
                    else if (delta.y < 0)
                    {
                        StartSlide();
                    }
                }
            }
        }
    }

    private void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;
        slideTimer = slideDuration;

        if (!isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.down * 20f, ForceMode.Impulse);
        }

        if (capsule)
        {
            float heightDiff = originalHeight - slideHeight;
            capsule.height = slideHeight;
            capsule.center = originalCenter - new Vector3(0, heightDiff / 2f, 0);
        }
    }

    private void EndSlide()
    {
        isSliding = false;

        if (capsule)
        {
            capsule.height = originalHeight;
            capsule.center = originalCenter;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    public void ActivateInvisibility()
    {
        if (isInvisible) return;

        isInvisible = true;
        invisibilityTimer = invisibilityDuration;

        Color c = playerMaterial.color;
        c.a = 0.3f;
        playerMaterial.color = c;

        forwardSpeed *= invisibilitySpeedMultiplier;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Obstacle"), true);
    }

    private void EndInvisibility()
    {
        isInvisible = false;

        playerMaterial.color = originalColor;
        forwardSpeed /= invisibilitySpeedMultiplier;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Obstacle"), false);
    }

    public void ActivateMagnet()
    {
        isMagnetActive = true;
        magnetTimer = magnetDuration;
    }

    private void AttractCoins()
    {
        Collider[] coins = Physics.OverlapSphere(transform.position, magnetRadius, coinLayer);
        foreach (Collider coin in coins)
        {
            Transform t = coin.transform;
            t.position = Vector3.MoveTowards(t.position, transform.position, 10f * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }

    private void OnDrawGizmos()
    {
        if (capsule == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
    }
}
