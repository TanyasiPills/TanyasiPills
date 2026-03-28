using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    [Header("Physics")]
    private Rigidbody rb;
    public Rigidbody leftLeg;
    public Rigidbody rightLeg;
    private Vector3 moving;
    private Vector3 velocity;
    private Vector3 slopeMovDir;
    public Vector3 direction;
    private bool isGrounded;


    [Header(header: "Movement Variables")]
    public float speed = 160f;
    public float speedMult = 35f;
    public float jumpHihi = 160f;
    public float jumpMult = 0.2f;
    public float drag = 6f;
    public float airDrag = 0.5f;
    public float lookSensitivity = 12f;
    public float lookSmoothSpeed;
    public float sinSpeed = 1;

    private RaycastHit hit;

    public float lookX = 0f;
    public float lookY = 0f;

    [Header(header: "Transforms")]
    public Transform groundCheck;
    public Transform playerCamera;
    public Transform orientation;

    [Header(header: "Layer")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private int playerLayer;

    float moveProgress = 0f;

    public void Start()
    {
        gameObject.tag = "Player";

        transform.gameObject.layer = playerLayer;
        LayerChild(transform, playerLayer);

        //playerCamera = GameObject.Find("cameraHolder").transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        jumpAction = InputSystem.actions.FindAction("Jump");
        jumpAction.performed += OnJump;

        jumpAction.actionMap.Enable();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        jumpAction.performed -= OnJump;
        jumpAction.actionMap.Disable();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHihi);
            rb.AddForce(velocity, ForceMode.Impulse);
        }
    }

    private bool OnSlope()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    private void Look()
    {
        //playerCamera.position = Vector3.Lerp(playerCamera.position, orientation.position, Time.deltaTime * lookSmoothSpeed);

        Vector2 lookdir = lookAction.ReadValue<Vector2>();
        lookX += lookdir.x * lookSensitivity * Time.deltaTime;
        lookY -= lookdir.y * lookSensitivity * Time.deltaTime;
        lookY = Mathf.Clamp(lookY, -80, 60);

        //playerCamera.localRotation = Quaternion.Euler(lookY, lookX, 0f);
        transform.rotation = Quaternion.Euler(0, lookX, transform.rotation.eulerAngles.z);
    }

    void FixedUpdate()
    {

        Vector2 movedir = moveAction.ReadValue<Vector2>();
        moving = transform.forward * movedir.y + transform.right * movedir.x;
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.3f, groundMask);

        if(movedir.magnitude > 0)
        {
            moveProgress += Time.fixedDeltaTime;
            float sin = Mathf.Sin(moveProgress * sinSpeed);
            transform.rotation = Quaternion.Euler(0, lookX, sin * 10);
            rightLeg.AddTorque(transform.right * 60 * sin);
            leftLeg.AddTorque(transform.right * 60 * -sin);
        } else{
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            moveProgress = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, lookX, 0), 5f * Time.deltaTime);
        }

        if (isGrounded && !OnSlope())
        {
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 0.3f, groundMask))
            {
                float dis = 0.3f - hit.distance;
                transform.position = new Vector3(transform.position.x, transform.position.y + dis, transform.position.z);
            }

            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.linearDamping = drag;
            rb.AddForce(moving.normalized * speed * speedMult * Time.deltaTime, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 0.3f, groundMask))
            {
                float dis = 0.3f - hit.distance;
                transform.position = new Vector3(transform.position.x, transform.position.y + dis, transform.position.z);
            }
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.linearDamping = drag;
            rb.AddForce(slopeMovDir.normalized * speed * speedMult * Time.deltaTime, ForceMode.Acceleration);
        }
        else
        {
            rb.linearDamping = airDrag;
            rb.AddForce(moving.normalized * speed * speedMult * jumpMult * Time.deltaTime, ForceMode.Acceleration);
        }

        Look();

        slopeMovDir = Vector3.ProjectOnPlane(moving, hit.normal).normalized;
    }

    private void LayerChild(Transform parent, int layer)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.layer = layer;
            LayerChild(child, layer);
        }
    }
}
