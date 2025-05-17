using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro
using System.Collections.Generic; // For List
using System.Collections; // For Coroutines

[RequireComponent(typeof(AudioSource))] // Ensure an AudioSource component is present
public class NoteScroller : MonoBehaviour
{
    [Header("Song Configuration")]
    [Tooltip("The SongData asset containing the notes and audio for the current song.")]
    public SongData currentSong;

    [Header("Game Elements")]
    [Tooltip("The UI Image prefab for a single note.")]
    public GameObject singleNotePrefab;
    [Tooltip("The UI Image prefab for a double note (requiring two presses).")]
    public GameObject doubleNotePrefab;
    [Tooltip("The Transform of the UI Panel where notes will be spawned and scroll.")]
    public RectTransform noteTrack;
    [Tooltip("The UI Panel that acts as the hit zone for notes.")]
    public RectTransform hitZone; // Assign your Hit Zone Panel here
    [Tooltip("A UI element (e.g., an invisible panel, direct child of NoteTrack) marking the Y position of the TOP-MOST note lane.")]
    public RectTransform topLaneMarker; // New field
    [Tooltip("A UI element (e.g., an invisible panel, direct child of NoteTrack) marking the Y position of the BOTTOM-MOST note lane.")]
    public RectTransform bottomLaneMarker; // New field

    [Header("Timing & Positioning Markers (Child of NoteTrack)")]
    [Tooltip("A UI element (child of NoteTrack) marking the X-coordinate where notes SPAWN.")]
    public RectTransform spawnLineMarkerX; // New
    [Tooltip("A UI element (child of NoteTrack) marking the X-coordinate of the HIT LINE (can be the same as HitZone or a more precise child).")]
    public RectTransform hitLineMarkerX; // New

    [Header("Countdown Settings")]
    [Tooltip("The TextMeshPro component to display countdown text.")]
    public TextMeshProUGUI countdownText;
    [Tooltip("Initial delay in seconds before the song starts.")]
    public float initialDelay = 3f;
    [Tooltip("Time each countdown message is displayed (in seconds).")]
    public float countdownTextDuration = 0.8f;
    [Tooltip("Scale animation amount for countdown text.")]
    public float countdownTextScalePunch = 1.5f;

    [Header("Gameplay Settings")]
    [Tooltip("Speed at which notes scroll from right to left.")]
    public float scrollSpeed = 200f;

    [Header("Effects")]
    [Tooltip("Particle system to play on successful note hit.")]
    public GameObject successParticlePrefab; // New Field
    [Tooltip("Particle system to play when a note is missed.")]
    public GameObject failParticlePrefab; // New Field
    [Tooltip("The Z-depth from the main camera at which particles should spawn.")]
    public float particleSpawnDepth = 10f; // New field for Z-depth

    // --- Song Playback State ---
    private AudioSource audioSource;
    private float songStartTime = -1f;
    private int nextNoteIndex = 0;
    private float timeToReachHitZone = 0f;
    private bool songPlaying = false;
    private float lastLoggedAudioTime = -1f; // For debug logging audio time
    private bool countdownActive = false;

    private Camera mainCamera; // Cache the main camera

    // List to keep track of active notes' data
    private List<NoteData> activeNotes = new List<NoteData>();

    void Start()
    {
        mainCamera = Camera.main; // Get the main camera
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Particle effects might not position correctly.");
        }

        audioSource = GetComponent<AudioSource>();

