using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    [SerializeField] Light pointLight;
    [SerializeField] float lightActiveTime;
    [SerializeField] Renderer emissiveRenderer; //Drag the light fixture mesh renderer here
    [SerializeField] Color emissionOnColor = Color.white;
    [SerializeField] Color emissionOffColor = Color.black;
    private Material emissiveMaterial;

    private bool triggerActive = false;
    private bool startTimer = false;
    [SerializeField] float timer;

    private PlayerController playerController;

    private void Start()
    {
        pointLight.enabled = false;
        timer = lightActiveTime;

        // Get material instance 
        if (emissiveRenderer != null)
        {
            emissiveMaterial = emissiveRenderer.material;
            SetEmission(false); // start off
        }

        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found in scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && playerController != null && playerController.isCrouched)
            return;

        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            triggerActive = true;
            startTimer = false;
            pointLight.enabled = true;
            SetEmission(true); // Turn on glow
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
                SetEmission(false); // Turn off glow
                timer = lightActiveTime;
                startTimer = false;
            }
        }
    }

    private void SetEmission(bool isOn)
    {
        if (emissiveMaterial == null) return;

        if (isOn)
        {
            emissiveMaterial.EnableKeyword("_EMISSION");
            emissiveMaterial.SetColor("_EmissionColor", emissionOnColor);
        }
        else
        {
            emissiveMaterial.SetColor("_EmissionColor", emissionOffColor);
            emissiveMaterial.DisableKeyword("_EMISSION");
        }
    }
}
