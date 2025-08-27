using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NirajArts;

/// <summary>
/// Example script showing how to use CustomSpatialAnchor in any scene to find and use the spatial anchor transform
/// </summary>
public class SpatialAnchorExample : MonoBehaviour
{
    [Header("Example Usage")]
    [SerializeField] private GameObject objectToAnchor;
    [SerializeField] private Vector3 offsetFromAnchor = Vector3.zero;
    [SerializeField] private bool followAnchor = true;
    
    private CustomSpatialAnchor spatialAnchor;
    private Transform anchorTransform;
    
    void Start()
    {
        // Find the spatial anchor in the scene
        FindSpatialAnchor();
        
        // Subscribe to anchor creation events
        CustomSpatialAnchor.OnAnchorCreated += OnSpatialAnchorCreated;
        CustomSpatialAnchor.OnAnchorDestroyed += OnSpatialAnchorDestroyed;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        CustomSpatialAnchor.OnAnchorCreated -= OnSpatialAnchorCreated;
        CustomSpatialAnchor.OnAnchorDestroyed -= OnSpatialAnchorDestroyed;
    }
    
    void Update()
    {
        // If we have an anchor and should follow it, update position
        if (followAnchor && anchorTransform != null && objectToAnchor != null)
        {
            objectToAnchor.transform.position = anchorTransform.position + offsetFromAnchor;
            objectToAnchor.transform.rotation = anchorTransform.rotation;
        }
    }
    
    /// <summary>
    /// Find the spatial anchor using various methods
    /// </summary>
    private void FindSpatialAnchor()
    {
        // Method 1: Use the singleton instance (fastest)
        if (CustomSpatialAnchor.Instance != null)
        {
            spatialAnchor = CustomSpatialAnchor.Instance;
            anchorTransform = spatialAnchor.GetAnchorTransform();
            Debug.Log($"[SpatialAnchorExample] Found spatial anchor via Instance: {spatialAnchor.AnchorId}");
            return;
        }
        
        // Method 2: Use FindObjectOfType (reliable)
        spatialAnchor = CustomSpatialAnchor.FindSpatialAnchor();
        if (spatialAnchor != null)
        {
            anchorTransform = spatialAnchor.GetAnchorTransform();
            Debug.Log($"[SpatialAnchorExample] Found spatial anchor via FindObjectOfType: {spatialAnchor.AnchorId}");
            return;
        }
        
        // Method 3: Use static method to get transform directly
        anchorTransform = CustomSpatialAnchor.GetSpatialAnchorTransform();
        if (anchorTransform != null)
        {
            Debug.Log($"[SpatialAnchorExample] Found spatial anchor transform directly");
            return;
        }
        
        Debug.LogWarning("[SpatialAnchorExample] No spatial anchor found in scene!");
    }
    
    /// <summary>
    /// Called when a new spatial anchor is created
    /// </summary>
    private void OnSpatialAnchorCreated(CustomSpatialAnchor anchor)
    {
        Debug.Log($"[SpatialAnchorExample] New spatial anchor created: {anchor.AnchorId}");
        spatialAnchor = anchor;
        anchorTransform = anchor.GetAnchorTransform();
    }
    
    /// <summary>
    /// Called when a spatial anchor is destroyed
    /// </summary>
    private void OnSpatialAnchorDestroyed(CustomSpatialAnchor anchor)
    {
        Debug.Log($"[SpatialAnchorExample] Spatial anchor destroyed: {anchor.AnchorId}");
        
        if (spatialAnchor == anchor)
        {
            spatialAnchor = null;
            anchorTransform = null;
            
            // Try to find another anchor
            FindSpatialAnchor();
        }
    }
    
    /// <summary>
    /// Public method to manually refresh anchor reference
    /// </summary>
    public void RefreshAnchorReference()
    {
        FindSpatialAnchor();
    }
    
    /// <summary>
    /// Get information about the current spatial anchor
    /// </summary>
    public void GetAnchorInfo()
    {
        if (spatialAnchor != null)
        {
            Debug.Log($"[SpatialAnchorExample] Anchor Info:");
            Debug.Log($"  ID: {spatialAnchor.AnchorId}");
            Debug.Log($"  Position: {spatialAnchor.transform.position}");
            Debug.Log($"  Rotation: {spatialAnchor.transform.rotation.eulerAngles}");
            Debug.Log($"  Original Position: {spatialAnchor.OriginalPosition}");
            Debug.Log($"  Original Rotation: {spatialAnchor.OriginalRotation.eulerAngles}");
            Debug.Log($"  Is Persistent: {spatialAnchor.IsPersistent}");
            Debug.Log($"  Is Valid: {spatialAnchor.IsAnchorValid()}");
        }
        else
        {
            Debug.LogWarning("[SpatialAnchorExample] No spatial anchor available for info!");
        }
    }
}
