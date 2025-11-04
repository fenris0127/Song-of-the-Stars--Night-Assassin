using UnityEngine;

public class PlayerStealth : MonoBehaviour
{
    public bool isStealthActive = false;
    
    [Header("Visuals")]
    public Renderer playerRenderer; 
    public Material stealthMaterial; 
    private Material _originalMaterial;

    void Start()
    {
        if (playerRenderer != null)
        {
            _originalMaterial = playerRenderer.material;
        }
    }

    public void ToggleStealth()
    {
        isStealthActive = !isStealthActive;
        
        if (playerRenderer != null)
        {
            playerRenderer.material = isStealthActive ? stealthMaterial : _originalMaterial;
        }
    }
}