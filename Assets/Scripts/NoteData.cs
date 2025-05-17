using UnityEngine;
using TMPro;

public class NoteData : MonoBehaviour
{
    public bool isDoubleNote = false;
    public string letter1 = "";
    public string letter2 = ""; // Only used if isDoubleNote is true

    public TextMeshProUGUI textMesh1;
    public TextMeshProUGUI textMesh2; // Only used if isDoubleNote is true

    public bool firstLetterHit = false;
    public bool isValidated = false;

    public void Initialize(string l1, TextMeshProUGUI tm1, bool isDouble = false, string l2 = "", TextMeshProUGUI tm2 = null)
    {
        letter1 = l1;
        textMesh1 = tm1;
        isDoubleNote = isDouble;
        firstLetterHit = false;
        isValidated = false;

        if (textMesh1 != null) textMesh1.text = letter1;

        if (isDoubleNote)
        {
            letter2 = l2;
            textMesh2 = tm2;
            if (textMesh2 != null) textMesh2.text = letter2;
        }
        else
        {
            letter2 = "";
            textMesh2 = null;
        }
    }

    // Call this when the first letter of a double note is successfully hit
    public void HitFirstLetter()
    {
        if (!isDoubleNote || firstLetterHit || isValidated) return;

        firstLetterHit = true;
        if (textMesh1 != null) textMesh1.text = ""; // Clear the first letter or give feedback
        // Potentially change appearance, play sound etc.
    }

    // Call this when a single note or the second letter of a double note is hit
    public void ValidateNote()
    {
        if (isValidated) return;

        isValidated = true;
        // Clear remaining text or give feedback
        if (textMesh1 != null) textMesh1.text = ""; 
        if (textMesh2 != null) textMesh2.text = "";
        
        // Here, you might want to visually change the note (e.g., to a "hit" sprite)
        // or prepare it for removal by NoteScroller
        // For now, we'll just mark it. NoteScroller will handle destruction.
        Debug.Log("Note Validated: " + (isDoubleNote ? letter1 + "-" + letter2 : letter1) );
    }
} 