using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]

public class ZeroGMovement : MonoBehaviour
{
    [Header("=== Player Movement Settings ===")]
    [SerializeField] private float rollTorque = 1000f;
    [SerializeField] private float thrust = 100f;
    [SerializeField] private float upThrust = 50f;
    [SerializeField] private float strafeThrust = 50f;

    private Camera mainCam;
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    [Header("=== Boost Settings ===")]
    [SerializeField] private float maxBoostAmount = 2f;
    [SerializeField] private float boostDeprecationRate = 0.25f;
    [SerializeField] private float boostRechargeRate = 0.5f;
    [SerializeField] private float boostMultpiler = 5f;
    public bool boosting = false;
    public float currentBoostAmount;

    [SerializeField, Range(0.001f, 0.999f)] private float thrustGlideReduction = 0.999f;
    [SerializeField, Range(0.001f, 0.999f)] private float upDownGlideReduction = 0.111f;
    [SerializeField, Range(0.001f, 0.999f)] private float leftRightReduction = 0.111f;
    float glide = 0f, verticalGlide = 0f, horizontalGlide = 0f;

    Rigidbody rb;

    //Input Values
    private float thrust1D;
    private float upDown1D;
    private float strafe1D;
    private float roll1D;
    private Vector2 pitchYaw;
    public delegate void OnRequestShipEntry();
    public event OnRequestShipEntry onRequestShipEntry;
    public Spaceship ShipToEnter;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        currentBoostAmount = maxBoostAmount;
        ShipToEnter = null;
        if(playerCamera != null)
        {
            CameraSwitch.Register(playerCamera);
        }
        else
        {
            Debug.LogError("Player camera not assigned");
        }
    }

    private void OnDisable()
    {
        if (playerCamera != null)
        {
            CameraSwitch.Register(playerCamera);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(ShipToEnter != null && context.action.triggered)
        {
            EnterShip();
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        HandleBoosting();
    }

    void EnterShip()
    {
        transform.parent = ShipToEnter.transform;
        this.gameObject.SetActive(false);

        if(onRequestShipEntry != null)
        {
            onRequestShipEntry();
        }
    }
    
    void ExitShip()
    {
        transform.parent = null;
        this.gameObject.SetActive(true);
    }

    void HandleBoosting()
    {
        if (boosting && currentBoostAmount > 0f)
        {
            currentBoostAmount -= boostDeprecationRate;
            if (currentBoostAmount <= 0f)
            {
                boosting = false;
            }
        }
        else
        {
            if (currentBoostAmount < maxBoostAmount)
            {
                currentBoostAmount += boostRechargeRate;
            }
        }
    }

    void HandleMovement()
    {
        //rolling the spaceship
        rb.AddRelativeTorque(-mainCam.transform.forward * roll1D * rollTorque * Time.deltaTime);

        ////Pitch
        //rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);

        ////Yaw
        //rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.deltaTime);

        //Thrust
        if (thrust1D > 0.1f || thrust1D < -0.1f)
        {
            float currentThrust;

            if (boosting)
            {
                currentThrust = thrust * boostMultpiler;
            }
            else
                currentThrust = thrust;

            rb.AddRelativeForce(Vector3.forward * thrust1D * currentThrust * Time.deltaTime);
            glide = thrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.forward * glide * Time.deltaTime);
            glide *= thrustGlideReduction;
        }

        //UP/DOWN
        if (upDown1D > 0.1f || upDown1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.up * upDown1D * upThrust * Time.fixedDeltaTime);
            verticalGlide = upDown1D * upThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide *= upDownGlideReduction;
        }

        //Strafing
        if (strafe1D > 0.1f || strafe1D < -0.1f)
        {
            rb.AddForce(mainCam.transform.right * strafe1D * upThrust * Time.fixedDeltaTime);
            horizontalGlide = strafe1D * strafeThrust;
        }
        else
        {
            rb.AddForce(mainCam.transform.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightReduction;
        }
    }
    

    #region Input Methods

    public void onThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }

    public void onUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
    }

    //public void OnPitchYaw(InputAction.CallbackContext context)
    //{
    //    pitchYaw = context.ReadValue<Vector2>();
    //}

    public void OnBoost(InputAction.CallbackContext context)
    {
        boosting = context.performed;
    }

    #endregion
}

