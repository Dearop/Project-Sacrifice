# Interaction System for Unity

This system allows you to make objects interactable in your game and display dialogue when the player interacts with them.

## Setup Instructions

### 1. Setting up the UI
1. Create a Canvas in your scene (GameObject > UI > Canvas)
2. Create an Interaction Prompt UI:
   - Add a Panel to the Canvas and name it "InteractionPromptPanel"
   - Add a Text - TextMeshPro component inside the panel
   - Style it as desired

3. Create a Dialogue UI:
   - Add another Panel to the Canvas and name it "DialoguePanel"
   - Add a Text - TextMeshPro component for the character name
   - Add a Text - TextMeshPro component for the dialogue text
   - Add a Button component for continuing dialogue

### 2. Setting up the Managers
1. Create an empty GameObject named "InteractionManager"
2. Add the InteractionManager script to it
3. Assign the prompt panel and prompt text in the Inspector
4. **Important:** Assign your player GameObject to the "Player Transform" field
   - For third-person games, this should be your actual player character, not the camera
5. **New:** Assign your DialogueSystem to the "Dialogue System" field
   - This allows the interaction prompt to hide automatically during dialogues

6. Create another empty GameObject named "DialogueSystem"  
7. Add the DialogueSystem script to it
8. Assign the dialogue panel, name text, dialogue text, and continue button in the Inspector

### 3. Making Objects Interactable
1. Select the GameObject you want to make interactable
2. Add the "Interactable" component
3. Set the interaction radius
4. Customize the prompt text (e.g., "Press E to talk")

### 4. Adding Dialogue to Interactable Objects
1. Select your interactable GameObject
2. Add the "DialogueInteractable" component
3. Assign the DialogueSystem reference
4. Add dialogue lines in the Inspector:
   - Set the speaker name
   - Write dialogue text

## How It Works

When the player gets close to an interactable object, the interaction prompt appears. When the player presses the interaction key (default: E), the interaction event is triggered, which can start a dialogue or perform other actions.

**Important:** The interaction prompt will automatically hide when a dialogue is active, and reappear when the dialogue ends.

### Dialogue Controls
- Players can advance dialogue by clicking the continue button OR by pressing the E key
- The key used for advancing dialogue automatically matches the interaction key
- Pressing E while text is still typing will complete the current text immediately

## Extending the System

You can create custom interactable types by:
1. Creating a new script that inherits from MonoBehaviour
2. Getting a reference to the Interactable component
3. Subscribing to the onInteract event
4. Implementing your custom behavior

Example:
```csharp
public class CustomInteractable : MonoBehaviour
{
    private Interactable interactable;
    
    private void Start()
    {
        interactable = GetComponent<Interactable>();
        interactable.onInteract.AddListener(MyCustomAction);
    }
    
    private void MyCustomAction()
    {
        // Your custom interaction code here
    }
}
``` 