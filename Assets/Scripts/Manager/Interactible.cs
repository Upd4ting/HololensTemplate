﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactible : MonoBehaviour
{
    public abstract void OnSelect();

    public void OnGazeEntered() { }

    public void OnGazeExited() { }
}
