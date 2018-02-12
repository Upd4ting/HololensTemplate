using UnityEngine;
using UnityEngine.XR.WSA.Input;

/// <summary>
/// GestureManager contains event handlers for subscribed gestures.
/// </summary>
public class GestureManager : Singleton<GestureManager>
{
    private GestureRecognizer gestureRecognizer;

    public GestureRecognizer GestureRecognizer
    {
        get
        {
            return gestureRecognizer;
        }

        set
        {
            gestureRecognizer = value;
        }
    }

    void Start()
    {
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);

        gestureRecognizer.Tapped += (args) =>
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;

            if (focusedObject != null)
            {
                focusedObject.SendMessage("OnSelect");
            }
        };

        gestureRecognizer.StartCapturingGestures();
    }

    void OnDestroy()
    {
        gestureRecognizer.StopCapturingGestures();
    }
}
