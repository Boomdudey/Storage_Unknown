using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    public Camera playerCamera;
    private Vector3 crouchPosition = new Vector3(0, .29f, .101f);
    [SerializeField] public Rigidbody rb;
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] public float runSpeed = 10f;
    [SerializeField] public float gravityStrength = 10f;
    [SerializeField] private float currentSpeed = 5f;
    [SerializeField] private float tiredSpeed = 2.5f;
    [SerializeField] public bool isTired;

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
    [SerializeField] public bool isRunning;
    [SerializeField] private Image staminaBar;
    [SerializeField] private CanvasGroup staminaBarCanvas;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource playerSFXSource;
    [SerializeField] private AudioClip footstepSFX;

    [SerializeField] public float lookSens = 2f;
    public float lookXLimit = 45f;

    [SerializeField] Vector3 moveDirection = Vector3.zero;
    Vector3 forward;
    Vector3 right;
    float rotationX = 0f;

    CharacterController characterController;
    #endregion

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

        // Start regenerating stamina when not sprinting and stamina is not full
        if (currentSpeed < runSpeed && playerCurrentStamina <= maxStamina - 0.01f)
        {
            StartStaminaRegen();
        }
        // Drain stamina only if the player is running and moving
        else if (
            currentSpeed == runSpeed &&
            playerCurrentStamina > 0 &&
            (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        )
        {
            StartStaminaDrain();
        }

        // Handle tired state when stamina is empty
        if (playerCurrentStamina <= 0)
        {
            isTired = true;
            currentSpeed = tiredSpeed;
            staminaBar.color = Color.red;
            staminaBarCanvas.alpha = 1.0f;
        }

        // Recover from tired state when stamina is half full
        if (playerCurrentStamina >= 50 && isTired)
        {
            isTired = false;
            currentSpeed = walkSpeed;
            staminaBar.color = Color.white;
        }
    }


    #region Player Movement
    private void HandleMovement()
    {
        forward = transform.TransformDirection(Vector3.forward);
        right = transform.TransformDirection(Vector3.right);

        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouched = true;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            isCrouched = false;

        }

        if (isCrouched == true)
        {
            Crouch();
            currentSpeed = crouchSpeed;
        }
        else
        {
            StandUp();
            if (isRunning == true && isTired == false)
            {
                currentSpeed = runSpeed;
            }
        }

        // Resets everything to walking speed
        if (isRunning == false && isCrouched == false && isTired == false)
        {
            currentSpeed = walkSpeed;
        }

        float curSpeedX = currentSpeed * Input.GetAxis("Vertical");
        float curSpeedY = currentSpeed * Input.GetAxis("Horizontal");

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = -2f;

        bool isMoving = characterController.isGrounded && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);

        if (isMoving)
        {
            PlayFootstepSound(isCrouched, isRunning);
        }
        else
        {
            StopFootstepSound();
        }

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
        if (isTired == false)
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

    #region Sound Effects
    private void PlayFootstepSound(bool isCrouching, bool isRunning)
    {
        float targetPitch = 1f;
        float targetVolume = 1f;

        if (isCrouching || isTired)
        {
            targetPitch = 0.6f;
            targetVolume = 0.5f;
        }
        else if (isRunning && !isTired)
        {
            targetPitch = 1.4f;
            targetVolume = 1.4f;
        }

        if (!playerSFXSource.isPlaying)
        {
            playerSFXSource.clip = footstepSFX;
            playerSFXSource.loop = true;
            playerSFXSource.pitch = targetPitch;
            playerSFXSource.volume = targetVolume;
            playerSFXSource.Play();
        }
        else
        {
            if (playerSFXSource.pitch != targetPitch)
            {
                playerSFXSource.pitch = targetPitch;
            }
            if (playerSFXSource.volume != targetVolume)
            {
                playerSFXSource.volume = targetVolume;
            }
        }
    }

    private void StopFootstepSound()
    {
        if (playerSFXSource.isPlaying)
        {
            playerSFXSource.Stop();
        }
    }

    #endregion
}
