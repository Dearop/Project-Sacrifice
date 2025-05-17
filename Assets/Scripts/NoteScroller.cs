using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro
using System.Collections.Generic; // For List

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
            // Calculate time for notes to travel from spawn to hit zone center
            // Assuming notes spawn at noteTrack.rect.width / 2f (right edge if pivot is center)
            // And hitZone is centered at X=0 within noteTrack's local coordinates.
            // More robust: calculate distance between spawn point's world X and hit zone's world X.
            float spawnX = noteTrack.rect.width / 2f;
            // Let's assume hit zone is at X=0 for simplicity in spawn calculation (adjust if hitZone.anchoredPosition.x is different)
            float distanceToTravel = spawnX; 
            timeToReachHitZone = distanceToTravel / scrollSpeed;
            
            StartSong();
        }
        else
        {    
            Debug.LogError("CurrentSong or its AudioClip is not assigned in NoteScroller.");
        }
    }

    public void StartSong()
    {
        if (currentSong == null || currentSong.songClip == null) return;

        audioSource.clip = currentSong.songClip;
        audioSource.Play();
        songStartTime = Time.time;
        nextNoteIndex = 0;
        songPlaying = true;
        Debug.Log($"Starting song: {currentSong.name}. Time to reach hit zone: {timeToReachHitZone}s");
    }

    void Update()
    {
        if (!songPlaying || currentSong == null) 
        {
            // Remove test spawning if a song is meant to be playing
            // if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnSingleNote("S", 0);
            // if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnDoubleNote("D", "F", 1);
            return;
        }

        float currentSongTime = Time.time - songStartTime;

        // Spawn notes based on song data
        while (nextNoteIndex < currentSong.notes.Count)
        {
            SongNoteInfo noteInfo = currentSong.notes[nextNoteIndex];
            float noteHitTime = noteInfo.timestamp;
            float noteSpawnTime = noteHitTime - timeToReachHitZone;

            if (currentSongTime >= noteSpawnTime)
            {
                if (noteInfo.gameplayType == NoteGameplayType.Single)
                {
                    SpawnSingleNote(noteInfo.keyToPress1, noteInfo.pitchLevel);
                }
                else // Double
                {
                    SpawnDoubleNote(noteInfo.keyToPress1, noteInfo.keyToPress2, noteInfo.pitchLevel);
                }
                nextNoteIndex++;
            }
            else
            {
                // Notes are sorted, so if this one isn't ready, subsequent ones won't be either
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
            float startX = noteTrack.rect.width / 2f;
            
            // Calculate Y position based on pitchLevel and numberOfVerticalLanes from currentSong
            float yPos = 0f;
            int numLanes = currentSong.numberOfVerticalLanes;
            if (numLanes > 1)
            {
                // Ensure pitchLevel is within valid range (0 to numLanes - 1)
                int clampedPitchLevel = Mathf.Clamp(pitchLevel, 0, numLanes - 1);
                yPos = Mathf.Lerp(-noteTrack.rect.height / 2f, noteTrack.rect.height / 2f, (float)clampedPitchLevel / (numLanes - 1));
            }
            // If numLanes is 1, yPos remains 0 (center).

            noteRectTransform.anchoredPosition = new Vector2(startX, yPos);
            
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

                if (firstPartHitForDouble && !alreadyValidated) 
                {
                    Debug.Log($"Double note {noteData.letter1}-{noteData.letter2} scrolled off after first hit (second part missed).");
                }
                else if (noteData.isDoubleNote && !firstPartHitForDouble && !alreadyValidated) 
                {
                     Debug.Log($"Double note {noteData.letter1}-{noteData.letter2} scrolled off (completely missed).");
                }
                else if (!noteData.isDoubleNote && !alreadyValidated)
                {
                    Debug.Log($"Single note {noteData.letter1} scrolled off (missed).");
                }
                Destroy(noteData.gameObject);
                activeNotes.RemoveAt(i);
            }
        }
    }
} 