using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSpawner : MonoBehaviour
{

    [Tooltip("Menu canvas to spawn once the player is in range")]
    public GameObject menu;

    [Tooltip("On which GameObject the menu should spawn")]
    public GameObject target;

    [Tooltip("The angle where the UI will pop")]
    public int angle = 90;

    [Tooltip("Offset between Menu canvas and target")]
    public float offset = 0.2f;

    [Tooltip("Meter range that we need to be in to display the menu")]
    public int range = 10;

    [Tooltip("Pop menu on click and not when in range")]
    public bool popMenuClick = false;

    private bool menuVisible = false;

	void Awake ()
    {
        if (menu == null || target == null)
            return;

        if (popMenuClick)
        {
            MenuTarget mt = target.AddComponent<MenuTarget>();
            mt.ms = this;
        }
	}
	
	void Update ()
    {
        if (menu == null || target == null)
        {
            if (menu != null)
                menu.SetActive(false);
            return;
        }

        Vector3 sub = target.transform.position - Camera.main.transform.position;

        float distance = sub.magnitude;

        if (!popMenuClick)
        {
            menuVisible = distance <= range;
        }

        if (menuVisible)
        {
            Quaternion quat = Quaternion.Euler(new Vector3(0, angle, 0));
            sub.Normalize();

            Vector3 menupos = target.transform.position + quat * sub * offset;

            menu.transform.position = menupos;

            if (menu.GetComponent<Billboard>() == null)
                menu.AddComponent<Billboard>();

            menu.SetActive(true);
        }
        else
            menu.SetActive(false);
    }

    public void OnTargetClicked()
    {
        menuVisible = !menuVisible;
    }


}
