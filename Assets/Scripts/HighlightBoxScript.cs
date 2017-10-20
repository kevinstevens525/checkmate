using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightBoxScript : MonoBehaviour
{
    Material mat;

    float transparencyMax = .18f;
    float transparencyCurrent = 0f;

    float greenMax = .75f;
    float greenCurrent = 0;

    float redMax = .75f;
    float redCurrent = 0;

    List<GameObject> piecesCovering = new List<GameObject>();

    public GameObject borderLeft;
    public GameObject borderRight;
    public GameObject borderTop;
    public GameObject borderBottom;

    List<GameObject> borderList = new List<GameObject>();

	// Use this for initialization
	void Start ()
    {
        mat = GetComponent<Renderer>().material;

        borderList.Add(borderLeft);
        borderList.Add(borderRight);
        borderList.Add(borderTop);
        borderList.Add(borderBottom);
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

        if (redCurrent > 0)
        {
            redCurrent -= .2f;
        }

        Color newColor = mat.GetColor("_Color");
        newColor.a = transparencyCurrent;
        newColor.g = greenCurrent;
        newColor.r = redCurrent;
        newColor.b = 0;
        mat.SetColor("_Color", newColor);
        

        if (newColor.a < .01f)
        {
            GetComponent<Renderer>().enabled = false;
        }
        else
        {
            GetComponent<Renderer>().enabled = true;
        }


        foreach (GameObject g in borderList)
        {
            //g.GetComponent<Renderer>().material.color = newColor;
        }
    }

    public void TurnOn()
    {
        transparencyCurrent = transparencyMax;
    }

    public void TurnGreen()
    {
        greenCurrent = greenMax;
    }

    public void TurnRed()
    {
        redCurrent = redMax;
    }

    public float GetTransparencyMax()
    {
        return transparencyMax;
    }

    public float GetRedMax()
    {
        return redMax;
    }

    public void DisableAllBorders()
    {
        foreach (GameObject g in borderList)
        {
            g.GetComponent<Renderer>().enabled = false;
        }
    }

    public void EnableAllBorders()
    {
        foreach (GameObject g in borderList)
        {
            g.GetComponent<Renderer>().enabled = true;
        }
    }

    public void DisableBorder(int num)
    {
        borderList[num].GetComponent<Renderer>().enabled = false;
    }

    public void EnableBorder(int num)
    {
        borderList[num].GetComponent<Renderer>().enabled = true;
    }
}
