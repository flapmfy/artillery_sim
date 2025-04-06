using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
[RequireComponent(typeof(AudioSource))]
public class ArtilleryFireButton : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("Drag the GameObject with the BulletLoadTrigger script here.")]
    [SerializeField] private BulletLoadTrigger bulletLoadTrigger;

    // --- ADD THIS LINE ---
    [Tooltip("Drag the GameObject with the ChamberBlockController script here.")]
    [SerializeField] private ChamberBlockController chamberBlockController;
    // --- END ADDED LINE ---

    [Header("Firing Effects")]
    [Tooltip("Particle system for the muzzle flash.")]
    [SerializeField] private ParticleSystem muzzleFlashParticle;
    [Tooltip("Particle system for smoke after firing.")]
    [SerializeField] private ParticleSystem smokeParticle;
    [Tooltip("Sound effect for firing.")]
    [SerializeField] private AudioClip fireSound;
    [Tooltip("Sound effect for trying to fire when empty or block open (optional).")]
    [SerializeField] private AudioClip emptyClickSound; // Renamed tooltip slightly

    private XRBaseInteractable interactable;
    private AudioSource audioSource;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // --- Validation ---
        if (bulletLoadTrigger == null)
        {
            Debug.LogError($"ArtilleryFireButton on {gameObject.name}: BulletLoadTrigger reference is missing!", this);
            enabled = false;
        }
        // --- ADD THIS CHECK ---
        // if (chamberBlockController == null)
        // {
        //     Debug.LogError($"ArtilleryFireButton on {gameObject.name}: ChamberBlockController reference is missing!", this);
        //     enabled = false;
        // }
        // --- END ADDED CHECK ---
    }

    void Start()
{
    // Perform checks that rely on other components being ready in Start()
    Debug.Log($"[{Time.frameCount}] ArtilleryFireButton Start() called for {gameObject.name}."); // Log when Start begins

        // Check the raw assignment from the Inspector FIRST
        if (chamberBlockController == null)
        {
            // This is the error you're getting - the Inspector assignment itself seems null at this point
            Debug.LogError($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: ChamberBlockController reference is NULL immediately in Start(). Check Inspector assignment persistence!", this);
            enabled = false; // Still disable if missing
            return; // Stop further checks if it's already null
        }
        else
        {
            // If the reference *is* assigned, let's check the state of the assigned object/component
            Debug.Log($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: ChamberBlockController reference SEEMS assigned in Inspector to GameObject '{chamberBlockController.gameObject.name}'.", chamberBlockController.gameObject);

            // Is the GameObject itself active?
            if (!chamberBlockController.gameObject.activeInHierarchy)
            {
                Debug.LogError($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: The assigned ChamberBlockController's GameObject ('{chamberBlockController.gameObject.name}') is INACTIVE in the hierarchy!", chamberBlockController.gameObject);
                // Note: Reference isn't null, but the object is inactive. This might still cause issues later.
            }

            // Does the component still exist on that GameObject?
            // Try getting the component again from the referenced GameObject
            ChamberBlockController checkComponent = chamberBlockController.gameObject.GetComponent<ChamberBlockController>();
            if (checkComponent == null)
            {
                 Debug.LogError($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: The assigned GameObject ('{chamberBlockController.gameObject.name}') NO LONGER HAS the ChamberBlockController component attached!", chamberBlockController.gameObject);
                 // This would imply something removed the component between Awake and Start, or the initial assignment was wrong.
            }
            else
            {
                 Debug.Log($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: Successfully re-fetched ChamberBlockController component from '{checkComponent.gameObject.name}'. Reference appears valid.", checkComponent.gameObject);
            }
        }

         // You could move the bulletLoadTrigger check here too if needed
         if (bulletLoadTrigger == null)
         {
             Debug.LogError($"[{Time.frameCount}] ArtilleryFireButton on {gameObject.name}: BulletLoadTrigger reference is missing! (Checked in Start)", this);
             enabled = false;
         }
}

    void OnEnable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(AttemptFire);
        }
    }

    void OnDisable()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(AttemptFire);
        }
    }

    // --- REPLACE the existing AttemptFire method with this ---
    private void AttemptFire(SelectEnterEventArgs args)
    {
        // Check if references are valid (already checked in Awake, but good practice)
        if (bulletLoadTrigger == null || chamberBlockController == null)
        {
            Debug.LogError("AttemptFire called, but references are missing.", this);
            return;
        }

        // --- The Core Logic: Check BOTH conditions ---
        bool isLoaded = bulletLoadTrigger.IsLoaded;
        bool isClosed = chamberBlockController.IsClosed();

        if (isLoaded && isClosed)
        {
            Debug.Log("Firing Artillery! (Loaded & Closed)");
            FireEffects(); // Trigger the actual firing sequence

            // Destroy the loaded bullet via the trigger script
            bulletLoadTrigger.DestroyLoadedBullet();
        }
        else
        {
            // Provide more specific feedback
            if (!isLoaded)
            {
                Debug.Log("Attempted to fire, but chamber is empty.");
            }
            else // Must be not closed if we got here
            {
                 Debug.Log("Attempted to fire, but chamber block is open.");
            }

            // Play empty click sound if available
            PlayFailSound();
        }
    }
    // --- END REPLACEMENT ---


    private void FireEffects()
    {
        // Play Particles
        if (muzzleFlashParticle != null)
        {
            muzzleFlashParticle.Play();
        }
        if (smokeParticle != null)
        {
            smokeParticle.Play();
        }

        // Play Sound
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        // --- TODO: Add Actual Projectile Launching Logic Here ---
        // This is where you would instantiate a separate projectile prefab
        // or apply force if the bullet *was* the projectile (though it's destroyed now).
    }

    // --- ADD THIS HELPER METHOD ---
    private void PlayFailSound()
    {
         if (emptyClickSound != null && audioSource != null)
         {
             audioSource.PlayOneShot(emptyClickSound);
         }
    }
    // --- END ADDED METHOD ---
}
