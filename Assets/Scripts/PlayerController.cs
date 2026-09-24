using UnityEngine;

/// <summary>
/// Player sphere movement (WASD/Arrows) via Rigidbody forces.
/// Each new control-key press shrinks mass and scale a little.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveForce = 18f;
    [SerializeField] float maxSpeed = 8f;

    [Header("Shrink on input (Spec 8)")]
    [SerializeField] float shrinkScaleFactor = 0.97f;
    [SerializeField] float shrinkMassFactor = 0.97f;
    [SerializeField] float minScale = 0.35f;
    [SerializeField] float minMass = 0.35f;

    Rigidbody rb;
    bool wasPressingMove;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        Vector2 input = ReadMoveInput();
        bool pressing = input.sqrMagnitude > 0.01f;

        // Spec 8: shrink once when a control key is newly pressed (not every frame while held)
        if (pressing && !wasPressingMove)
        {
            ApplyShrinkOnInput();
        }

        wasPressingMove = pressing;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude < 0.01f)
            return;

        Vector3 force = new Vector3(input.x, 0f, input.y) * moveForce;
        rb.AddForce(force, ForceMode.Force);

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            flatVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
        }
    }

    static Vector2 ReadMoveInput()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;

        Vector2 v = new Vector2(x, z);
        return v.sqrMagnitude > 1f ? v.normalized : v;
    }

    void ApplyShrinkOnInput()
    {
        Vector3 scale = transform.localScale * shrinkScaleFactor;
        float s = Mathf.Max(minScale, scale.x);
        transform.localScale = Vector3.one * s;

        rb.mass = Mathf.Max(minMass, rb.mass * shrinkMassFactor);
    }

    public void Grow(float scaleMultiplier, float massMultiplier)
    {
        transform.localScale *= scaleMultiplier;
        rb.mass *= massMultiplier;
    }
}
