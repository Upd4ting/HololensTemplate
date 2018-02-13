using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTarget : Interactible {

    public MenuSpawner ms { get; set; }

	public override void OnSelect()
    {
        ms.SendMessage("OnTargetClicked");
    }
}