        if (currentSong != null && currentSong.songClip != null)
        {
            // --- Calculate timeToReachHitZone using explicit X markers --- 
            if (spawnLineMarkerX == null || hitLineMarkerX == null)
            {
                Debug.LogError("[NoteScroller] SpawnLineMarkerX or HitLineMarkerX is not assigned! Cannot calculate note travel time accurately. Please assign these as children of NoteTrack.");
                // Fallback or stop, for now, let's make travel time huge so it's obvious
                timeToReachHitZone = float.MaxValue; 
            }
            else if (spawnLineMarkerX.parent != noteTrack || hitLineMarkerX.parent != noteTrack)
            {
                 Debug.LogError("[NoteScroller] SpawnLineMarkerX and HitLineMarkerX MUST be direct children of NoteTrack for accurate timing calculations with this method.");
                 timeToReachHitZone = float.MaxValue;
            }
            else
            {
                float spawnX_local = spawnLineMarkerX.anchoredPosition.x;
                float hitX_local = hitLineMarkerX.anchoredPosition.x;
                float distanceToTravel = Mathf.Abs(spawnX_local - hitX_local);
                
                if (scrollSpeed <= 0) 
                {
                    Debug.LogError("ScrollSpeed must be greater than 0 for timing calculations.");
                    timeToReachHitZone = float.MaxValue; 
                }
                else
                {
                    timeToReachHitZone = distanceToTravel / scrollSpeed;
                }
                Debug.Log($"[NoteScroller] TIMING (Explicit Markers): Distance to travel: {distanceToTravel} (using anchoredX of markers within NoteTrack). Time to reach hit zone: {timeToReachHitZone}s. ScrollSpeed: {scrollSpeed}");
                Debug.Log($"SpawnLineMarkerX.anchoredPosition.x: {spawnX_local}, HitLineMarkerX.anchoredPosition.x: {hitX_local}");
            }
            // --- End of new timing calculation ---
            
            StartSong();
        }
        else
        {    
            Debug.LogError("CurrentSong or its AudioClip is not assigned in NoteScroller.");
        }

        if (topLaneMarker == null || bottomLaneMarker == null)
        {
            Debug.LogWarning("TopLaneMarker or BottomLaneMarker is not assigned. Vertical note positioning might use fallback.");
        }

        // Initialize countdown text        
        if (countdownText != null)        
        {            
            // Make sure we can reference it later, but hide it initially            
            countdownText.gameObject.SetActive(true);            
            Debug.Log("[NoteScroller] Countdown text found and initialized.");        
        }        
        else        
        {            
            Debug.LogWarning("[NoteScroller] Countdown text is not assigned. Will start song without visual countdown.");        
        }                
        
