using System;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    #region Variables
    [SerializeField] public Transform player;

    [SerializeField] public float speed = 1f;
    [SerializeField] public float viewRange = 15f;
    [SerializeField] public float viewAngle = 60f;
    [SerializeField] public float timeBeforeChase = 2f;
    [SerializeField] public float visionTimer = 0f;

    [Header("Jumpscare Settings")]
    private bool isJumpscaring = false;
    private float swaySpeed = 8f;
    private float swayAmount = 10f;
    [SerializeField] private float jumpscareDuration = 0f;

    [SerializeField] public bool isChasing = false;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isJumpscaring)
        {
            JumpScareTilt();
            jumpscareDuration += Time.deltaTime;
            if(jumpscareDuration >= 6f)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneManager.LoadScene("MainMenu");
            }
            return;
        }

        if (isChasing)
        {
            BeginChase();
            return;
        }

        if (CanSeePlayer())
        {
            visionTimer += Time.deltaTime;
            if (visionTimer >= timeBeforeChase)
            {
                isChasing = true;
            }
        }
        else
        {
            visionTimer = 0f;
        }
    }

    private void JumpScareTilt()
    {
        float tiltAngle = Mathf.Sin(Time.unscaledTime * swaySpeed) * swayAmount;
        transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, tiltAngle);
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if(directionToPlayer.magnitude <= viewRange && angle <= viewAngle)
        {
            Ray ray = new Ray(transform.position + Vector3.up, directionToPlayer.normalized);

            if(Physics.Raycast(ray, out RaycastHit hit, viewRange))
            {
                if(hit.transform == player)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void BeginChase()
    {
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < 2f)
        {
            JumpScare();
        }
    }

    private void JumpScare()
    {
        isJumpscaring = true;
        // Stop enemy movement
        isChasing = false;

        // Disable player movement
        var playerScript = player.GetComponent<PlayerController>();
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

        Vector3 forwardOffset = player.forward * 1.5f;
        Vector3 targetPosition = player.position + forwardOffset;
        targetPosition.y = player.position.y;
        transform.position = targetPosition;
        transform.LookAt(player);
    }
}
