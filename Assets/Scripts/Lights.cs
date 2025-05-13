using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    [SerializeField] Light pointLight;
    [SerializeField] float lightActiveTime;
    private bool triggerActive = false;
    private bool startTimer = false;
    [SerializeField] float timer;

    private PlayerController playerController;

    private void Start()
    {
        pointLight.enabled = false;
        timer = lightActiveTime;

        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
        {
        // Check if player and crouching
        if (other.CompareTag("Player") && playerController != null && playerController.isCrouched)
        {
            return; // Do not activate light if player is crouched
        }

        // If collider is player or enemy
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            triggerActive = true;
            startTimer = false;
            pointLight.enabled = true;
            timer = lightActiveTime;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            triggerActive = false;
            startTimer = true;
        }
    }

    private void Update()
    {
        if (startTimer && !triggerActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                pointLight.enabled = false;
                timer = lightActiveTime;
                startTimer = false;
            }
        }
    }
}
