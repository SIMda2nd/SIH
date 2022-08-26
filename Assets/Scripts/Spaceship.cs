using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


[RequireComponent (typeof(Rigidbody))]
public class Spaceship : MonoBehaviour
{

    [Header("=== Ship Movement Settings ===")]
    [SerializeField] private float yawTorque = 500f;
    [SerializeField] private float pitchTorque = 1000f;
    [SerializeField] private float rollTorque = 1000f;
    [SerializeField] private float thrust = 100f;
    [SerializeField] private float upThrust = 50f;
    [SerializeField] private float strafeThrust = 50f;

    [Header("=== Boost Settings ===")]
    [SerializeField] private float maxBoostAmount = 2f;
    [SerializeField] private float boostDeprecationRate = 0.25f;
    [SerializeField] private float boostRechargeRate = 0.5f;
    [SerializeField] private float boostMultpiler = 5f;
    public bool boosting = false;
    public float currentBoostAmount;

    [SerializeField] private CinemachineVirtualCamera shipCam;

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

    private bool isOccupied = false;

    private ZeroGMovement player;

    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentBoostAmount = maxBoostAmount;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<ZeroGMovement>();
        if(player != null)
        {
            print("Player Found");
        }
        player.onRequestShipEntry += PlayerEnteredShip;
        
    }
    private void onEnable()
    {
        if (shipCam != null)
        {
            CameraSwitch.Register(shipCam);
        }
        else
        {
            Debug.LogError("Ship camera not assigned");
        }
    }

    private void OnDisable()
    {
        if(shipCam != null)
        {
            CameraSwitch.Register(shipCam);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isOccupied)
        {
            HandleMovement();
            HandleBoosting();

        }
        
    }

    void HandleBoosting()
    {
        if ( boosting && currentBoostAmount > 0f)
        {
            currentBoostAmount -= boostDeprecationRate;
            if(currentBoostAmount <= 0f)
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
        rb.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.deltaTime);

        //Pitch
        rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);

        //Yaw
        rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.deltaTime);

        //Thrust
        if(thrust1D > 0.1f || thrust1D < -0.1f)
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
        if(upDown1D > 0.1f || upDown1D < -0.1f)
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
        if(strafe1D > 0.1f || strafe1D < -0.1f)
        {
            rb.AddRelativeForce(Vector3.right * strafe1D * upThrust * Time.fixedDeltaTime);
            horizontalGlide = strafe1D * strafeThrust;
        }
        else
        {
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightReduction;
        }
    }

    void PlayerEnteredShip()
    {
        rb.isKinematic = false;
        CameraSwitch.SwitchCamera(shipCam);
        isOccupied = true;
    }

    void PlayerExitedShip()
    {
        rb.isKinematic = true;
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

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        pitchYaw = context.ReadValue<Vector2>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        boosting = context.performed;
    }

    #endregion
}