        // Make sure initialDelay is positive        
        if (initialDelay <= 0)        
        {            
            Debug.LogWarning("[NoteScroller] Initial delay is set to 0 or negative. Song will start immediately without countdown.");        
        }
    }

    public void StartSong()    
    {        
        if (currentSong == null || currentSong.songClip == null || timeToReachHitZone == float.MaxValue)         
        {            
            if (timeToReachHitZone == float.MaxValue) Debug.LogError("[NoteScroller] Cannot start song, timeToReachHitZone calculation failed due to marker setup.");            
            return;        
        }                
        
        audioSource.clip = currentSong.songClip;                
        
        // Start countdown instead of playing immediately        
        if (initialDelay > 0)        
        {            
            // Reset countdown state to make sure it runs
            countdownActive = false;
            StartCoroutine(CountdownAndStartSong());        
        }        
        else        
        {            
            // If no delay, start immediately            
            PlaySongImmediately();        
        }    
    }

    private IEnumerator CountdownAndStartSong()
    {
        countdownActive = true;
        
        if (countdownText != null)
        {
            // Make sure the text is visible
            countdownText.gameObject.SetActive(true);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, 1f); // Ensure full opacity
            
            Debug.Log("[NoteScroller] Countdown started - Ready");
            // Show "Ready"
            countdownText.text = "Ready";
            yield return AnimateCountdownText();
            
            Debug.Log("[NoteScroller] Countdown - Set");
            // Show "Set"
            countdownText.text = "Set";  
            yield return AnimateCountdownText();
            
            Debug.Log("[NoteScroller] Countdown - Go!");
            // Show "Go"
            countdownText.text = "Go!";
            yield return AnimateCountdownText();
            
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[NoteScroller] Countdown text is null, using time delay only");
            // If no countdown text is assigned, just wait for the delay
            yield return new WaitForSeconds(initialDelay);
        }
        
        Debug.Log("[NoteScroller] Countdown finished, starting song");
        PlaySongImmediately();
        countdownActive = false;
    }
    
    private IEnumerator AnimateCountdownText()
    {
        if (countdownText != null)
        {
            // Simple scale punch animation
            Vector3 originalScale = countdownText.transform.localScale;
            Vector3 targetScale = originalScale * countdownTextScalePunch;
            
            float elapsedTime = 0f;
            float halfDuration = countdownTextDuration * 0.5f;
            
            // Scale up
            while (elapsedTime < halfDuration)
            {
                float t = elapsedTime / halfDuration;
                countdownText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            elapsedTime = 0f;
            
            // Scale down
            while (elapsedTime < halfDuration)
            {
                float t = elapsedTime / halfDuration;
                countdownText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure we end at original scale
            countdownText.transform.localScale = originalScale;
            
            // Wait any remaining time
            if (countdownTextDuration > 0)
            {
                yield return new WaitForSeconds(countdownTextDuration - (halfDuration * 2));
            }
        }
        else
        {
            yield return new WaitForSeconds(countdownTextDuration);
        }
    }
    
    private void PlaySongImmediately()
    {
        audioSource.Play();
        nextNoteIndex = 0;
        songPlaying = true;
        lastLoggedAudioTime = -1f; // Reset for new song
    }

    void Update()
    {
        if (!songPlaying || currentSong == null || audioSource == null) 
        {
            return;
        }

        // Check if song actually finished playing
        if (!audioSource.isPlaying && audioSource.time >= audioSource.clip.length - 0.1f) // Check if near or at the end
        {
            songPlaying = false;
            Debug.Log("[NoteScroller] Song finished playing based on audioSource state.");
            return;
        }
        if (!audioSource.isPlaying && nextNoteIndex >= currentSong.notes.Count) // All notes spawned and audio stopped
        {
             songPlaying = false;
             Debug.Log("[NoteScroller] Song finished: All notes spawned and audio source stopped.");
             return;
        }

        float currentSongTime = audioSource.time; // Use AudioSource's time directly

        // Optional: Log audio time periodically to check if it's advancing
        if (Time.frameCount % 300 == 0 && currentSongTime != lastLoggedAudioTime) // Log every 5 seconds approx if time changed
        {
            Debug.Log($"[NoteScroller] Audio time: {currentSongTime:F2}s");
            lastLoggedAudioTime = currentSongTime;
        }

        while (nextNoteIndex < currentSong.notes.Count)
        {
            SongNoteInfo noteInfo = currentSong.notes[nextNoteIndex];
            float noteHitTime = noteInfo.timestamp; // This is when the note should be AT the hit zone
            
            // FIXED: Spawn notes at the actual hit time - this will make them spawn at the hit line
            // and then we'll immediately position them at the spawn point
            if (currentSongTime >= noteHitTime)
            {
                Debug.Log($"[NoteScroller] Spawning note. Hit Time: {noteHitTime:F2}s. Current Audio Time: {currentSongTime:F2}s. Key: {noteInfo.keyToPress1}");
                
                // Determine which prefab to use based on gameplayType
                GameObject prefabToUse = (noteInfo.gameplayType == NoteGameplayType.Single) ? singleNotePrefab : doubleNotePrefab;
                bool isDouble = (noteInfo.gameplayType == NoteGameplayType.Double);
                string key2 = isDouble ? noteInfo.keyToPress2 : null;

                SpawnAndPositionNote(prefabToUse, isDouble, noteInfo.keyToPress1, noteInfo.pitchLevel, key2);
                nextNoteIndex++;
            }
            else
            {
                break; 
            }
        }

        ProcessPlayerInput();
        MoveNotes();
        CleanupNotes(); 
    }

    private void ProcessPlayerInput()
    {
        if (!Input.anyKeyDown) return; 
        for (char c = 'A'; c <= 'Z'; c++)
        {
            if (Input.GetKeyDown(KeyCodeHelper.GetKeyCodeFromString(c.ToString())))
            {
                CheckNoteHit(c.ToString());
                break; 
            }
        }
    }
    
    public static class KeyCodeHelper 
    {
        private static Dictionary<string, KeyCode> keyMapping = new Dictionary<string, KeyCode>();
        static KeyCodeHelper()
        {
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                keyMapping[kc.ToString().ToUpper()] = kc;
            }
        }
        public static KeyCode GetKeyCodeFromString(string keyName)
        {
            keyName = keyName.ToUpper();
            return keyMapping.ContainsKey(keyName) ? keyMapping[keyName] : KeyCode.None;
        }
    }

    private void PlayParticleEffectAtRectTransform(GameObject particlePrefab, RectTransform targetRect, float depth)
    {
        if (particlePrefab == null || mainCamera == null || targetRect == null) return;
        Vector3[] worldCorners = new Vector3[4];
        targetRect.GetWorldCorners(worldCorners);
        Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldCenter);
        Vector3 spawnPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
        Instantiate(particlePrefab, spawnPosition, Quaternion.identity);
    }

    private void CheckNoteHit(string keyPressed)
    {
        if (hitZone == null) return;
        Rect hitZoneWorldRect = GetWorldRect(hitZone); 

        for (int i = activeNotes.Count - 1; i >= 0; i--) 
        {
            NoteData note = activeNotes[i];
            if (note == null || note.isValidated) continue; 

            RectTransform noteContainerRectTransform = note.GetComponent<RectTransform>(); 
            if (noteContainerRectTransform == null) 
            {
                activeNotes.RemoveAt(i); 
                continue;
            }

            RectTransform targetTextRectTransform = null;
            if (note.isDoubleNote)
            {
                targetTextRectTransform = !note.firstLetterHit ? 
                    (note.textMesh1 != null ? note.textMesh1.rectTransform : null) :
                    (note.textMesh2 != null ? note.textMesh2.rectTransform : null);
            }
            else 
            {
                targetTextRectTransform = note.textMesh1 != null ? note.textMesh1.rectTransform : null;
            }

            if (targetTextRectTransform == null) continue; 

            Rect targetTextWorldRect = GetWorldRect(targetTextRectTransform); 
            bool isOverlapping = targetTextWorldRect.Overlaps(hitZoneWorldRect);

            if (isOverlapping)
            {
                bool noteDestroyedThisCheck = false;
                if (note.isDoubleNote)
                {
                    if (!note.firstLetterHit && keyPressed.Equals(note.letter1, System.StringComparison.OrdinalIgnoreCase))
                    {
                        note.HitFirstLetter();
                        PlayParticleEffectAtRectTransform(successParticlePrefab, targetTextRectTransform, particleSpawnDepth);
                    }
                    else if (note.firstLetterHit && keyPressed.Equals(note.letter2, System.StringComparison.OrdinalIgnoreCase))
                    {
                        note.ValidateNote(); 
                        PlayParticleEffectAtRectTransform(successParticlePrefab, targetTextRectTransform, particleSpawnDepth);
                        Destroy(note.gameObject);
                        activeNotes.RemoveAt(i);
                        noteDestroyedThisCheck = true;
                    }
                }
                else 
                {
                    if (keyPressed.Equals(note.letter1, System.StringComparison.OrdinalIgnoreCase))
                    {
                        note.ValidateNote();
                        PlayParticleEffectAtRectTransform(successParticlePrefab, targetTextRectTransform, particleSpawnDepth);
                        Destroy(note.gameObject);
                        activeNotes.RemoveAt(i);
                        noteDestroyedThisCheck = true;
                    }
                }
                if (note.firstLetterHit || noteDestroyedThisCheck) break; 
            }
        }
    }

    public static Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[1].y - corners[0].y);
    }

    private void SpawnAndPositionNote(GameObject prefabToSpawn, bool isDouble, string letter1, int pitchLevel, string letter2 = null)
    {
        if (prefabToSpawn == null || noteTrack == null || currentSong == null) return;

        GameObject newNoteObject = Instantiate(prefabToSpawn, noteTrack);
        RectTransform noteRectTransform = newNoteObject.GetComponent<RectTransform>();
        NoteData noteData = newNoteObject.GetComponent<NoteData>();

        if (noteRectTransform != null && noteData != null)
        {
            // Horizontal Spawn Position: Always position notes directly at the hit line
            float hitX;
            if (hitLineMarkerX != null && hitLineMarkerX.parent == noteTrack) 
            {
                hitX = spawnLineMarkerX.anchoredPosition.x;
            }
            else
            {
                // Fallback if hitLineMarkerX isn't set up correctly
                hitX = 0; // Center of the track
            }

            float yPos = 0f;
            int numLanes = currentSong.numberOfVerticalLanes;

            if (topLaneMarker != null && bottomLaneMarker != null)
            {
                Vector3 noteTrackPivotWorldPos = noteTrack.position;
                Vector3 topMarkerWorldPos = topLaneMarker.position;
                Vector3 bottomMarkerWorldPos = bottomLaneMarker.position;
                Vector3 topReferenceWorld = new Vector3(noteTrackPivotWorldPos.x, topMarkerWorldPos.y, noteTrackPivotWorldPos.z);
                Vector3 bottomReferenceWorld = new Vector3(noteTrackPivotWorldPos.x, bottomMarkerWorldPos.y, noteTrackPivotWorldPos.z);
                float minY_local = noteTrack.InverseTransformPoint(bottomReferenceWorld).y;
                float maxY_local = noteTrack.InverseTransformPoint(topReferenceWorld).y;
                
                if (maxY_local < minY_local)
                {
                    (minY_local, maxY_local) = (maxY_local, minY_local); 
                }

                if (numLanes > 1)
                {
                    int clampedPitchLevel = Mathf.Clamp(pitchLevel, 0, numLanes - 1);
                    yPos = Mathf.Lerp(minY_local, maxY_local, (float)clampedPitchLevel / (numLanes - 1));
                }
                else 
                {
                    yPos = (minY_local + maxY_local) / 2f;
                }
            }
            else
            {
                if (numLanes > 1)
                {
                    int clampedPitchLevel = Mathf.Clamp(pitchLevel, 0, numLanes - 1);
                    yPos = Mathf.Lerp(-noteTrack.rect.height / 2f, noteTrack.rect.height / 2f, (float)clampedPitchLevel / (numLanes - 1));
                }
            }

            noteRectTransform.anchoredPosition = new Vector2(hitX, yPos);
            
            TextMeshProUGUI tm1 = null, tm2 = null;
            TextMeshProUGUI[] texts = newNoteObject.GetComponentsInChildren<TextMeshProUGUI>(true); 
            if (texts.Length > 0) tm1 = texts[0];
            if (isDouble && texts.Length > 1) 
            {
                tm2 = texts[1];
                if(texts[0] == texts[1] && texts.Length > 2) tm2 = texts[2];
            }

            noteData.Initialize(letter1, tm1, isDouble, letter2, tm2);
            activeNotes.Add(noteData);
        }
        else
        {
            Destroy(newNoteObject);
        }
    }

    // Updated public spawn methods to include pitchLevel (though they'll primarily be called by song data now)
    public void SpawnSingleNote(string letter, int pitchLevel)
    {
        SpawnAndPositionNote(singleNotePrefab, false, letter, pitchLevel);
    }

    public void SpawnDoubleNote(string letter1, string letter2, int pitchLevel)
    {
        SpawnAndPositionNote(doubleNotePrefab, true, letter1, pitchLevel, letter2);
    }

    void MoveNotes()
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            NoteData note = activeNotes[i];
            if (note == null || note.GetComponent<RectTransform>() == null || note.isValidated) 
            {
                if(note == null && i < activeNotes.Count) activeNotes.RemoveAt(i);
                continue;
            }
            RectTransform noteRect = note.GetComponent<RectTransform>();
            noteRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;
        }
    }

    void CleanupNotes() 
    {
        if (noteTrack == null) return;
        float trackLeftEdgeWorldX = GetWorldRect(noteTrack).xMin; 

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            NoteData noteData = activeNotes[i];
            if (noteData == null) { 
                if (i < activeNotes.Count) activeNotes.RemoveAt(i); 
                continue;
            }

            RectTransform noteRectTransform = noteData.GetComponent<RectTransform>();
            if (noteRectTransform == null) { 
                if (i < activeNotes.Count) activeNotes.RemoveAt(i);
                continue;
            }
            
            RectTransform overallNoteRectForFail = noteData.GetComponent<RectTransform>();
            Rect noteWorldRect = GetWorldRect(noteRectTransform); 

            if (noteWorldRect.xMax < trackLeftEdgeWorldX) 
            {
                bool alreadyValidated = noteData.isValidated; 
                bool firstPartHitForDouble = noteData.isDoubleNote && noteData.firstLetterHit;

                if (!alreadyValidated && overallNoteRectForFail != null) 
                { 
                    PlayParticleEffectAtRectTransform(failParticlePrefab, overallNoteRectForFail, particleSpawnDepth);
                }

                Destroy(noteData.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
    }
} 