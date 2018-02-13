# HololensTemplate

Hololens template including cursor system, menu system, interactibe system and more.

## Managers

### GazeManager

This manager handle raycast in the direction you're facing and store result of the hit. This manager can be accessed with
```code
GazeManager.Instance
```

### InteractibleManager

This manager use the GazeManager to retrieve the focused object and store it. He also send message to FocusedGameObject that have the component **Interactible**, these message are **GazeEntered**, **GazeExited**.
This manager can be accessed with
```code
InteractibleManager.Instance
```

### CursorManager

This manager handle the cursor mechanics for us and use some algorithm to stabilize it. You can configure **CursorOnHologram** and **CursorOffHologram**.

### GestureManager

This manager hold an object *GestureRecognizer* that we can access to subscribe to event such as *Tapped*, etc. This manager send the message **OnSelect** to a GameObject when the user tapped on it and only if this GameObject have the component **Interactible**.
This manager can be accessed with
```code
GestureManager.Instance
```

### KeywordManager

This manager allow us to track *word* that the user say and execute an *UnityEvent* when this word has been recognized.

## Menu

Documentation coming soon.
