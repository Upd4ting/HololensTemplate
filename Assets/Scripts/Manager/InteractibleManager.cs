using UnityEngine;

/// <summary>
/// InteractibleManager keeps tracks of which GameObject
/// is currently in focus.
/// </summary>
public class InteractibleManager : Singleton<InteractibleManager>
{
    public GameObject FocusedGameObject { get; private set; }

    private GameObject oldFocusedGameObject = null;

    void Start()
    {
        FocusedGameObject = null;
    }

    void Update()
    {
        oldFocusedGameObject = FocusedGameObject;

        if (GazeManager.Instance.Hit)
        {
            FocusedGameObject = GazeManager.Instance.HitObject;
        }
        else
        {
            FocusedGameObject = null;
        }

        if (FocusedGameObject != oldFocusedGameObject)
        {
            ResetFocusedInteractible();

            if (FocusedGameObject != null)
            {
                if (FocusedGameObject.GetComponent<Interactible>() != null)
                {
                    FocusedGameObject.GetComponent<Interactible>().OnGazeEntered();
                }
            }
        }
    }

    private void ResetFocusedInteractible()
    {
        if (oldFocusedGameObject != null)
        {
            if (oldFocusedGameObject.GetComponent<Interactible>() != null)
            {
                FocusedGameObject.GetComponent<Interactible>().OnGazeExited();
            }
        }
    }
}