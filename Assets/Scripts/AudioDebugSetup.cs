using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AudioDebugSetup : MonoBehaviour
{
    // This script is just a helper to add the AudioDebugger to a scene
    
#if UNITY_EDITOR
    [MenuItem("Tools/Audio/Add Audio Debugger to Scene")]
    public static void AddAudioDebuggerToScene()
    {
        // Check if an AudioDebugger already exists in the scene
        AudioDebugger existingDebugger = Object.FindObjectOfType<AudioDebugger>();
        
        if (existingDebugger != null)
        {
            Debug.Log("AudioDebugger already exists in the scene.");
            Selection.activeGameObject = existingDebugger.gameObject;
            return;
        }
        
        // Create a new GameObject for the debugger
        GameObject debuggerObj = new GameObject("Audio Debugger");
        AudioDebugger debugger = debuggerObj.AddComponent<AudioDebugger>();
        
        // Mark the scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        // Select the new GameObject
        Selection.activeGameObject = debuggerObj;
        
        Debug.Log("AudioDebugger added to the scene. Press F12 in play mode to toggle the debug UI.");
    }
    
    [MenuItem("Tools/Audio/Add Simple Audio Player to Scene")]
    public static void AddSimpleAudioPlayerToScene()
    {
        // Check if a SimpleAudioPlayer already exists in the scene
        SimpleAudioPlayer existingPlayer = Object.FindObjectOfType<SimpleAudioPlayer>();
        
        if (existingPlayer != null)
        {
            Debug.Log("SimpleAudioPlayer already exists in the scene.");
            Selection.activeGameObject = existingPlayer.gameObject;
            return;
        }
        
        // Create a new GameObject for the player
        GameObject playerObj = new GameObject("Simple Audio Player");
        SimpleAudioPlayer player = playerObj.AddComponent<SimpleAudioPlayer>();
        
        // Try to find audio clips in the project
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");
        if (audioGuids.Length > 0)
        {
            // Get up to 5 audio clips
            int clipCount = Mathf.Min(audioGuids.Length, 5);
            AudioClip[] clips = new AudioClip[clipCount];
            
            for (int i = 0; i < clipCount; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(audioGuids[i]);
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            
            player.audioClips = clips;
            Debug.Log($"Found and assigned {clipCount} audio clips to SimpleAudioPlayer");
        }
        
        // Mark the scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        // Select the new GameObject
        Selection.activeGameObject = playerObj;
        
        Debug.Log("SimpleAudioPlayer added to the scene. Press P to play/pause, N to change clips.");
    }
    
    [MenuItem("Tools/Audio/Add Basic Audio Tester to Scene")]
    public static void AddAudioTesterToScene()
    {
        // Check if an AudioTester already exists in the scene
        AudioTester existingTester = Object.FindObjectOfType<AudioTester>();
        
        if (existingTester != null)
        {
            Debug.Log("AudioTester already exists in the scene.");
            Selection.activeGameObject = existingTester.gameObject;
            return;
        }
        
        // Create a new GameObject for the tester
        GameObject testerObj = new GameObject("Audio Tester");
        AudioTester tester = testerObj.AddComponent<AudioTester>();
        
        // Try to find audio clips in the project
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");
        if (audioGuids.Length > 0)
        {
            // Get the first audio clip
            string path = AssetDatabase.GUIDToAssetPath(audioGuids[0]);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            
            tester.testClip = clip;
            Debug.Log($"Found and assigned audio clip {clip.name} to AudioTester");
        }
        
        // Mark the scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        // Select the new GameObject
        Selection.activeGameObject = testerObj;
        
        Debug.Log("AudioTester added to the scene. Press Space to play/stop the test clip.");
    }
    
    [MenuItem("Tools/Audio/Fix Audio NPCs in Scene")]
    public static void FixAudioNPCsInScene()
    {
        AudioInteractableAdvanced[] npcs = Object.FindObjectsOfType<AudioInteractableAdvanced>();
        
        if (npcs.Length == 0)
        {
            Debug.Log("No AudioInteractableAdvanced NPCs found in the scene.");
            return;
        }
        
        int fixedCount = 0;
        
        foreach (AudioInteractableAdvanced npc in npcs)
        {
            // Check if the NPC already has an AudioSource component
            AudioSource audioSource = npc.GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                // Add AudioSource component
                audioSource = npc.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0;
                
                // Try to get the clip from the serialized field
                SerializedObject serializedNPC = new SerializedObject(npc);
                SerializedProperty clipProperty = serializedNPC.FindProperty("musicClip");
                
                if (clipProperty != null && clipProperty.objectReferenceValue != null)
                {
                    audioSource.clip = clipProperty.objectReferenceValue as AudioClip;
                    Debug.Log($"Fixed {npc.gameObject.name}: Added AudioSource with clip {audioSource.clip.name}");
                }
                else
                {
                    Debug.LogWarning($"Fixed {npc.gameObject.name}: Added AudioSource but could not find clip reference");
                }
                
                fixedCount++;
            }
        }
        
        // Mark the scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        Debug.Log($"Fixed {fixedCount} of {npcs.Length} Audio NPCs in the scene.");
    }
    
    [MenuItem("Tools/Audio/Check Audio Listener")]
    public static void CheckAudioListener()
    {
        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            Debug.LogError("No AudioListener found in the scene! Audio will not be heard.");
            
            // Try to find the main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Add an AudioListener to the main camera if it doesn't have one
                if (mainCamera.GetComponent<AudioListener>() == null)
                {
                    mainCamera.gameObject.AddComponent<AudioListener>();
                    Debug.Log("Added AudioListener to main camera.");
                    
                    // Mark the scene as dirty
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
            else
            {
                Debug.LogError("No main camera found. Please add an AudioListener to a GameObject in your scene.");
            }
        }
        else if (listeners.Length > 1)
        {
            Debug.LogWarning($"Multiple AudioListeners found in the scene ({listeners.Length}). This can cause issues with audio playback.");
            
            // List all objects with AudioListener
            foreach (AudioListener listener in listeners)
            {
                Debug.Log($"AudioListener found on: {listener.gameObject.name}");
            }
        }
        else
        {
            Debug.Log($"One AudioListener found on: {listeners[0].gameObject.name}");
        }
    }
#endif
} 