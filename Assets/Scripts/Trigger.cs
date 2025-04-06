using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Needed for Interaction Manager & Interfaces
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Needed for XRGrabInteractable & IXRSelectInteractable
// using UnityEngine.XR.Interaction.Toolkit.Interactors; // Not strictly needed here
using System.Collections.Generic; // Not strictly needed here

[RequireComponent(typeof(Collider))] // Ensure there's a collider
public class BulletLoadTrigger : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The exact position and rotation where the bullet should snap to.")]
    [SerializeField] private Transform loadingPoint;

    [Tooltip("The Tag assigned to your bullet prefab.")]
    [SerializeField] private string bulletTag = "ArtilleryShell"; // CHANGE THIS to your bullet's tag

    [Tooltip("Reference to the scene's XR Interaction Manager.")]
    [SerializeField] private XRInteractionManager interactionManager;

    [Header("State")]
    [Tooltip("Reference to the currently loaded bullet instance.")]
    [SerializeField] // Show in inspector for debugging
    private GameObject loadedBulletInstance = null;

    [Header("Optional")]
    [Tooltip("Sound effect to play when loading is successful.")]
    [SerializeField] private AudioClip loadSound;
    private AudioSource audioSource; // Optional: Use an AudioSource component

    void Awake()
    {
        // Ensure the collider on this object is set to trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} is not set to 'Is Trigger'. Auto-fixing.", this);
            col.isTrigger = true;
        }

        // Basic validation
        if (loadingPoint == null)
        {
            Debug.LogError("BulletLoadTrigger: Loading Point transform is not assigned!", this);
            enabled = false; // Disable script if setup is invalid
        }
        if (interactionManager == null)
        {
            // Attempt to find if not assigned (Manual assignment is preferred)
            interactionManager = FindObjectOfType<XRInteractionManager>();
            if (interactionManager == null)
            {
                Debug.LogError("BulletLoadTrigger: XR Interaction Manager is not assigned and could not be found! Please add one (GameObject -> XR -> Interaction Manager) and assign it.", this);
                enabled = false;
            }
            else
            {
                 Debug.LogWarning("BulletLoadTrigger: Automatically found XR Interaction Manager. Manual assignment is recommended.", this);
            }
        }
        if (string.IsNullOrEmpty(bulletTag))
        {
             Debug.LogError("BulletLoadTrigger: Bullet Tag is not set!", this);
             enabled = false;
        }

        // Optional: Get or add an AudioSource for sound
        audioSource = GetComponent<AudioSource>();
        if (loadSound != null && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    

    void OnTriggerEnter(Collider other)
    {
        // 1. Check if already loaded
        if (loadedBulletInstance != null)
        {
            return; // Already loaded, do nothing
        }

        // 2. Check if the entering object has the correct tag
        if (!other.CompareTag(bulletTag))
        {
            return; // Not the right object
        }

        // 3. Try to get the interactable and rigidbody from the bullet
        XRGrabInteractable bulletInteractable = other.GetComponent<XRGrabInteractable>();
        Rigidbody bulletRigidbody = other.GetComponent<Rigidbody>();

        if (bulletInteractable == null || bulletRigidbody == null)
        {
            Debug.LogWarning($"Bullet '{other.name}' entered trigger but is missing XRGrabInteractable or Rigidbody.", other);
            return; // Missing required components
        }

        // 4. IMPORTANT: Check if the bullet is currently selected (grabbed)
        // We only want to load if the player actively brings it here.
        if (!bulletInteractable.isSelected)
        {
             Debug.Log($"Bullet '{other.name}' entered trigger but was not selected (grabbed).");
             return; // Don't load if it just rolls in or isn't held
        }

        Debug.Log($"Loading bullet: {other.name}");

        // --- Perform Loading ---

        // 5. Force the player to release the bullet (EXPLICIT CAST TO INTERFACE)
        // This is crucial before manipulating the object directly.
        if (bulletInteractable.isSelected) // Double check it's still selected
        {
            // Explicitly cast the interactable to the required interface type (IXRSelectInteractable)
            // to ensure the correct non-obsolete method overload is called.
            IXRSelectInteractable interactableToCancel = bulletInteractable;

            // Pass the variable holding the interface reference
            interactionManager.CancelInteractableSelection(interactableToCancel);
            // Or cast directly: interactionManager.CancelInteractableSelection((IXRSelectInteractable)bulletInteractable);

            Debug.Log($"Forced release of interactable: {bulletInteractable.name}");
        }
        else
        {
            // This case shouldn't normally happen if we checked isSelected earlier,
            // but handle it defensively.
            Debug.LogWarning($"Bullet '{other.name}' was no longer selected when trying to force release.", bulletInteractable);
        }


        // 6. Disable further interaction & physics influence
        bulletInteractable.enabled = false; // Stop it from being grabbed again easily
        bulletRigidbody.isKinematic = true; // Stop physics forces from moving it
        bulletRigidbody.velocity = Vector3.zero;
        bulletRigidbody.angularVelocity = Vector3.zero;

        // 7. Snap to loading position and rotation
        GameObject bulletToLoad = other.gameObject;
        bulletToLoad.transform.position = loadingPoint.position;
        bulletToLoad.transform.rotation = loadingPoint.rotation;

        // 8. Parent (Optional but good practice for organization)
        // Parent it to the loading point or the barrel itself
        bulletToLoad.transform.SetParent(loadingPoint, true); // worldPositionStays = true

        // 9. Store reference and play sound
        loadedBulletInstance = bulletToLoad;
        PlayLoadSound();

        // 10. (Optional) Notify other systems (e.g., enable firing)
        // FindObjectOfType<ArtilleryController>()?.OnBulletLoaded(loadedBulletInstance);
    }

    // --- Optional Methods ---

    private void PlayLoadSound()
    {
        if (audioSource != null && loadSound != null)
        {
            audioSource.PlayOneShot(loadSound);
        }
        else if (loadSound != null) // Fallback if no AudioSource component
        {
             AudioSource.PlayClipAtPoint(loadSound, loadingPoint.position);
        }
    }

    // Call this method from elsewhere if you implement unloading
    /// <summary>
    /// Destroys the currently loaded bullet instance and resets the state.
    /// Called after firing.
    /// </summary>
    public void DestroyLoadedBullet() // Renamed for clarity
    {
        if (loadedBulletInstance != null)
        {
            Debug.Log($"Destroying loaded bullet: {loadedBulletInstance.name}");

            // --- Destroy the GameObject ---
            Destroy(loadedBulletInstance); // This removes it from the scene

            // Clear reference
            loadedBulletInstance = null;

            // Optional: Play an "eject" or "spent" sound here if desired
        }
        else
        {
             Debug.LogWarning("DestroyLoadedBullet called, but no bullet was loaded.");
        }
    }

    // Public property to check if loaded (read-only from outside)
    public bool IsLoaded => loadedBulletInstance != null;
    public GameObject GetLoadedBullet() => loadedBulletInstance;
}
