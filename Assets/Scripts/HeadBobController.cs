using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    #region Variables
    [Header("Head Bob Parameters")]
    [SerializeField] private bool headBobEnabled = true;

    [SerializeField, Range(0, 0.1f)] private float headBobAmp = 0.015f;
    [SerializeField, Range(0, 30)] private float headBobFreq = 10.0f;
    [SerializeField] private float startHeadBobFreq;

    [SerializeField] private Transform playerCamera = null;
    [SerializeField] private Transform cameraPivot = null;

    private float toggleSpeed = 3.0f;
    private Vector3 camStartPos;
    private CharacterController cController;
    private PlayerController playerController;

    #endregion

    void Awake()
    {
        // Get the CharacterController component attached to the player
        cController = GetComponent<CharacterController>();
        playerController = FindFirstObjectByType<PlayerController>();
        // Store the initial position of the camera
        camStartPos = playerCamera.localPosition;
        startHeadBobFreq = headBobFreq;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!headBobEnabled)
            return;

        if (playerController.isCrouched || playerController.isTired)
        {
            headBobFreq = startHeadBobFreq * 0.6f;
        }
        else if (playerController.isRunning)
        {
            headBobFreq = startHeadBobFreq * 1.4f;
        }
        else
        {
            headBobFreq = startHeadBobFreq;
        }

        UpdateMotion();
        ResetCameraPosition();
        playerCamera.LookAt(FocusTarget());
    }

    private void PlayMotion(Vector3 motion)
    {
        playerCamera.localPosition += motion;
    }

    private Vector3 StepMotion()
    {
        Vector3 position = Vector3.zero;
        position.y = Mathf.Sin(Time.time * headBobFreq) * headBobAmp;
        position.x = Mathf.Sin(Time.time * headBobFreq/2) * headBobAmp * 2;
        return position;
    }

    private void UpdateMotion()
    {
        float playerSpeed = new Vector3(cController.velocity.x, 0, cController.velocity.z).magnitude;

        float dynamicToggleSpeed = toggleSpeed;

        if (playerController.isCrouched)
            dynamicToggleSpeed = 0.5f; // much lower threshold for crouch bobbing

        bool isMoving = playerSpeed > 0.1f;
        if (!isMoving)
        {
            return;
        }

        PlayMotion(StepMotion());
    }

    private void ResetCameraPosition()
    {
        if(playerCamera.localPosition == camStartPos)
        {
            return;
        }

        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, camStartPos, Time.deltaTime * 1) ;
    }

    private Vector3 FocusTarget()
    {
        Vector3 position = new Vector3(transform.position.x, transform.position.y + cameraPivot.localPosition.y, transform.position.z);
        position += cameraPivot.forward * 15.0f;
        return position;
    }
}
