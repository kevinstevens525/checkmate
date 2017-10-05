using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightMarkerScript : MonoBehaviour
{
    public GameObject highlightCirclePrefab;

    GameObject childHighlightCircle;

    GameObject origin;

    int xDist;
    int yDist;
    float zDist;

    BoardScript boardScr;

	// Use this for initialization
	void Start ()
    {
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (origin != null 
            && ((origin.GetComponent<Collider>() != null && origin.GetComponent<Collider>().enabled)
            || (origin.GetComponent<HighlightMarkerScript>() != null)))
        {
            transform.localPosition = origin.transform.localPosition + new Vector3(xDist, yDist, zDist);
            
            /*
            foreach(GameObject h in boardScr.GetHighlightBoxList())
            {
                if ((h.transform.position - transform.position).magnitude < .5f)
                {
                    h.SendMessage("TurnOn", SendMessageOptions.DontRequireReceiver);
                }
            }
            */
        }
        else if (boardScr != null)
        {
            boardScr.RemoveHighlight(gameObject);

            Destroy(gameObject);
        }
    }

    public void TurnOnHighlight()
    {
        if (boardScr != null)
        {
            foreach (GameObject h in boardScr.GetHighlightBoxList())
            {
                if ((h.transform.position - transform.position).magnitude < .5f)
                {
                    h.SendMessage("TurnOn", SendMessageOptions.DontRequireReceiver);
                }
            }

            bool doNext = true;

            foreach (GameObject g in boardScr.GetBlockList())
            {
                if ((g.transform.position - transform.position).magnitude < .5f)
                {
                    doNext = false;
                }
            }

            if (doNext && childHighlightCircle != null)
            {
                childHighlightCircle.SendMessage("TurnOnHighlight", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void MakeNextCircle(int num, int newXDist, int newYDist, Transform newParent, GameObject newOrigin, BoardScript newBoardScr)
    {
        transform.SetParent(newParent);

        origin = newOrigin;

        name = "HighlightMarker" + num;

        boardScr = newBoardScr;

        boardScr.AddHighlight(gameObject);

        xDist = newXDist;
        yDist = newYDist;
        zDist = 0;

        childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);

        childHighlightCircle.transform.localPosition = transform.localPosition + new Vector3(xDist, yDist, zDist);

        if (num > 0)
        {
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(num - 1, xDist, yDist, newParent, gameObject, boardScr);
        }
    }
}
