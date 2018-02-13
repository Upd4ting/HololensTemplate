using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// GazeManager determines the location of the user's gaze, hit position and normals.
/// </summary>
public class GazeManager : Singleton<GazeManager>
{
    [Tooltip("Maximum gaze distance for calculating a hit.")]
    public float MaxGazeDistance = 5.0f;

    [Tooltip("Select the layers raycast should target.")]
    public LayerMask RaycastLayerMask = Physics.DefaultRaycastLayers;

    [Tooltip("Should take count of UI or not?")]
    public bool ui = false;

    /// <summary>
    /// Physics.Raycast result is true if it hits a Hologram.
    /// </summary>
    public bool Hit { get; private set; }

    /// <summary>
    /// GameObject that has been hit.
    /// </summary>
    public GameObject HitObject { get; private set; }

    /// <summary>
    /// Position of the user's gaze.
    /// </summary>
    public Vector3 Position { get; private set; }

    /// <summary>
    /// RaycastHit Normal direction.
    /// </summary>
    public Vector3 Normal { get; private set; }

    private GazeStabilizer gazeStabilizer;
    private Vector3 gazeOrigin;
    private Vector3 gazeDirection;

    void Awake()
    {
        gazeStabilizer = GetComponent<GazeStabilizer>();
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        gazeOrigin = ray.origin;
        gazeDirection = ray.direction;

        gazeStabilizer.UpdateHeadStability(gazeOrigin, Camera.main.transform.rotation);
        gazeOrigin = gazeStabilizer.StableHeadPosition;

        UpdateRaycast();
    }

    /// <summary>
    /// Calculates the Raycast hit position and normal.
    /// </summary>
    private void UpdateRaycast()
    {
        if (ui)
        {
            PointerEventData ped = new PointerEventData(null);
            ped.position = new Vector2(Screen.width / 2, Screen.height / 2);

            List<RaycastResult> list = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, list);

            if (list.Count > 0)
            {
                RaycastResult result = list[0];
                Position = result.worldPosition;
                Normal = result.worldNormal;
                HitObject = result.gameObject;
                Debug.Log("Hit gameobject " + gameObject.name);
                return;
            }
        }

        RaycastHit hitInfo;

        Hit = Physics.Raycast(gazeOrigin,
                        gazeDirection,
                        out hitInfo,
                        MaxGazeDistance,
                        RaycastLayerMask);

        if (Hit)
        {
            Position = hitInfo.point;
            Normal = hitInfo.normal;
        }
        else
        {
            Position = gazeOrigin + (gazeDirection * MaxGazeDistance);
            Normal = gazeDirection;
        }
    }
}