using UnityEngine;


public class LoadProjectile : MonoBehaviour
{
    public Transform loadPosition; // Where bullet snaps to
    public GameObject chamberBlock; // Your steel block
    private GameObject loadedBullet;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && loadedBullet == null)
        {
            // Disable bullet physics
            Destroy(other.GetComponent<Rigidbody>());
            Destroy(other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>());
            
            // Snap to position
            other.transform.position = loadPosition.position;
            other.transform.rotation = loadPosition.rotation;
            
            // Make it child of barrel
            other.transform.SetParent(loadPosition);
            
            loadedBullet = other.gameObject;
            
            // Enable chamber block
            chamberBlock.SetActive(true);
        }
    }
}