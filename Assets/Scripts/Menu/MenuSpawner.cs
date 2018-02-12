using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSpawner : MonoBehaviour
{

    [Tooltip("Menu canvas to spawn once the player is in range")]
    public GameObject menu;

    [Tooltip("On which GameObject the menu should spawn")]
    public GameObject target;

    [Tooltip("Handle that will received onUIClicked message")]
    public GameObject handler;

    [Tooltip("The angle where the UI will pop")]
    public int angle = 90;

    [Tooltip("Offset between Menu canvas and target")]
    public float offset = 0.2f;

    [Tooltip("Meter range that we need to be in to display the menu")]
    public int range = 10;

	void Awake ()
    {
        if (menu == null || target == null || handler == null)
            return;

        // Add MenuAction to each childs of Menu game object.

        Transform transform = menu.transform;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject go = menu.transform.GetChild(i).gameObject;
            MenuAction action = go.AddComponent<MenuAction>();
            action.Handler = handler;
        }
	}
	
	void Update ()
    {
        if (menu == null || target == null || handler == null)
        {
            if (menu != null)
                menu.SetActive(false);
            return;
        }

        Vector3 sub = target.transform.position - Camera.main.transform.position;

        float distance = sub.magnitude;

        if (distance > range)
        {
            menu.SetActive(false);
            return;
        }

        Quaternion quat = Quaternion.Euler(new Vector3(0, angle, 0));
        sub.Normalize();

        Vector3 menupos = target.transform.position + quat * sub * offset;

        menu.transform.position = menupos;
        menu.SetActive(true);

        if (menu.GetComponent<Billboard>() == null)
            menu.AddComponent<Billboard>();
    }
}
