using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction leftArmAction;
    private InputAction rightArmAction;

    [Header("Physics")]
    private Rigidbody rb;
    public Rigidbody leftLeg;
    public Rigidbody rightLeg;
    public Rigidbody leftArm;
    public Rigidbody rightArm;
    public GameObject leftHeld;
    public GameObject rightHeld;
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
    [SerializeField] public LayerMask layerMask;

    float moveProgress = 0f;

    bool canPickupLeft = false;
    bool canPickupRight = false;

    public void Start()
    {
        gameObject.tag = "Player";

        transform.gameObject.layer = playerLayer;
        LayerChild(transform, playerLayer);

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        jumpAction = InputSystem.actions.FindAction("Jump");
        jumpAction.performed += OnJump;

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");

        leftArmAction = InputSystem.actions.FindAction("Lefthand");
        rightArmAction = InputSystem.actions.FindAction("Righthand");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputSystem.actions.FindActionMap("Player").Enable();
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

        ArmManagement();

        if (movedir.magnitude > 0)
        {
            moveProgress += Time.fixedDeltaTime;
            float sin = Mathf.Sin(moveProgress * sinSpeed);
            transform.rotation = Quaternion.Euler(0, lookX, sin * 6);
            rightLeg.AddTorque(transform.right * 100 * sin);
            leftLeg.AddTorque(transform.right * 100 * -sin);
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

    private void ArmManagement()
    {
        float left = leftArmAction.ReadValue<float>();
        float right = rightArmAction.ReadValue<float>();

        canPickupLeft = left > 0;
        canPickupRight = right > 0;

        float leftDegree = Vector3.Dot(transform.forward, leftArm.transform.forward);
        float rightDegree = Vector3.Dot(transform.forward, rightArm.transform.forward);

        leftArm.AddTorque(leftArm.transform.right * 80 * left * leftDegree);
        rightArm.AddTorque(rightArm.transform.right * 80 * right * rightDegree);

        if(leftHeld != null && !canPickupLeft)
        {
            Destroy(leftHeld.GetComponent<ConfigurableJoint>());
            leftHeld = null;
        }

        if (rightHeld != null && !canPickupRight)
        {
            Destroy(rightHeld.GetComponent<ConfigurableJoint>());
            rightHeld = null;
        }
    }

    public void Pickup(GameObject other, GameObject hand, Vector3 closest)
    {
        if((hand.tag == "left" && canPickupLeft && leftHeld == null) || (hand.tag == "right") && canPickupRight && rightHeld == null)
        {
            ConfigurableJoint joint = other.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = hand.GetComponent<Rigidbody>();

            joint.anchor = closest;
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive
            {
                positionSpring = 2000f,
                positionDamper = 200f,
                maximumForce = Mathf.Infinity
            };

            joint.xDrive = joint.yDrive = joint.zDrive = drive;

            if (hand.tag == "left") leftHeld = other;
            else rightHeld = other;
        }
    }
}
