using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightMarkerScript : MonoBehaviour
{
    public GameObject highlightCirclePrefab;

    GameObject childHighlightCircle;

    GameObject origin;

    GameObject pieceOrigin;

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
            
        }
        else if (boardScr != null)
        {
            boardScr.RemoveHighlight(gameObject);

            Destroy(gameObject);
        }
    }

    public void TurnOnHighlight(bool recur)
    {
        if (boardScr != null && boardScr.GetCurrentBlock() != null)
        {
            foreach (GameObject h in boardScr.GetHighlightBoxList())
            {
                if ((h.transform.position - transform.position).magnitude < .5f)
                {
                    h.SendMessage("TurnOn", SendMessageOptions.DontRequireReceiver);
                    

                    if (pieceOrigin == boardScr.GetCurrentBlock() 
                        || pieceOrigin == boardScr.GetCurrentBlock().GetComponent<BlockScript>().GetSecondary())
                    {
                        h.SendMessage("TurnGreen", SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        h.SendMessage("TurnRed", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            bool doNext = recur;

            foreach (GameObject g in boardScr.GetBlockList())
            {
                if ((g.transform.position - transform.position).magnitude < .5f)
                {
                    if (g.GetComponent<BlockScript>().GetPiece() == "king" && doNext)
                    {
                        childHighlightCircle.SendMessage("TurnOnHighlight", false, SendMessageOptions.DontRequireReceiver);
                    }

                    doNext = false;
                }
            }

            if (doNext && childHighlightCircle != null)
            {
                childHighlightCircle.SendMessage("TurnOnHighlight", true, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void SetPieceOrigin(GameObject newPieceOrigin)
    {
        pieceOrigin = newPieceOrigin;
    }

    public void MakeNextCircle(int num, int newXDist, int newYDist, Transform newParent, GameObject newOrigin, BoardScript newBoardScr, GameObject newPieceOrigin)
    {
        transform.SetParent(newParent);

        origin = newOrigin;

        pieceOrigin = newPieceOrigin;

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
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(num - 1, xDist, yDist, newParent, gameObject, boardScr, newPieceOrigin);
        }
    }
}
