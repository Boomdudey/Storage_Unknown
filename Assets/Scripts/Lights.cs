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
        if(playerController.isCrouched == true)
        {
            return; // Prevents the light from turning on if the player is crouched
        }
        // Debug.Log("Collider entered the trigger");
        triggerActive = true;
        startTimer = false; // Stop the timer when entering the trigger
        pointLight.enabled = true; // Turns on the light when a collider enter
        timer = lightActiveTime;  // Reset the timer when the collider enters the trigger
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log("Collider exited the trigger");
        triggerActive = false;
        startTimer = true; // Start the timer when exiting the trigger
    }
    
    private void Update()
    {
        if (startTimer && !triggerActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                pointLight.enabled = false; // Turns off the light once timer reaches zero
                timer = lightActiveTime;  // Reset the timer for next activation
                startTimer = false;  // Stop the timer since the light is off now
            }
        }
    }
}