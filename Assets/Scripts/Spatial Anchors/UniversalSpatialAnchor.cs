using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace NirajArts
{
    public class UniversalSpatialAnchor : MonoBehaviour
{
    [Header("Spatial Anchor Settings")]
    [SerializeField] private bool autoCreateAnchor = true;
    [SerializeField] private bool debugLogging = true;
    [SerializeField] public bool visualize = false;
    
    [Header("Session Persistence")]
    [SerializeField] private bool sessionOnlyAnchors = true; // No device storage, session only
    
    [Header("Public Methods (Can be called from buttons)")]
    [Space(10)]
    [Tooltip("Call DeleteSpatialAnchor() to remove spatial anchor")]
    [SerializeField] private bool showPublicMethods = true;
    
    // Private variables
    private bool hasCreatedAnchor = false;
    private bool wasAnchorSet = false;
    private OVRSpatialAnchor currentSpatialAnchor;
    private GameObject spatialAnchorGameObject;
    
    // Reusable buffer for loading anchors (reduces garbage collection)
    private List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
    
    // Public properties
    public bool HasSpatialAnchor { get { return currentSpatialAnchor != null && currentSpatialAnchor.Created; } }
    public OVRSpatialAnchor CurrentSpatialAnchor { get { return currentSpatialAnchor; } }
    
    void Start()
    {
        // Try to load existing spatial anchor on start (disabled for session-only mode)
        LoadExistingSpatialAnchorAsync();
    }
    
    void Update()
    {
        // Monitor SetUniversalAnchor for anchor setting
        if (SetUniversalAnchor.instance != null && autoCreateAnchor)
        {
            bool currentAnchorState = SetUniversalAnchor.instance.isAnchorSet;
            
            // Check if anchor was just set (transition from false to true)
            if (currentAnchorState && !wasAnchorSet && !hasCreatedAnchor)
            {
                CreateSpatialAnchor();
            }
            
            wasAnchorSet = currentAnchorState;
        }
    }
    
    /// <summary>
    /// Creates a spatial anchor at the saved universal anchor position
    /// </summary>
    public void CreateSpatialAnchor()
    {
        if (hasCreatedAnchor)
        {
            LogDebug("Spatial anchor already created, skipping...");
            return;
        }
        
        if (SetUniversalAnchor.instance == null || !SetUniversalAnchor.instance.isAnchorSet)
        {
            LogDebug("SetUniversalAnchor is not set yet, cannot create spatial anchor");
            return;
        }
        
        // Check if we're in the editor - spatial anchors only work on device
        if (Application.isEditor)
        {
            LogDebug("WARNING: Spatial anchors don't work in Unity Editor. Please test on Meta Quest device.");
            LogDebug("Creating mock spatial anchor for editor visualization only...");
            CreateMockSpatialAnchor();
            return;
        }
        
        Vector3 anchorPosition = SetUniversalAnchor.SavedCenterPosition;
        Quaternion anchorRotation = SetUniversalAnchor.SavedCenterRotation;
        
        // Validate the position
        if (anchorPosition == Vector3.zero)
        {
            LogDebug("WARNING: SavedCenterPosition is Vector3.zero. Using SavedAnchorPosition as fallback.");
            anchorPosition = SetUniversalAnchor.SavedAnchorPosition;
            anchorRotation = SetUniversalAnchor.SavedAnchorRotation;
        }
        
        LogDebug($"Creating spatial anchor at center position: {anchorPosition}, rotation: {anchorRotation.eulerAngles}");
        
        // Create GameObject for spatial anchor
        spatialAnchorGameObject = new GameObject("UniversalSpatialAnchor");
        spatialAnchorGameObject.transform.position = anchorPosition;
        spatialAnchorGameObject.transform.rotation = anchorRotation;
        
        // Add OVRSpatialAnchor component
        currentSpatialAnchor = spatialAnchorGameObject.AddComponent<OVRSpatialAnchor>();
        
        // Add CustomSpatialAnchor component for persistence and easy access
        var customAnchor = spatialAnchorGameObject.AddComponent<CustomSpatialAnchor>();
        customAnchor.Initialize($"UniversalAnchor_{System.DateTime.Now.Ticks}", true);
        
        // Add visual representation components
        AddVisualComponents(spatialAnchorGameObject);
        
        // Start the spatial anchor creation process
        CreateAndSaveSpatialAnchor();
        
        hasCreatedAnchor = true;
    }
    
    /// <summary>
    /// Creates a mock spatial anchor for editor testing (no OVRSpatialAnchor component)
    /// </summary>
    private void CreateMockSpatialAnchor()
    {
        Vector3 anchorPosition = SetUniversalAnchor.SavedCenterPosition;
        Quaternion anchorRotation = SetUniversalAnchor.SavedCenterRotation;
        
        // Validate the position
        if (anchorPosition == Vector3.zero)
        {
            LogDebug("WARNING: SavedCenterPosition is Vector3.zero. Using SavedAnchorPosition as fallback.");
            anchorPosition = SetUniversalAnchor.SavedAnchorPosition;
            anchorRotation = SetUniversalAnchor.SavedAnchorRotation;
        }
        
        LogDebug($"Creating MOCK spatial anchor at center position: {anchorPosition}, rotation: {anchorRotation.eulerAngles}");
        
        // Create GameObject for mock spatial anchor (without OVRSpatialAnchor component)
        spatialAnchorGameObject = new GameObject("MockUniversalSpatialAnchor [EDITOR ONLY]");
        spatialAnchorGameObject.transform.position = anchorPosition;
        spatialAnchorGameObject.transform.rotation = anchorRotation;
        
        // Add CustomSpatialAnchor component for persistence and easy access (even in editor)
        var customAnchor = spatialAnchorGameObject.AddComponent<CustomSpatialAnchor>();
        customAnchor.Initialize($"MockAnchor_{System.DateTime.Now.Ticks}", true);
        
        // Add visual representation components
        AddVisualComponents(spatialAnchorGameObject);
        
        // Change color to red to indicate it's a mock
        var meshRenderer = spatialAnchorGameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
        }
        
        LogDebug("Mock spatial anchor created for editor testing. This will not persist across sessions.");
        hasCreatedAnchor = true;
    }
    
    /// <summary>
    /// Coroutine to handle spatial anchor creation using the new async API (session only)
    /// </summary>
    private async void CreateAndSaveSpatialAnchor()
    {
        LogDebug("Starting spatial anchor creation process (session only)...");
        
        // Wait for the spatial anchor to be created
        while (!currentSpatialAnchor.Created)
        {
            await Task.Yield();
        }
        
        LogDebug("Spatial anchor created successfully (session only - not saved to device)!");
        
        // Note: We're not saving to device storage anymore for session-only anchors
        if (!sessionOnlyAnchors)
        {
            // Save the anchor using the new async API (only if session-only is disabled)
            try
            {
                var result = await currentSpatialAnchor.SaveAnchorAsync();
                
                if (result.Success)
                {
                    LogDebug("Spatial anchor saved to device storage!");
                }
                else
                {
                    LogDebug($"Failed to save spatial anchor to device: {result.Status}");
                }
            }
            catch (Exception e)
            {
                LogDebug($"Exception while saving spatial anchor: {e.Message}");
            }
        }
        else
        {
            LogDebug("Session-only mode: Spatial anchor not saved to device storage");
        }
    }
    
    /// <summary>
    /// Loads existing spatial anchor from storage (disabled for session-only mode)
    /// </summary>
    private void LoadExistingSpatialAnchorAsync()
    {
        // Check if we're in the editor - spatial anchors only work on device
        if (Application.isEditor)
        {
            LogDebug("Skipping spatial anchor loading in Unity Editor. Spatial anchors only work on Meta Quest device.");
            return;
        }
        
        // Skip loading in session-only mode
        if (sessionOnlyAnchors)
        {
            LogDebug("Session-only mode: Skipping spatial anchor loading from device storage");
            return;
        }
        
        LogDebug("Session-only mode is disabled, but no loading implementation for persistent storage");
        // Note: If you want to re-enable persistent storage, you would implement the loading logic here
    }
    
    /// <summary>
    /// Manually trigger spatial anchor creation
    /// </summary>
    public void ManualCreateSpatialAnchor()
    {
        CreateSpatialAnchor();
    }
    
    /// <summary>
    /// Manually trigger loading of existing spatial anchor from device storage
    /// </summary>
    public void ManualLoadSpatialAnchor()
    {
        if (hasCreatedAnchor)
        {
            LogDebug("Spatial anchor already loaded, skipping...");
            return;
        }
        
        LogDebug("Manually loading spatial anchor from device storage...");
        LoadExistingSpatialAnchorAsync();
    }
    
    /// <summary>
    /// Clear saved spatial anchor data (session only - no device storage)
    /// </summary>
    public async void ClearSavedAnchor()
    {
        // No PlayerPrefs to clear in session-only mode
        LogDebug("Session-only mode: No device storage to clear");
        
        if (currentSpatialAnchor != null && !sessionOnlyAnchors)
        {
            try
            {
                var eraseResult = await currentSpatialAnchor.EraseAnchorAsync();
                if (eraseResult.Success)
                {
                    LogDebug("Spatial anchor erased successfully from storage");
                }
                else
                {
                    LogDebug($"Failed to erase spatial anchor: {eraseResult.Status}");
                }
            }
            catch (Exception e)
            {
                LogDebug($"Exception while erasing spatial anchor: {e.Message}");
            }
        }
        
        if (spatialAnchorGameObject != null)
        {
            Destroy(spatialAnchorGameObject);
        }
        
        hasCreatedAnchor = false;
        currentSpatialAnchor = null;
        spatialAnchorGameObject = null;
        
        LogDebug("Spatial anchor cleared (session only)");
    }
    
    /// <summary>
    /// Public method to delete spatial anchor - can be called from buttons or at scene start
    /// </summary>
    public void DeleteSpatialAnchor()
    {
        LogDebug("DeleteSpatialAnchor called - removing spatial anchor (session only)...");
        
        // No PlayerPrefs to clear in session-only mode
        LogDebug("Session-only mode: No device storage to clear");
        
        // If we have a current spatial anchor, try to erase it from device storage (only if not session-only)
        if (currentSpatialAnchor != null && !Application.isEditor && !sessionOnlyAnchors)
        {
            // Use async void for fire-and-forget deletion
            _ = DeleteSpatialAnchorAsync();
        }
        
        // Destroy the GameObject immediately
        if (spatialAnchorGameObject != null)
        {
            LogDebug($"Destroying spatial anchor GameObject: {spatialAnchorGameObject.name}");
            Destroy(spatialAnchorGameObject);
        }
        
        // Reset state variables
        hasCreatedAnchor = false;
        currentSpatialAnchor = null;
        spatialAnchorGameObject = null;
        wasAnchorSet = false;
        
        LogDebug("Spatial anchor deleted successfully (session only). Ready to create new anchor.");
    }
    
    /// <summary>
    /// Async helper method for deleting spatial anchor from device storage
    /// </summary>
    private async Task DeleteSpatialAnchorAsync()
    {
        try
        {
            if (currentSpatialAnchor != null)
            {
                LogDebug("Attempting to erase spatial anchor from device storage...");
                var eraseResult = await currentSpatialAnchor.EraseAnchorAsync();
                
                if (eraseResult.Success)
                {
                    LogDebug("Spatial anchor erased successfully from device storage");
                }
                else
                {
                    LogDebug($"Failed to erase spatial anchor from device storage: {eraseResult.Status}");
                }
            }
        }
        catch (Exception e)
        {
            LogDebug($"Exception while erasing spatial anchor from device storage: {e.Message}");
        }
    }
    
    /// <summary>
    /// Public method to delete spatial anchor at scene start - useful for testing or reset scenarios
    /// </summary>
    public void DeleteSpatialAnchorOnStart()
    {
        LogDebug("DeleteSpatialAnchorOnStart called - clearing any existing spatial anchor data...");
        DeleteSpatialAnchor();
    }
    
    /// <summary>
    /// Get the current spatial anchor position
    /// </summary>
    public Vector3 GetSpatialAnchorPosition()
    {
        if (spatialAnchorGameObject != null)
            return spatialAnchorGameObject.transform.position;
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get the current spatial anchor rotation
    /// </summary>
    public Quaternion GetSpatialAnchorRotation()
    {
        if (spatialAnchorGameObject != null)
            return spatialAnchorGameObject.transform.rotation;
        
        return Quaternion.identity;
    }
    
    /// <summary>
    /// Completely reset the spatial anchor system
    /// This will delete the anchor and reset all state variables
    /// </summary>
    public void CompleteReset()
    {
        LogDebug("CompleteReset called - performing full spatial anchor system reset...");
        
        // Delete any existing spatial anchor
        DeleteSpatialAnchor();
        
        // Reset all state variables to initial state
        hasCreatedAnchor = false;
        wasAnchorSet = false;
        currentSpatialAnchor = null;
        spatialAnchorGameObject = null;
        
        LogDebug("Complete spatial anchor reset finished. System ready for fresh start.");
    }
    
    /// <summary>
    /// Temporarily disable auto-creation (useful during reset operations)
    /// </summary>
    public void SetAutoCreateEnabled(bool enabled)
    {
        autoCreateAnchor = enabled;
        LogDebug($"Auto-create anchor set to: {enabled}");
    }
    
    /// <summary>
    /// Check if spatial anchor is localized (tracking properly)
    /// </summary>
    public bool IsSpatialAnchorLocalized()
    {
        return currentSpatialAnchor != null && currentSpatialAnchor.Created && currentSpatialAnchor.Localized;
    }
    
    /// <summary>
    /// Add visual components to make the spatial anchor visible in the scene
    /// </summary>
    private void AddVisualComponents(GameObject anchorGameObject)
    {
        // Only add visual components if visualize is enabled
        if (!visualize)
        {
            LogDebug("Visualization disabled - skipping visual components");
            return;
        }
        
        // Add a small cube to visualize the anchor
        var meshFilter = anchorGameObject.AddComponent<MeshFilter>();
        var meshRenderer = anchorGameObject.AddComponent<MeshRenderer>();
        
        // Create a small cube mesh
        meshFilter.mesh = CreateCubeMesh();
        
        // Create a material for the anchor
        var material = new Material(Shader.Find("Standard"));
        material.color = Color.cyan;
        material.SetFloat("_Metallic", 0.5f);
        material.SetFloat("_Smoothness", 0.8f);
        meshRenderer.material = material;
        
        // Scale it down to be a small visual indicator
        anchorGameObject.transform.localScale = Vector3.one * 0.1f;
        
        LogDebug("Added visual components to spatial anchor GameObject");
    }
    
    /// <summary>
    /// Create a simple cube mesh for visualization
    /// </summary>
    private Mesh CreateCubeMesh()
    {
        Mesh mesh = new Mesh();
        
        // Define vertices for a cube
        Vector3[] vertices = new Vector3[]
        {
            // Front face
            new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1),
            // Back face
            new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, -1, -1),
            // Top face
            new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, -1),
            // Bottom face
            new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1),
            // Right face
            new Vector3(1, -1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1),
            // Left face
            new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 1, -1)
        };
        
        // Define triangles (each face has 2 triangles = 6 vertices)
        int[] triangles = new int[]
        {
            // Front face
            0, 2, 1, 0, 3, 2,
            // Back face
            4, 5, 6, 4, 6, 7,
            // Top face
            8, 9, 10, 8, 10, 11,
            // Bottom face
            12, 14, 13, 12, 15, 14,
            // Right face
            16, 17, 18, 16, 18, 19,
            // Left face
            20, 22, 21, 20, 23, 22
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.name = "SpatialAnchorCube";
        
        return mesh;
    }
    
    /// <summary>
    /// Debug logging helper
    /// </summary>
    private void LogDebug(string message)
    {
        if (debugLogging)
        {
            Debug.Log($"[UniversalSpatialAnchor] {message}");
        }
    }
}
}
