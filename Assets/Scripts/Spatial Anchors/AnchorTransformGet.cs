using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NirajArts;

public class AnchorTransformGet : MonoBehaviour
{
    [Header("References")]
    private CustomSpatialAnchor spatialAnchor;
    
    [Header("Transform Settings")]
    [SerializeField] private float yRotationOffset = 0f; // Y rotation offset
    [SerializeField] private bool continuousTracking = true; // Enable real-time tracking
    [SerializeField] private float updateInterval = 0.1f; // Update every 0.1 seconds
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    // Private variables for continuous tracking
    private float lastUpdateTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to CustomSpatialAnchor using the Instance property
        spatialAnchor = CustomSpatialAnchor.Instance;
        
        // If Instance is null, try to find one in the scene
        if (spatialAnchor == null)
        {
            spatialAnchor = CustomSpatialAnchor.FindSpatialAnchor();
        }
        
        if (spatialAnchor == null)
        {
            Debug.LogError("AnchorTransformGet: CustomSpatialAnchor not found in scene! Make sure a spatial anchor exists.");
        }
        else
        {
            if (enableDebugLog)
                Debug.Log("AnchorTransformGet: Successfully found CustomSpatialAnchor.");
            
            // Apply transform once at start since spatial anchor should be available
            ApplyTransformFromAnchor();
        }
        
        // Subscribe to anchor creation events in case anchor is created later
        CustomSpatialAnchor.OnAnchorCreated += OnSpatialAnchorCreated;
    }
    
    void Update()
    {
        // Continuously track spatial anchor if enabled
        if (continuousTracking && spatialAnchor != null)
        {
            // Update at specified intervals to avoid too frequent updates
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                ApplyTransformFromAnchor();
                lastUpdateTime = Time.time;
            }
        }
    }
    
    private void ApplyTransformFromAnchor()
    {
        if (spatialAnchor != null && spatialAnchor.GetAnchorTransform() != null)
        {
            // Get the spatial anchor's position and rotation
            Transform anchorTransform = spatialAnchor.GetAnchorTransform();
            Vector3 anchorPosition = anchorTransform.position;
            Quaternion anchorRotation = anchorTransform.rotation;
            
            // Apply position with Y always set to 0
            transform.position = new Vector3(anchorPosition.x, 0f, anchorPosition.z);
            
            // Apply rotation with Y offset
            Vector3 eulerAngles = anchorRotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y + yRotationOffset, eulerAngles.z);
            
            if (enableDebugLog)
            {
                Debug.Log($"AnchorTransformGet: Applied transform - Position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
                Debug.Log($"AnchorTransformGet: Original anchor position: {anchorPosition}, Original anchor rotation: {anchorRotation.eulerAngles}");
            }
        }
        else
        {
            Debug.LogWarning("AnchorTransformGet: CustomSpatialAnchor or its transform is null!");
        }
    }
    
    /// <summary>
    /// Manually trigger transform application (useful for testing or re-applying)
    /// </summary>
    public void ManuallyApplyTransform()
    {
        if (spatialAnchor != null)
        {
            ApplyTransformFromAnchor();
        }
        else
        {
            Debug.LogWarning("AnchorTransformGet: Cannot manually apply transform - spatialAnchor is null.");
        }
    }
    
    /// <summary>
    /// Called when a new spatial anchor is created
    /// </summary>
    private void OnSpatialAnchorCreated(CustomSpatialAnchor anchor)
    {
        if (enableDebugLog)
            Debug.Log("AnchorTransformGet: New spatial anchor created, updating reference.");
        
        spatialAnchor = anchor;
        ApplyTransformFromAnchor();
        lastUpdateTime = Time.time; // Reset update timer
    }
    
    /// <summary>
    /// Enable or disable continuous tracking at runtime
    /// </summary>
    public void SetContinuousTracking(bool enabled)
    {
        continuousTracking = enabled;
        if (enableDebugLog)
            Debug.Log($"AnchorTransformGet: Continuous tracking {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Set the update interval for continuous tracking
    /// </summary>
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.01f, interval); // Minimum 0.01 seconds
        if (enableDebugLog)
            Debug.Log($"AnchorTransformGet: Update interval set to {updateInterval} seconds");
    }
    
    /// <summary>
    /// Force immediate update regardless of interval
    /// </summary>
    public void ForceUpdate()
    {
        ApplyTransformFromAnchor();
        lastUpdateTime = Time.time;
        if (enableDebugLog)
            Debug.Log("AnchorTransformGet: Forced immediate update");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        CustomSpatialAnchor.OnAnchorCreated -= OnSpatialAnchorCreated;
    }
    
    void OnEnable()
    {
        // Refresh anchor reference and apply transform when object is enabled
        RefreshAnchorReference();
    }
    
    /// <summary>
    /// Refresh the spatial anchor reference (useful when returning to scenes)
    /// </summary>
    public void RefreshAnchorReference()
    {
        // Try to get the current spatial anchor instance
        var currentAnchor = CustomSpatialAnchor.Instance;
        if (currentAnchor == null)
        {
            currentAnchor = CustomSpatialAnchor.FindSpatialAnchor();
        }
        
        if (currentAnchor != null && currentAnchor != spatialAnchor)
        {
            spatialAnchor = currentAnchor;
            if (enableDebugLog)
                Debug.Log("AnchorTransformGet: Refreshed spatial anchor reference");
        }
        
        // Apply transform immediately if we have a valid anchor
        if (spatialAnchor != null)
        {
            ApplyTransformFromAnchor();
        }
    }
}
