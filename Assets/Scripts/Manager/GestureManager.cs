using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

/// <summary>
///     GestureManager contains event handlers for subscribed gestures.
/// </summary>
public class GestureManager : Singleton<GestureManager>
{
    public GestureRecognizer GestureRecognizer { get; private set; }

    private void Awake()
    {
        GestureRecognizer = new GestureRecognizer();
        // Ecoute de tous les events
        //GestureRecognizer.SetRecognizableGestures(GestureSettings.);

        GestureRecognizer.Tapped += args =>
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;
            if (focusedObject != null && focusedObject.GetComponent<Interactible>() != null)
                focusedObject.GetComponent<Interactible>().OnSelect();
        };

        GestureRecognizer.HoldStarted += args =>
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;
            if (focusedObject != null && focusedObject.GetComponent<Draggable>() != null)
                focusedObject.GetComponent<Draggable>().OnHoloDragEnter();
        };

        GestureRecognizer.HoldCompleted += args =>
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;
            if (focusedObject != null && focusedObject.GetComponent<Draggable>() != null)
                focusedObject.GetComponent<Draggable>().OnHoloDragComplete();
        };

        GestureRecognizer.HoldCanceled += args =>
        {
            GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;
            if (focusedObject != null && focusedObject.GetComponent<Draggable>() != null)
                focusedObject.GetComponent<Draggable>().OnHoloDragCancel();
        };

        GestureRecognizer.StartCapturingGestures();
    }

    private void OnDestroy()
    {
        GestureRecognizer.StopCapturingGestures();
    }
}