using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NirajArts
{
    /// <summary>
    /// A persistent component that marks spatial anchor GameObjects and provides easy access to their transforms across scenes.
    /// This script is automatically attached to spatial anchors created by UniversalSpatialAnchor.
    /// </summary>
    public class CustomSpatialAnchor : MonoBehaviour
{
    [Header("Spatial Anchor Info")]
    [SerializeField] private string anchorId = "";
    [SerializeField] private bool isPersistent = true;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;
    
    // Static reference for easy access
    public static CustomSpatialAnchor Instance { get; private set; }
    
    // Public properties for external access
    public string AnchorId => anchorId;
    public Vector3 OriginalPosition => originalPosition;
    public Quaternion OriginalRotation => originalRotation;
    public bool IsPersistent => isPersistent;
    
    // Events for when anchor is created/destroyed
    public static System.Action<CustomSpatialAnchor> OnAnchorCreated;
    public static System.Action<CustomSpatialAnchor> OnAnchorDestroyed;
    
    void Awake()
    {
        // Make this GameObject persistent across scenes
        if (isPersistent)
        {
            DontDestroyOnLoad(gameObject);
        }
        
        // Set as singleton instance (latest created anchor)
        Instance = this;
        
        // Store original transform data
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        Debug.Log($"[CustomSpatialAnchor] Initialized on GameObject: {gameObject.name}");
    }
    
    void Start()
    {
        // Generate anchor ID if not set
        if (string.IsNullOrEmpty(anchorId))
        {
            anchorId = System.Guid.NewGuid().ToString();
        }
        
        // Invoke creation event
        OnAnchorCreated?.Invoke(this);
        
        Debug.Log($"[CustomSpatialAnchor] Anchor started with ID: {anchorId}");
    }
    
    void OnDestroy()
    {
        // Invoke destruction event
        OnAnchorDestroyed?.Invoke(this);
        
        // Clear singleton reference if this was the instance
        if (Instance == this)
        {
            Instance = null;
        }
        
        Debug.Log($"[CustomSpatialAnchor] Anchor destroyed: {anchorId}");
    }
    
    /// <summary>
    /// Initialize the anchor with custom data
    /// </summary>
    public void Initialize(string customId = "", bool persistent = true)
    {
        if (!string.IsNullOrEmpty(customId))
        {
            anchorId = customId;
        }
        
        isPersistent = persistent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        Debug.Log($"[CustomSpatialAnchor] Initialized with ID: {anchorId}, Persistent: {isPersistent}");
    }
    
    /// <summary>
    /// Get the current transform of this spatial anchor
    /// </summary>
    public Transform GetAnchorTransform()
    {
        return transform;
    }
    
    /// <summary>
    /// Get the OVRSpatialAnchor component if attached
    /// </summary>
    public OVRSpatialAnchor GetOVRSpatialAnchor()
    {
        return GetComponent<OVRSpatialAnchor>();
    }
    
    /// <summary>
    /// Check if this anchor has a valid OVRSpatialAnchor and is created
    /// </summary>
    public bool IsAnchorValid()
    {
        var ovrAnchor = GetOVRSpatialAnchor();
        return ovrAnchor != null && ovrAnchor.Created;
    }
    
    /// <summary>
    /// Static method to find the spatial anchor in any scene
    /// </summary>
    public static CustomSpatialAnchor FindSpatialAnchor()
    {
        return FindFirstObjectByType<CustomSpatialAnchor>();
    }
    
    /// <summary>
    /// Static method to find all spatial anchors in the scene
    /// </summary>
    public static CustomSpatialAnchor[] FindAllSpatialAnchors()
    {
        return FindObjectsByType<CustomSpatialAnchor>(FindObjectsSortMode.None);
    }
    
    /// <summary>
    /// Static method to get the transform of the current spatial anchor
    /// </summary>
    public static Transform GetSpatialAnchorTransform()
    {
        var anchor = FindSpatialAnchor();
        return anchor != null ? anchor.transform : null;
    }
}
}
