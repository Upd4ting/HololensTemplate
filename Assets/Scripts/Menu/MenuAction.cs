using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAction : MonoBehaviour
{

    private GameObject handler;

    public GameObject Handler
    {
        get
        {
            return handler;
        }

        set
        {
            handler = value;
        }
    }

    public void Awake()
    {
        GameObject go = this.gameObject;

        if (go.GetComponent<Interactible>() == null)
            go.AddComponent<Interactible>();
    }

    public void OnSelect()
    {
        handler.SendMessage("OnUIClicked", this.gameObject.name);
    }
}
