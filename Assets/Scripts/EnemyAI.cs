using System;
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

    [SerializeField] public AudioSource jumpScareAudio;
    [SerializeField] public AudioClip jumpScareClip;

    [Header("Post-Processing")]
    [SerializeField] private PostProcessVolume postProcessVolume;
    private Vignette vignette;

    [Header("AI Navigation")]
    private NavMeshAgent agent;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;
    private float wanderCooldown;
    private Vector3 wanderTarget;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out vignette);
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
            return;
        }

        if (CanSeePlayer())
        {
            visionTimer += Time.deltaTime;
            float intensity = Mathf.Clamp01(visionTimer / timeBeforeChase - .2f);

            if (visionTimer >= timeBeforeChase)
            {
                isChasing = true;
            }

            if (vignette != null)
            {
                vignette.intensity.value = intensity;
            }
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
        Vector3 eyeLevel = transform.position + Vector3.up * 1.6f; // Enemy eye height
        Vector3 playerTarget = player.position + Vector3.up * 0.9f; // Adjust this to aim at the middle of the player's body/head

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
        wanderCooldown += Time.deltaTime;

        if (wanderCooldown >= wanderTimer || Vector3.Distance(transform.position, wanderTarget) < 1f)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                agent.SetDestination(wanderTarget);
            }

            wanderCooldown = 0f;
        }
    }

    #endregion
}
