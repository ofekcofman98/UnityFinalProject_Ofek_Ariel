using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetTargetDisplay : MonoBehaviour 
{
	public int displayID;

	void Awake() 
    {
		GetComponent<Camera>().targetDisplay = displayID;
	}
}
