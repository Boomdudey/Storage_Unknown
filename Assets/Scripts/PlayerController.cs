using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;
    private Vector3 crouchPosition = new Vector3(0, .29f, .101f);
    [SerializeField] public Rigidbody rb;
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float runSpeed = 10f;
    [SerializeField] public float jumpPower = 7f;
    [SerializeField] public float gravityStrength = 10f;
    [SerializeField] private float currentSpeed = 5f;
    [SerializeField] private float tiredSpeed = 2.5f;
    [SerializeField] private bool isTired;

    [Header("Crouch Parameters")]
    [SerializeField] public bool isCrouched;
    [SerializeField] private bool isNotCrouched;
    [SerializeField] public float crouchSpeed = 2f;
    [SerializeField] public float crouchYScale;
    private float startYScale;

    [Header("Stamina Parameters")]
    [SerializeField] public float playerCurrentStamina = 100.0f;
    [SerializeField] private float maxStamina = 100.0f;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 5f;
    [SerializeField] private bool isRunning;
    [SerializeField] private Image staminaBar;
    [SerializeField] private CanvasGroup staminaBarCanvas;

    [SerializeField] public float lookSens = 2f;
    public float lookXLimit = 45f;

    [SerializeField]Vector3 moveDirection = Vector3.zero;
    Vector3 forward;
    Vector3 right;
    float rotationX = 0f;

    CharacterController characterController;

    private void Awake()
    {
        // gets character controller
        characterController = GetComponent<CharacterController>();
        // makes it so Cursor doesn't move off screen
        Cursor.lockState = CursorLockMode.Locked;
        // hides cursor
        Cursor.visible = false;
        currentSpeed = 5f;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Stamina bar is faded out since on start you won't be sprinting the frame you load in
        staminaBarCanvas.alpha = 0f;
        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        StaminaBarUpdate(1);


        // Run speed is 10f so as long as currentSpeed is less then 10f AND playerCurrentStamina is less then full max stamina
        // you will begin the StartRegenStamina function
        if (currentSpeed < 10f && playerCurrentStamina <= maxStamina - .01)
        {
            StartStaminaRegen();
        }
        // When sprinting, which turns currentSpeed to 10, and playerCurrentStamina is greater than zero
        // as well as when moveDirection vector3, does not equal (0,0,0), stamina will drain
        else if (currentSpeed == 10f && playerCurrentStamina >= 0 && moveDirection != Vector3.zero)
        {
             StartStaminaDrain();
        }

        if (playerCurrentStamina <= 0)
        {
            isTired = true;
            currentSpeed = tiredSpeed;
            staminaBar.color = Color.red;
            staminaBarCanvas.alpha = 1.0f;
        }

        if(playerCurrentStamina >= 50 && isTired)
        {
            isTired = false;
            currentSpeed = 5f;
            staminaBar.color = Color.white;
        }
    }

    #region Player Movement
    private void HandleMovement()
    {
        forward = transform.TransformDirection(Vector3.forward);
        right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouched = true;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            isCrouched = false;

        }

        if(isCrouched == true)
        {
            Crouch();
            currentSpeed = crouchSpeed;
        }
        else
        {
            StandUp();
            if(isRunning == true && isTired == false)
            {
                currentSpeed = runSpeed;
            }
        }
        
        // Resets everything to walking speed
        if(isRunning == false && isCrouched == false && isTired == false)
        {
            currentSpeed = walkSpeed;
        }

        float curSpeedX = currentSpeed * Input.GetAxis("Vertical");
        float curSpeedY = currentSpeed * Input.GetAxis("Horizontal");

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = characterController.velocity.y;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleRotation()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSens;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        float rotationY = Input.GetAxis("Mouse X") * lookSens;
        transform.rotation *= Quaternion.Euler(0, rotationY, 0);
    }

    private void Crouch()
    {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StandUp()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }
    #endregion

    #region Player Stamina

    private void StartStaminaRegen()
    {
        playerCurrentStamina += staminaRegen * Time.deltaTime;
        // has stamina bar fade out
        if(isTired == false)
        {
            staminaBarCanvas.alpha -= 1f * Time.deltaTime;
        }
    }

    private void StartStaminaDrain()
    {
        playerCurrentStamina -= staminaDrain * Time.deltaTime;
        // when using stamina has stamina bar shown
        staminaBarCanvas.alpha = 1.0f;
    }

    private void StaminaBarUpdate(int value)
    {
        // updates the stamina bar to fill depending on the player's current stamina in comparison to the max stamina
        staminaBar.fillAmount = playerCurrentStamina / maxStamina;
    }
    #endregion
}
