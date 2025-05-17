# Audio NPC System for Unity

This system allows you to create NPCs that play music when interacted with. The player can interact with these NPCs to start and stop music playback.

## Setup Instructions

### 1. Adding Audio Files
1. Create a folder in your Assets directory for audio files (e.g., `Assets/Audio/Music`)
2. Import your audio files into this folder
3. Make sure your audio files are set to the appropriate import settings:
   - Select the audio file in the Project window
   - In the Inspector, set:
     - Force To Mono: Optional (depends on your needs)
     - Load Type: Streaming (for longer music files)
     - Compression Format: Vorbis/MP3 (for good quality/size balance)
     - Quality: Adjust as needed (higher quality = larger file size)

### 2. Setting up an Audio NPC
1. Create or select a GameObject to be your Audio NPC
2. Add the `Interactable` component to it
   - Set the interaction radius
   - Customize the prompt text (e.g., "Press E to talk to the musician")
3. Add either the `AudioInteractable` or `AudioInteractableAdvanced` component to the same GameObject
4. Configure the component in the Inspector:
   - Assign your DialogueSystem reference
   - Add intro dialogue lines (shown when first interacting)
   - Add outro dialogue lines (shown when stopping the music)
   - Assign the audio clip to play
   - Adjust volume and loop settings as needed
   - If using the advanced version, configure fade and spatial audio settings

## Basic vs Advanced Version

This system comes with two versions:

### AudioInteractable (Basic)
- Simple implementation with basic functionality
- Instantly starts/stops music
- Good for simple use cases or background music

### AudioInteractableAdvanced
- Includes fade-in/fade-out effects for smooth transitions
- Supports spatial audio for immersive 3D sound
- Additional configurable parameters
- Improved state management for transitions

## How It Works

The Audio NPC interaction follows these steps:

1. **Initial Interaction**: When the player first interacts with the NPC, it locks player movement and displays the intro dialogue.
2. **Start Music**: After the player advances through the intro dialogue, the music starts playing (with fade-in if using advanced version).
3. **Stop Music**: When the player interacts with the NPC again while music is playing, the music stops (with fade-out if using advanced version) and the outro dialogue is shown.
4. **Resume Control**: After advancing through the outro dialogue, player movement is unlocked and the cycle can repeat.

## State Flow

### Basic Version States:
1. **Initial**: Ready for first interaction
2. **IntroDialogue**: Showing introduction dialogue
3. **Playing**: Music is playing
4. **OutroDialogue**: Showing goodbye dialogue

### Advanced Version States:
1. **Initial**: Ready for first interaction
2. **IntroDialogue**: Showing introduction dialogue
3. **FadingIn**: Music is fading in
4. **Playing**: Music is playing at full volume
5. **FadingOut**: Music is fading out
6. **OutroDialogue**: Showing goodbye dialogue

## Example Usage

```csharp
// Example setup in code (normally done in Inspector)
AudioInteractableAdvanced pianistNPC = gameObject.AddComponent<AudioInteractableAdvanced>();
pianistNPC.musicClip = pianoMusicClip;
pianistNPC.volume = 0.7f;
pianistNPC.loop = true;
pianistNPC.fadeInTime = 3.0f;
pianistNPC.fadeOutTime = 2.0f;

// Configure spatial audio
pianistNPC.spatialAudio = true;
pianistNPC.spatialBlend = 1.0f;
pianistNPC.minDistance = 2.0f;
pianistNPC.maxDistance = 15.0f;

// Add some dialogue
pianistNPC.introDialogueLines = new DialogueLine[] {
    new DialogueLine { speakerName = "Pianist", dialogueText = "Would you like to hear my latest composition?" },
    new DialogueLine { speakerName = "Pianist", dialogueText = "I call it 'Moonlight Sonata'. I'll play it for you now." }
};

pianistNPC.outroDialogueLines = new DialogueLine[] {
    new DialogueLine { speakerName = "Pianist", dialogueText = "I hope you enjoyed the music." },
    new DialogueLine { speakerName = "Pianist", dialogueText = "Come back anytime you want to hear it again!" }
};
```

## Tips
- You can create multiple Audio NPCs with different music clips
- Adjust the volume to create a sense of distance
- Consider adding animations to your NPCs that sync with the music
- For background music that should be heard everywhere, disable spatial audio
- For music that should feel like it's coming from the NPC, enable spatial audio and adjust the distance settings
- Use longer fade times for smoother transitions with longer pieces of music 