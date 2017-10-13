using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightBoxScript : MonoBehaviour
{
    Material mat;

    float transparencyMax = .4f;
    float transparencyCurrent = 0f;

    float greenMax = 1;
    float greenCurrent = 0;

    List<GameObject> piecesCovering = new List<GameObject>();

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

        if (greenCurrent > 0)
        {
            greenCurrent -= .2f;
        }

        Color newColor = mat.GetColor("_Color");
        newColor.a = transparencyCurrent;
        newColor.g = greenCurrent;
        newColor.r = 1 - greenCurrent;
        mat.SetColor("_Color", newColor);
    }

    public void TurnOn()
    {
        transparencyCurrent = transparencyMax;
    }

    public void TurnGreen()
    {
        greenCurrent = greenMax;
    }
}
