using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    [SerializeField] private bool showLoadingLog = true;
    
    [Header("Delay Settings")]
    [SerializeField] public float delay = 2.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Loads a scene by name
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Loading scene '{sceneName}'");
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Loads a scene by name asynchronously
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    public void LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }
    
    /// <summary>
    /// Loads a scene additively (without unloading current scene)
    /// </summary>
    /// <param name="sceneName">The name of the scene to load additively</param>
    public void LoadSceneAdditive(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Loading scene '{sceneName}' additively");
        }
        
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    
    /// <summary>
    /// Reloads the current active scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Unloads a scene by name
    /// </summary>
    /// <param name="sceneName">The name of the scene to unload</param>
    public void UnloadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Unloading scene '{sceneName}'");
        }
        
        SceneManager.UnloadSceneAsync(sceneName);
    }
    
    /// <summary>
    /// Loads a scene by name with a delay (useful for animations/effects)
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    public void LoadWithDelay(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        StartCoroutine(LoadWithDelayCoroutine(sceneName, false));
    }
    
    /// <summary>
    /// Loads a scene by name with a delay asynchronously (useful for animations/effects)
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    public void LoadWithDelayAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ScenesManager: Scene name cannot be null or empty!");
            return;
        }
        
        StartCoroutine(LoadWithDelayCoroutine(sceneName, true));
    }
    
    /// <summary>
    /// Coroutine for loading scenes asynchronously
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    /// <returns></returns>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Loading scene '{sceneName}' asynchronously");
        }
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // You can add loading progress here if needed
            // float progress = asyncLoad.progress;
            yield return null;
        }
        
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Scene '{sceneName}' loaded successfully");
        }
    }
    
    /// <summary>
    /// Coroutine for loading scenes with delay (for animations/effects)
    /// </summary>
    /// <param name="sceneName">The name of the scene to load</param>
    /// <param name="useAsync">Whether to use async loading or not</param>
    /// <returns></returns>
    private IEnumerator LoadWithDelayCoroutine(string sceneName, bool useAsync)
    {
        if (showLoadingLog)
        {
            Debug.Log($"ScenesManager: Starting delayed loading for scene '{sceneName}'. Delay: {delay} seconds");
        }
        
        // Wait for the specified delay (perfect time for animations/effects)
        yield return new WaitForSeconds(delay);
        
        if (useAsync)
        {
            // Load asynchronously
            if (showLoadingLog)
            {
                Debug.Log($"ScenesManager: Delay finished, loading scene '{sceneName}' asynchronously");
            }
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            if (showLoadingLog)
            {
                Debug.Log($"ScenesManager: Scene '{sceneName}' loaded successfully after delay");
            }
        }
        else
        {
            // Load synchronously
            if (showLoadingLog)
            {
                Debug.Log($"ScenesManager: Delay finished, loading scene '{sceneName}' synchronously");
            }
            
            SceneManager.LoadScene(sceneName);
        }
    }
}
