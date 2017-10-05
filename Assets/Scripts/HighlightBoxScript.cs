using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightBoxScript : MonoBehaviour
{
    Material mat;

    float transparencyMax = .3f;
    float transparencyCurrent = 0f;

	// Use this for initialization
	void Start ()
    {
        mat = GetComponent<Renderer>().material;

        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (transparencyCurrent > 0)
        {
            transparencyCurrent -= .1f;
        }

        Color newColor = mat.GetColor("_Color");
        newColor.a = transparencyCurrent;
        mat.SetColor("_Color", newColor);

    }

    public void TurnOn()
    {
        transparencyCurrent = transparencyMax;
    }
}
