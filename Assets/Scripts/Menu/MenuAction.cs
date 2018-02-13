using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAction : Interactible
{
    public GameObject handler { get; set; }

    public override void OnSelect()
    {
        handler.SendMessage("OnUIClicked", this.gameObject.name);
    }
}
