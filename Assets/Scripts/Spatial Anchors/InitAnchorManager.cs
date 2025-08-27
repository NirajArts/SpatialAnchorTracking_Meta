using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NirajArts;

/// <summary>
/// Manager script for the initial anchor setup scene.
/// Provides public interface methods to control spatial anchors through UI buttons.
/// This script stays in the setup scene and interfaces with the persistent UniversalSpatialAnchor.
/// </summary>
public class InitAnchorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UniversalSpatialAnchor universalSpatialAnchor;
    [SerializeField] private SetUniversalAnchor setUniversalAnchor;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    
    void Start()
    {
        // Find references to the persistent scripts
        if (universalSpatialAnchor == null)
        {
            universalSpatialAnchor = FindFirstObjectByType<UniversalSpatialAnchor>();
        }
        
        if (setUniversalAnchor == null)
        {
            setUniversalAnchor = FindFirstObjectByType<SetUniversalAnchor>();
        }
        
        // Log status
        if (enableDebugLog)
        {
            Debug.Log($"[InitAnchorManager] UniversalSpatialAnchor found: {universalSpatialAnchor != null}");
            Debug.Log($"[InitAnchorManager] SetUniversalAnchor found: {setUniversalAnchor != null}");
        }
    }
    
    // ======================== PUBLIC INTERFACE METHODS ========================
    // These methods can be called from UI buttons in the initial setup scene
    
    /// <summary>
    /// Create a new spatial anchor manually
    /// </summary>
    public void CreateSpatialAnchor()
    {
        if (universalSpatialAnchor != null)
        {
            LogDebug("Creating spatial anchor via button...");
            universalSpatialAnchor.CreateSpatialAnchor();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot create spatial anchor - UniversalSpatialAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// Delete the current spatial anchor
    /// </summary>
    public void DeleteSpatialAnchor()
    {
        if (universalSpatialAnchor != null)
        {
            LogDebug("Deleting spatial anchor via button...");
            universalSpatialAnchor.DeleteSpatialAnchor();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot delete spatial anchor - UniversalSpatialAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// Clear/delete spatial anchor (alias for DeleteSpatialAnchor for UI clarity)
    /// </summary>
    public void ClearSpatialAnchor()
    {
        DeleteSpatialAnchor();
    }
    
    /// <summary>
    /// Reset the spatial anchor system (delete existing anchor)
    /// </summary>
    public void ResetSpatialAnchor()
    {
        LogDebug("ResetSpatialAnchor called - performing complete reset...");
        ResetAnchor(); // Use the new comprehensive reset method
    }
    
    /// <summary>
    /// Start the hand positioning process
    /// </summary>
    public void StartPositioning()
    {
        if (setUniversalAnchor != null)
        {
            LogDebug("Starting anchor positioning via button...");
            setUniversalAnchor.EnablePositioningOfAnchor();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot start positioning - SetUniversalAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// Clear any existing spatial anchor and start fresh positioning
    /// Perfect for "Start New Placement" buttons
    /// </summary>
    public void StartFreshPositioning()
    {
        LogDebug("Starting fresh positioning - clearing existing anchor first...");
        
        // First clear any existing anchor
        ClearSpatialAnchor();
        
        // Then start positioning
        StartPositioning();
    }
    
    /// <summary>
    /// Restart the positioning process (reposition)
    /// </summary>
    public void RepositionAnchor()
    {
        if (setUniversalAnchor != null)
        {
            LogDebug("Repositioning anchor via button...");
            setUniversalAnchor.RepositionTable();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot reposition - SetUniversalAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// Complete the anchoring process and proceed to menu
    /// </summary>
    public void CompleteAnchoring()
    {
        if (setUniversalAnchor != null)
        {
            LogDebug("Completing anchoring process via button...");
            setUniversalAnchor.SetAnchor();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot complete anchoring - SetUniversalAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// Load any existing saved spatial anchor from device storage
    /// Perfect for "Load Saved Anchor" buttons
    /// </summary>
    public void LoadSpatialAnchor()
    {
        if (universalSpatialAnchor != null)
        {
            LogDebug("Loading saved spatial anchor via button...");
            universalSpatialAnchor.ManualLoadSpatialAnchor();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot load spatial anchor - UniversalSpatialAnchor reference is null!");
        }
    }
    
    /// <summary>
    /// COMPLETE RESET - Resets everything related to spatial anchors
    /// This will reset both the spatial anchor AND the hand positioning
    /// Perfect for a complete fresh start
    /// </summary>
    public void ResetAnchor()
    {
        LogDebug("=== COMPLETE ANCHOR RESET ===");
        LogDebug("Resetting ALL spatial anchor and hand positioning data...");
        
        // Step 1: Temporarily disable auto-creation to prevent immediate recreation
        if (universalSpatialAnchor != null)
        {
            LogDebug("Step 1: Temporarily disabling auto-creation...");
            universalSpatialAnchor.SetAutoCreateEnabled(false);
        }
        
        // Step 2: Reset hand positioning first (this prevents triggers for auto-recreation)
        if (setUniversalAnchor != null)
        {
            LogDebug("Step 2: Resetting hand positioning...");
            setUniversalAnchor.ResetAnchorPosition(); // This should reset isAnchorSet to false
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot reset hand positioning - SetUniversalAnchor reference is null!");
        }
        
        // Step 3: Perform complete spatial anchor reset
        if (universalSpatialAnchor != null)
        {
            LogDebug("Step 3: Performing complete spatial anchor reset...");
            universalSpatialAnchor.CompleteReset();
        }
        else
        {
            Debug.LogError("[InitAnchorManager] Cannot reset spatial anchor - UniversalSpatialAnchor reference is null!");
        }
        
        // Step 4: Re-enable auto-creation after a short delay
        if (universalSpatialAnchor != null)
        {
            LogDebug("Step 4: Re-enabling auto-creation...");
            StartCoroutine(ReEnableAutoCreateAfterDelay());
        }
        
        // Step 5: Force refresh to ensure clean state
        LogDebug("Step 5: Forcing reference refresh...");
        RefreshReferences();
        
        LogDebug("=== COMPLETE ANCHOR RESET FINISHED ===");
        LogDebug("System is now ready for fresh anchor placement.");
    }
    
    /// <summary>
    /// Coroutine to re-enable auto-creation after a short delay
    /// This prevents immediate recreation during the reset process
    /// </summary>
    private IEnumerator ReEnableAutoCreateAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Wait half a second
        
        if (universalSpatialAnchor != null)
        {
            universalSpatialAnchor.SetAutoCreateEnabled(true);
            LogDebug("Auto-creation re-enabled after reset delay.");
        }
    }
    
    // ======================== STATUS CHECK METHODS ========================
    
    /// <summary>
    /// Check if a spatial anchor currently exists
    /// </summary>
    public bool HasSpatialAnchor()
    {
        if (universalSpatialAnchor != null)
        {
            return universalSpatialAnchor.HasSpatialAnchor;
        }
        return false;
    }
    
    /// <summary>
    /// Check if the hand positioning is complete
    /// </summary>
    public bool IsAnchorPositionSet()
    {
        if (setUniversalAnchor != null)
        {
            return setUniversalAnchor.IsAnchorSet;
        }
        return false;
    }
    
    /// <summary>
    /// Check if spatial anchor is properly localized/tracking
    /// </summary>
    public bool IsSpatialAnchorLocalized()
    {
        if (universalSpatialAnchor != null)
        {
            return universalSpatialAnchor.IsSpatialAnchorLocalized();
        }
        return false;
    }
    
    /// <summary>
    /// Get current spatial anchor position (for debugging)
    /// </summary>
    public Vector3 GetSpatialAnchorPosition()
    {
        if (universalSpatialAnchor != null)
        {
            return universalSpatialAnchor.GetSpatialAnchorPosition();
        }
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get current spatial anchor rotation (for debugging)
    /// </summary>
    public Quaternion GetSpatialAnchorRotation()
    {
        if (universalSpatialAnchor != null)
        {
            return universalSpatialAnchor.GetSpatialAnchorRotation();
        }
        return Quaternion.identity;
    }
    
    // ======================== UI HELPER METHODS ========================
    
    /// <summary>
    /// Log current anchor status (useful for debugging buttons)
    /// </summary>
    public void LogAnchorStatus()
    {
        Debug.Log("=== ANCHOR STATUS ===");
        Debug.Log($"Hand Position Set: {IsAnchorPositionSet()}");
        Debug.Log($"Has Spatial Anchor: {HasSpatialAnchor()}");
        Debug.Log($"Spatial Anchor Localized: {IsSpatialAnchorLocalized()}");
        Debug.Log($"Spatial Anchor Position: {GetSpatialAnchorPosition()}");
        Debug.Log($"Spatial Anchor Rotation: {GetSpatialAnchorRotation().eulerAngles}");
        Debug.Log("===================");
    }
    
    /// <summary>
    /// Force refresh references (useful if scripts are created dynamically)
    /// </summary>
    public void RefreshReferences()
    {
        universalSpatialAnchor = FindFirstObjectByType<UniversalSpatialAnchor>();
        setUniversalAnchor = FindFirstObjectByType<SetUniversalAnchor>();
        
        LogDebug($"References refreshed - UniversalSpatialAnchor: {universalSpatialAnchor != null}, SetUniversalAnchor: {setUniversalAnchor != null}");
    }
    
    // ======================== PRIVATE HELPERS ========================
    
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[InitAnchorManager] {message}");
        }
    }
}
