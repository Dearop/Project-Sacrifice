using UnityEngine;

// Enum to define if a note is single or double
public enum NoteGameplayType 
{
    Single,
    Double
}

// Class to hold information about each note in a song
[System.Serializable]
public class SongNoteInfo
{
    [Tooltip("Time in seconds from the start of the song when this note should align with the hit zone.")]
    public float timestamp;

    [Tooltip("Type of note: Single or Double.")]
    public NoteGameplayType gameplayType = NoteGameplayType.Single;

    [Tooltip("The primary keyboard key to press for this note (e.g., \"A\").")]
    public string keyToPress1;

    [Tooltip("The second keyboard key for Double notes (leave empty for Single notes).")]
    public string keyToPress2; // Only used if gameplayType is Double

    [Tooltip("Relative pitch level (e.g., 0 for lowest, 4 for highest on a 5-lane track). Higher values appear higher on screen.")]
    [Range(0, 10)] // Max 11 lanes, adjustable range as needed
    public int pitchLevel = 0; 
} 