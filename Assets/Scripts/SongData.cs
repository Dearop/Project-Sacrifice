using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSongData", menuName = "Rhythm Game/Song Data", order = 1)]
public class SongData : ScriptableObject
{
    [Header("Song Info")]
    [Tooltip("The audio clip for the song.")]
    public AudioClip songClip;

    [Tooltip("Beats Per Minute of the song. Can be used for beat-based quantization or effects (optional).")]
    public float bpm = 120f;

    [Tooltip("The total number of distinct vertical lanes/pitch levels available on the note track (e.g., 5 for 5 lanes).")]
    [Range(1, 11)] // Min 1 lane, max 11 lanes (0-10 pitchLevel)
    public int numberOfVerticalLanes = 5;

    [Header("Note Sequence")]
    [Tooltip("List of notes in the song, should be ordered by timestamp if not automatically sorted later.")]
    public List<SongNoteInfo> notes = new List<SongNoteInfo>();

    // Optional: You could add a method here to sort notes by timestamp if needed during editor time or OnValidate
    private void OnValidate()
    {
        // Ensure notes are sorted by timestamp for easier processing
        // This is helpful if notes are added out of order in the editor.
        notes.Sort((note1, note2) => note1.timestamp.CompareTo(note2.timestamp));

        // Validate pitch levels against numberOfVerticalLanes
        if (numberOfVerticalLanes < 1) numberOfVerticalLanes = 1;
        foreach (var note in notes)
        {
            if (note.pitchLevel >= numberOfVerticalLanes)
            {
                note.pitchLevel = numberOfVerticalLanes - 1;
                Debug.LogWarning($"SongData '{this.name}': Note timestamp {note.timestamp} had pitchLevel adjusted to {note.pitchLevel} to fit within {numberOfVerticalLanes} lanes.");
            }
            if (note.pitchLevel < 0) note.pitchLevel = 0;
        }
    }
} 