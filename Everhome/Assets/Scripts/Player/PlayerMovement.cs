using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : NetworkBehaviour
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
    private Vector3 slopeMovDir = Vector3.up;
    public Vector3 direction;
    private bool isGrounded;


    [Header(header: "Movement Variables")]
    public float speed = 160f;
    public float speedMult = 35f;
    public float jumpHihi = 160f;
    public float jumpMult = 0.2f;
    public float drag = 6f;
    public float airDrag = 0.5f;
    public float sinSpeed = 1;

    private RaycastHit hit;

    [Header(header: "Transforms")]
    public Transform groundCheck;
    public Transform torso;
    public Transform LTP;
    public Transform RTP;
    public CinemachineOrbitalFollow orbit;
    public GameObject camera;
    public GameObject trackingPoint;
    public Transform mainBody;

    [Header(header: "Layer")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] public LayerMask layerMask;

    float moveProgress = 0f;

    bool canPickupLeft = false;
    bool canPickupRight = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) {
            AudioPlayer ap = GameObject.FindGameObjectWithTag("Manager").GetComponent<AudioPlayer>();
            NetworkObject netObj = GetComponent<NetworkObject>();
            ap.AddPlayer(netObj);
            Debug.Log($"yo {netObj.OwnerClientId}");
            GetComponent<SpeakerBody>().ap = ap.APS[netObj.OwnerClientId];
            return;
        }

        GameObject.FindGameObjectWithTag("Recorder").GetComponent<AudioRecord>().player = GetComponent<NetworkObject>();
        StartCoroutine(GameObject.FindGameObjectWithTag("Godcam").GetComponent<GodCam>().MoveCamera(camera.transform, this));

        gameObject.tag = "Player";

        transform.gameObject.layer = playerLayer;
        LayerChild(transform, playerLayer);

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
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

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        jumpAction.performed -= OnJump;
        jumpAction.actionMap.Disable();
    }

    public void OnGodCamFinish()
    {
        orbit.gameObject.SetActive(true);
        camera.SetActive(true);
        trackingPoint.SetActive(true);
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

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.2f))
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
        Vector3 forward = camera.transform.forward;
        Vector3 full = forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward);

            float sin = Mathf.Sin(moveProgress * sinSpeed);
            Quaternion sway = Quaternion.AngleAxis(-sin * 5f, transform.up);
            Quaternion sway2 = Quaternion.AngleAxis(-sin * 8f, Vector3.forward);

            Quaternion finalRot = targetRot * sway * sway2;

            transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * 10f);

            targetRot = Quaternion.LookRotation(full);
            torso.rotation = Quaternion.Slerp(torso.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 movedir = moveAction.ReadValue<Vector2>();
        moving = transform.forward * movedir.y + transform.right * movedir.x;
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.001f, groundMask);

        ArmManagement();

        if (movedir.magnitude > 0)
        {
            moveProgress += Time.fixedDeltaTime;
            float sin = Mathf.Sin(moveProgress * sinSpeed);
            rightLeg.AddTorque(transform.right * 100 * sin);
            leftLeg.AddTorque(transform.right * 100 * -sin);

            Debug.DrawLine(transform.position, transform.position + transform.forward);

            //Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            //Quaternion sway = Quaternion.AngleAxis(-sin * 10, transform.forward);

            //transform.rotation = yRot * sway;

        } else{
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            moveProgress = 0;

            //Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            //transform.rotation = Quaternion.Lerp(transform.rotation, yRot * originRot, 10 * Time.deltaTime);
        }

        if (isGrounded && !OnSlope())
        {
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.linearDamping = drag;
            rb.AddForce(moving.normalized * speed * speedMult * Time.deltaTime, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
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

        float leftDegree = Vector3.Dot(LTP.forward, leftArm.transform.forward);
        float rightDegree = Vector3.Dot(RTP.forward, rightArm.transform.forward);

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
        if (!IsOwner) return;

        if (leftHeld == other || rightHeld == other) return;

        if ((hand.tag == "left" && canPickupLeft && leftHeld == null) || (hand.tag == "right") && canPickupRight && rightHeld == null)
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
