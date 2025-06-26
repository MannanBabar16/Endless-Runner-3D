using System.Collections;
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



    [Header("Power-Up Settings")]
    [SerializeField] private float invisibilityDuration = 5f;
    [SerializeField] private float invisibilitySpeedMultiplier = 1.5f;
    [SerializeField] private float flickerInterval = 0.1f;
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
    private float magnetTimer = 0f;

    private float originalHeight;
    private Vector3 originalCenter;
    private float slideHeight = 1.5f;

    private Renderer[] renderers;
    private TrailRenderer trail;
    private Coroutine invisibilityRoutine;
    
    public UiManager uiManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        targetPosition = transform.position;

        if (capsule)
        {
            originalHeight = capsule.height;
            originalCenter = capsule.center;
        }

        renderers = GetComponentsInChildren<Renderer>();
        trail = GetComponentInChildren<TrailRenderer>();
        if (trail) trail.enabled = false;
    }

    private void Update()
    {
        HandleTouchInput();

        float newX = Mathf.MoveTowards(transform.position.x, targetPosition.x, turnSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        float speed = isSliding ? slideSpeed : forwardSpeed;
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f) EndSlide();
        }

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
        if (collision.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    public void ActivateInvisibility()
    {
        if (isInvisible) return;

        if (invisibilityRoutine != null) StopCoroutine(invisibilityRoutine);
        invisibilityRoutine = StartCoroutine(InvisibilityFlicker());
    }

    private IEnumerator InvisibilityFlicker()
    {
        isInvisible = true;
        float timer = 0f;
        bool visible = false;

        if (trail) trail.enabled = true;

        forwardSpeed *= invisibilitySpeedMultiplier;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Obstacle"), true);
        if (uiManager) uiManager.ActivateInvisibility(invisibilityDuration);

        while (timer < invisibilityDuration)
        {
            foreach (Renderer r in renderers)
            {
                if (r) r.enabled = visible;
            }
            visible = !visible;

            timer += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        foreach (Renderer r in renderers)
        {
            if (r) r.enabled = true;
        }

        if (trail) trail.enabled = false;

        forwardSpeed /= invisibilitySpeedMultiplier;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Obstacle"), false);

        isInvisible = false;
        

    }

    public void ActivateMagnet()
    {
        isMagnetActive = true;
        magnetTimer = magnetDuration;
        
        if (uiManager) uiManager.ActivateMagnet(magnetDuration);
    }

    private void AttractCoins()
    {
        Collider[] coins = Physics.OverlapSphere(transform.position, magnetRadius, coinLayer);
        foreach (Collider coin in coins)
        {
            Transform t = coin.transform;
            t.position = Vector3.MoveTowards(t.position, transform.position, 40f * Time.deltaTime);
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
