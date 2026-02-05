using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotion1 : MonoBehaviour
{
    #region Varibales

    public float baseSpeed = 500f;
    private float baseFOV;
    public float sprintSpeedModifier;
    private float sprintFOVModefier = 1.5f;
    public float jumpForce;

    public Camera normalCam;
    private Rigidbody rb;
    public Transform weaponParent;
    private Vector3 weaponParentOrigin;
    private Vector3 targetWeaponBobPosition;

    private float movementCounter;
    private float idleCounter;

    public Transform groundDetector;
    public LayerMask ground;

    private float horizontalMove;
    private float verticalMove;
    private bool sprint, isSprinting;
    private bool jump, canJump;
    private bool isGrounded;

    #endregion

    #region MonoBehavior Callbacks

    void Start()
    {
        Camera.main.enabled = false;
        rb = GetComponent<Rigidbody>();
        baseFOV = normalCam.fieldOfView;
        weaponParentOrigin = weaponParent.localPosition;
    }

    private void Update()
    {
        // Axles
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");

        // Controls
        sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        jump = Input.GetKeyDown(KeyCode.Space);

        // States
        isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        canJump = jump && isGrounded;
        isSprinting = sprint && (verticalMove > 0) && !canJump && isGrounded;
    }

    private void FixedUpdate()
    {
        // Movement
        Vector3 t_direction = new Vector3(horizontalMove, 0, verticalMove);
        t_direction.Normalize();

        // Jumping
        if (canJump)
        {
            Debug.Log("Jump!");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        float actualSpeed = baseSpeed;

        if (isSprinting)
        {
            actualSpeed *= sprintSpeedModifier;
            // FOV
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModefier, Time.deltaTime * 8f);
        }
        else
        {
            // FOV
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
        }

        Vector3 targetVelocity = transform.TransformDirection(t_direction) * actualSpeed * Time.deltaTime;
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;

        // Head Bob
        if (horizontalMove == 0 && verticalMove == 0)
        {
            HeadBob(idleCounter, 0.025f, 0.025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }
        else if (!isSprinting)
        {
            HeadBob(movementCounter, 0.05f, 0.035f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
        }
        else
        {
            HeadBob(movementCounter, 0.15f, 0.075f);
            movementCounter += Time.deltaTime * 7f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
        }
    }

    #endregion

    #region Private Methods

    void HeadBob(float p_z, float p_xIntensity, float p_yIntensity)
    {
        targetWeaponBobPosition = weaponParentOrigin + new Vector3 (Mathf.Cos(p_z) * p_xIntensity, Mathf.Sin(p_z * 2) * p_yIntensity, 0);
    }

    #endregion
}
