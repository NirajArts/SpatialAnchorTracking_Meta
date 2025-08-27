using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NirajArts
{
    public class SetUniversalAnchor : MonoBehaviour
{
    [SerializeField] Transform rightHandController;
    [SerializeField] Vector3 anchorPosOffset;
    [SerializeField] Vector3 anchorRotOffset;
    [SerializeField] GameObject mainAnchor;
    [SerializeField] GameObject anchorSetGameObject; // GameObject to enable when anchor is set
    bool shouldFollow = false;
    float timeCounter = 0f;
    float timeToLock = 3f;
    Vector3 lastPos;
    [SerializeField] float moveThreshold = 0.5f;
    float currentVel;
    public bool useLerp = true;
    public float lerpSmoothTime = 1f;
    public float smoothTime = 1f;
    public static SetUniversalAnchor instance;
    public bool isAnchorSet = false;

    // Store anchor position and rotation for access by other scripts
    public static Vector3 SavedAnchorPosition { get; private set; }
    public static Quaternion SavedAnchorRotation { get; private set; }
     
    // Store center transform position and rotation for access by other scripts
    public static Vector3 SavedCenterPosition { get; private set; }
    public static Quaternion SavedCenterRotation { get; private set; }

    public bool IsAnchorSet { get { return isAnchorSet; } }

    public string menuSceneName;

    public Transform CenterTransform;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        mainAnchor.SetActive(false);
        
        // Initially disable the anchor set GameObject
        if (anchorSetGameObject != null)
        {
            anchorSetGameObject.SetActive(false);
        }
        
        transform.position = Vector3.one * 999f;
    }

    void LateUpdate()
    {
        if (!shouldFollow) return;

        transform.position = rightHandController.transform.position + anchorPosOffset;

        //Make the transition smooth
        if (useLerp)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, rightHandController.eulerAngles.y + anchorRotOffset.y, 0f), Time.deltaTime * lerpSmoothTime);
        else
            transform.rotation = Quaternion.Euler(0f, Mathf.SmoothDamp(transform.rotation.eulerAngles.y, rightHandController.eulerAngles.y + anchorRotOffset.y, ref currentVel, smoothTime), 0f);

        if (Vector3.Distance(lastPos, rightHandController.position) < moveThreshold)
        {
            if (timeCounter > timeToLock)
            {
                shouldFollow = false;
                // Save anchor position and rotation
                SavedAnchorPosition = GetPosition();
                SavedAnchorRotation = GetRotation();
                
                // Save center transform position and rotation
                if (CenterTransform != null)
                {
                    SavedCenterPosition = CenterTransform.position;
                    SavedCenterRotation = CenterTransform.rotation;
                }
                else
                {
                    Debug.LogWarning("SetUniversalAnchor: CenterTransform is not assigned! Center position/rotation will not be saved.");
                }
                
                isAnchorSet = true;
                
                // Enable the anchor set GameObject when anchor is set
                if (anchorSetGameObject != null)
                {
                    anchorSetGameObject.SetActive(true);
                    Debug.Log("SetUniversalAnchor: Anchor set GameObject enabled!");
                }
                else
                {
                    Debug.LogWarning("SetUniversalAnchor: anchorSetGameObject is not assigned!");
                }
            }
            else
                timeCounter += Time.deltaTime;
        }
        else
            timeCounter = 0f;

        lastPos = rightHandController.position;
    }

    public void EnablePositioningOfAnchor()
    {
        timeCounter = 0f;
        shouldFollow = true;
        mainAnchor.SetActive(true);
    }

    public void RepositionTable()
    {
        timeCounter = 0f;
        shouldFollow = true;
        mainAnchor.SetActive(true);
    }

    public void SetAnchor()
    {
        if (isAnchorSet)
        {
            mainAnchor.SetActive(false);
            
            // Also disable the anchor set GameObject when transitioning to menu scene
            if (anchorSetGameObject != null)
            {
                anchorSetGameObject.SetActive(false);
                Debug.Log("SetUniversalAnchor: Anchor set GameObject disabled before scene transition!");
            }
            
            // Load menuSceneName
            if (!string.IsNullOrEmpty(menuSceneName))
            {
                SceneManager.LoadScene(menuSceneName);
            }
            else
            {
                Debug.LogWarning("SetUniversalAnchor: menuSceneName is not set! Please assign a scene name in the inspector.");
            }
        }
    }

    public Vector3 GetPosition()
    {
        return (transform.GetChild(0).position + Vector3.up * -transform.GetChild(0).localPosition.y);
    }
    public Quaternion GetRotation()
    {
        return (transform.rotation);
    }
    
    /// <summary>
    /// Reset the anchor positioning system to initial state
    /// This will clear isAnchorSet and reset all positioning variables
    /// </summary>
    public void ResetAnchorPosition()
    {
        Debug.Log("SetUniversalAnchor: Resetting anchor position system...");
        
        // Reset all state variables
        shouldFollow = false;
        isAnchorSet = false;
        timeCounter = 0f;
        
        // Reset transform position to initial state
        transform.position = Vector3.one * 999f;
        
        // Disable main anchor
        mainAnchor.SetActive(false);
        
        // Disable the anchor set GameObject
        if (anchorSetGameObject != null)
        {
            anchorSetGameObject.SetActive(false);
            Debug.Log("SetUniversalAnchor: Anchor set GameObject disabled during reset!");
        }
        
        // Clear saved positions
        SavedAnchorPosition = Vector3.zero;
        SavedAnchorRotation = Quaternion.identity;
        SavedCenterPosition = Vector3.zero;
        SavedCenterRotation = Quaternion.identity;
        
        Debug.Log("SetUniversalAnchor: Anchor position system reset complete!");
    }
}
}
