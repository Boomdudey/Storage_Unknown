using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    #region Variables
    [SerializeField] public Transform player;

    [SerializeField] public float speed = 1f;
    [SerializeField] public float viewRange;
    [SerializeField] public float viewAngle = 60f;
    [SerializeField] public float timeBeforeChase = 2f;
    [SerializeField] public float visionTimer = 0f;

    [Header("Jumpscare Settings")]
    private bool isJumpscaring = false;
    private float swaySpeed = 8f;
    private float swayAmount = 10f;
    [SerializeField] private float jumpscareDuration = 0f;

    [SerializeField] public bool isChasing = false;

    [SerializeField] public AudioSource jumpScareAudio;
    [SerializeField] public AudioClip jumpScareClip;

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    private Vignette vignette;

    [Header("Hearing")]
    [SerializeField] private float hearingRange = 20f;

    [Header("AI Navigation")]
    private NavMeshAgent agent;
    [SerializeField] private float wanderRadius = 10f;
    private Vector3 wanderTarget;
    private List<Transform> allPositions = new List<Transform>();
    [SerializeField] private float waitTimeAtPoint = 1.0f;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private MeshRenderer meshRenderer;
    private bool hasScreamed = false;

    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out vignette);
        }

        // This is for the positions the enemy can wander to
        GameObject[] positionObjects = GameObject.FindGameObjectsWithTag("Position");
        foreach (GameObject obj in positionObjects)
        {
            allPositions.Add(obj.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isJumpscaring)
        {
            JumpScareTilt();
            jumpscareDuration += Time.deltaTime;
            if (jumpscareDuration >= 6f)
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
            if(hasScreamed == false)
            {
                jumpScareAudio.clip = jumpScareClip;
                jumpScareAudio.Play();
                hasScreamed = true;
            }
            return;
        }

        if (!isChasing)
        {
            DetectFootsteps();
        }

        if (CanSeePlayer())
        {
            // Stop movement while watching the player
            if (agent.enabled && agent.hasPath)
                agent.ResetPath();

            visionTimer += Time.deltaTime;
            float intensity = Mathf.Clamp01(visionTimer / timeBeforeChase - 0.2f);

            if (visionTimer >= timeBeforeChase)
            {
                isChasing = true;
            }

            if (vignette != null)
            {
                vignette.intensity.value = intensity;
            }

            return; // Exit early so Wander doesn't run
        }
        else
        {
            visionTimer = 0f;

            if (vignette != null)
            {
                vignette.intensity.value = 0f;
            }

            Wander(); // Wander when not seeing the player
        }
    }


    private void JumpScareTilt()
    {
        float tiltAngle = Mathf.Sin(Time.unscaledTime * swaySpeed) * swayAmount;
        transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, tiltAngle);
    }

    private bool CanSeePlayer()
    {
        Vector3 eyeLevel = transform.position + Vector3.up * 0.3f; // Enemy eye height
        Vector3 playerTarget = player.position + Vector3.up * 0.3f; // Adjust this to aim at the middle of the player's body/head

        Vector3 directionToPlayer = playerTarget - eyeLevel;

        // Flatten for angle check
        Vector3 flatDir = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z);
        float angle = Vector3.Angle(transform.forward, flatDir);

        if (directionToPlayer.magnitude <= viewRange && angle <= viewAngle)
        {
            Ray ray = new Ray(eyeLevel, directionToPlayer.normalized);
            Debug.DrawRay(ray.origin, ray.direction * viewRange, Color.red, 0f, true);

            if (Physics.Raycast(ray, out RaycastHit hit, viewRange))
            {
                if (hit.transform.CompareTag("Player"))
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

        if (meshRenderer != null && !meshRenderer.enabled)
        {
            meshRenderer.enabled = true; // Turn mesh back on
        }

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

        jumpScareAudio.clip = jumpScareClip;
        jumpScareAudio.Play();
    }

    #region AI Nav

    private void Wander()
    {
        // 1. If waiting after reaching a point
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                waitTimer = 0f;

                PickNewDestination();  // Now pick a new point
            }

            return;
        }

        // 2. If close to destination and not yet waiting
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
        }
    }
    private void PickNewDestination()
    {
        List<Transform> nearbyPoints = new List<Transform>();

        foreach (Transform t in allPositions)
        {
            if (Vector3.Distance(transform.position, t.position) <= wanderRadius)
            {
                nearbyPoints.Add(t);
            }
        }

        if (nearbyPoints.Count > 0)
        {
            wanderTarget = nearbyPoints[UnityEngine.Random.Range(0, nearbyPoints.Count)].position;
            agent.SetDestination(wanderTarget);
        }
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // Draw line to wander target
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawSphere(wanderTarget, 0.3f);
        }

        // Optional: visualize view cone
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector3 leftRay = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, viewAngle, 0) * transform.forward;

        Gizmos.DrawRay(transform.position + Vector3.up * 1.6f, leftRay * viewRange);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.6f, rightRay * viewRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }

    private void DetectFootsteps()
    {
        // Look for sounds within the hearing range
        Collider[] colliders = Physics.OverlapSphere(transform.position, hearingRange);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))  // Check if player is within range
            {
                // If the player's AudioSource is emitting sound, move towards the player
                AudioSource playerAudio = col.GetComponent<AudioSource>();
                if (playerAudio != null && playerAudio.isPlaying)
                {
                    MoveTowardsNoise(playerAudio.transform.position);
                    break;
                }
            }
        }
    }

    private void MoveTowardsNoise(Vector3 soundLocation)
    {
        // Move the enemy toward the noise source (the player's position)
        agent.SetDestination(soundLocation);
    }

    #endregion
}
