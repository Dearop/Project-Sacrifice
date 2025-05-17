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


} 