using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    float speed = 4f;

    Rigidbody rb;
    Renderer rend;
    SpriteRenderer pieceRend;

    string piece = "pawn";

    List<int> pieceProbabilities = new List<int>();
    List<string> pieceNames = new List<string>();

    bool playerColor = true;

    List<GameObject> adjacentMatching = new List<GameObject>(); // Adjacent blocks of the same piece
    List<GameObject> adjacentBlocks = new List<GameObject>(); // Only blocks directly up, down, left, right
    List<GameObject> adjacentDiagonals = new List<GameObject>(); // All adjacent blocks AND all diagonals

    List<GameObject> allBlocks = new List<GameObject>(); // All blocks on the board

    List<GameObject> highlightBoxList = new List<GameObject>();

    bool setToDestroy = false;

    BoardScript parentBS;

    public Sprite pawnSprite;
    public Sprite knightSprite;
    public Sprite bishopSprite;
    public Sprite rookSprite;
    public Sprite queenSprite;
    public Sprite kingSprite;

    bool isSecondary = false;

    GameObject secondaryBlock;
    GameObject primaryBlock;

    Vector3 secondaryPosition = new Vector3(1, 0, 0);

    public GameObject highlightCirclePrefab;
    GameObject childHighlightCircle;
    List<GameObject> childHighlightCirclesList = new List<GameObject>();

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = -transform.up * speed;

        rend = GetComponent<Renderer>();

        pieceRend = GetComponentInChildren<SpriteRenderer>();

        pieceProbabilities.Add(10);
        pieceProbabilities.Add(9);
        pieceProbabilities.Add(9);
        pieceProbabilities.Add(8);
        pieceProbabilities.Add(7);
        pieceProbabilities.Add(0);

        pieceNames.Add("pawn");
        pieceNames.Add("knight");
        pieceNames.Add("bishop");
        pieceNames.Add("rook");
        pieceNames.Add("queen");
        pieceNames.Add("king");

        int pieceProbMax = 0;
        int pieceProbCurrent = 0;

        foreach (int i in pieceProbabilities)
        {
            pieceProbMax += i;
        }

        int pieceProbRandom = Random.Range(1, pieceProbMax + 1);

        // pieceProbRandom = pieceProbMax - 1; // ------- FORCE PIECE -------

        for (int i = 0; i < pieceNames.Count; i++)
        {
            if (pieceProbRandom > pieceProbCurrent)
            {
                piece = pieceNames[i];
            }

            pieceProbCurrent += pieceProbabilities[i];
        }

        

        SetSpriteAndColor();

        // playerColor = false;

        if (playerColor)
        {
            pieceRend.color = Color.white;
        }
        else
        {
            pieceRend.color = Color.black;
        }

        
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetAdjacent(false);

        if (!setToDestroy)
        {

            if (parentBS.GetCurrentBlock() == this.gameObject || (primaryBlock != null && parentBS.GetCurrentBlock() == primaryBlock))
            {
                
                if (secondaryBlock != null)
                {
                    Controls();
                    secondaryBlock.transform.localPosition = transform.localPosition + secondaryPosition;
                }
            }
            else
            {
                Vector3 newPos = transform.localPosition;
                newPos.x = Mathf.Round(newPos.x);
                newPos.y = Mathf.Round(newPos.y);
                transform.localPosition = newPos;
            }

            if (childHighlightCirclesList.Count == 0 && parentBS != null)
            {
                MakeHighlightCircles();
            }

            foreach (GameObject g in childHighlightCirclesList)
            {
                g.SendMessage("TurnOnHighlight", SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (GetComponent<Collider>().enabled)
        {
            
            ActuallyDestroy();
        }

        if (transform.localPosition.y < -10)
        {
            Destroy(gameObject);
        }

        
    }

    void Controls()
    {
        bool canGoLeft = true;
        bool canGoRight = true;

        foreach (GameObject b in adjacentDiagonals)
        {
            if ((secondaryBlock != null && b != secondaryBlock))
            {
                if (b.transform.localPosition.x < transform.localPosition.x - .1f)
                {
                    canGoLeft = false;
                }

                if (b.transform.localPosition.x > transform.localPosition.x + .1f)
                {
                    canGoRight = false;
                }
            }
        }

        foreach (GameObject b in secondaryBlock.GetComponent<BlockScript>().GetAdjacentList())
        {
            if ((b != gameObject))
            {
                if (b.transform.localPosition.x < secondaryBlock.transform.localPosition.x - .1f)
                {
                    canGoLeft = false;
                }

                if (b.transform.localPosition.x > secondaryBlock.transform.localPosition.x + .1f)
                {
                    canGoRight = false;
                }
            }
        }

        if (transform.localPosition.x < .9f || secondaryBlock.transform.localPosition.x < .9f)
        {
            canGoLeft = false;
        }

        if (transform.localPosition.x > 6.1f || secondaryBlock.transform.localPosition.x > 6.1f)
        {
            canGoRight = false;
        }

        if (Input.GetButtonDown("Left") && canGoLeft)
        {
            transform.localPosition -= transform.right;
        }

        if (Input.GetButtonDown("Right") && canGoRight)
        {
            transform.localPosition += transform.right;
        }

        if (Input.GetButtonDown("RotateLeft"))
        {
            if (secondaryPosition.x == 1)
            {
                secondaryPosition = new Vector3(0, 1, 0);
            }
            else if (secondaryPosition.y == 1 && canGoLeft)
            {
                secondaryPosition = new Vector3(-1, 0, 0);
            }
            else if (secondaryPosition.x == -1)
            {
                secondaryPosition = new Vector3(0, -1, 0);
            }
            else if (secondaryPosition.y == -1 && canGoRight)
            {
                secondaryPosition = new Vector3(1, 0, 0);
            }
        }

        if (Input.GetButtonDown("RotateRight"))
        {
            if (secondaryPosition.x == -1)
            {
                secondaryPosition = new Vector3(0, 1, 0);
            }
            else if (secondaryPosition.y == -1 && canGoLeft)
            {
                secondaryPosition = new Vector3(-1, 0, 0);
            }
            else if (secondaryPosition.x == 1)
            {
                secondaryPosition = new Vector3(0, -1, 0);
            }
            else if (secondaryPosition.y == 1 && canGoRight)
            {
                secondaryPosition = new Vector3(1, 0, 0);
            }
        }
    }

    public void GetAdjacent(bool checkDestroy)
    {
        adjacentMatching.Clear();
        adjacentBlocks.Clear();
        adjacentDiagonals.Clear();

        foreach (GameObject b in allBlocks)
        {
            if (b != null)
            {
                if ((b.transform.localPosition - transform.localPosition).magnitude < 1.5f)
                {
                    adjacentDiagonals.Add(b);
                }

                if ((b.transform.localPosition - transform.localPosition).magnitude < 1.2f)
                {
                    adjacentBlocks.Add(b);
                }
            }
        }

        foreach (GameObject b in adjacentBlocks)
        {
            if (b != null)
            {
                if (b.GetComponent<BlockScript>().GetPiece() == piece
                    && b.GetComponent<Rigidbody>().velocity.magnitude < .1f)
                {
                    adjacentMatching.Add(b);
                }
            }
        }

        if (checkDestroy && adjacentMatching.Count >= 3)
        {
            DestroyObject();
        }
    }

    public List<GameObject> GetAdjacentList()
    {
        return adjacentDiagonals;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((secondaryBlock != null && collision.gameObject != secondaryBlock) || (primaryBlock != null && collision.gameObject != primaryBlock))
        {
            if (!setToDestroy && collision.gameObject.tag == gameObject.tag)
            {
                StopFalling();

                if (primaryBlock != null)
                {
                    primaryBlock.GetComponent<BlockScript>().StopFalling();
                }

                if (secondaryBlock != null)
                {
                    secondaryBlock.GetComponent<BlockScript>().StopFalling();
                }
            }
        }
    }

    public void StopFalling()
    {
        rb.velocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void SetAllBlocks(List<GameObject> newBlockList)
    {
        allBlocks = newBlockList;
    }

    public void SetHighlightBoxList(List<GameObject> newBoxList)
    {
        highlightBoxList = newBoxList;
    }

    public void SetBoardScript(BoardScript bs)
    {
        parentBS = bs;
    }

    public string GetPiece()
    {
        return piece;
    }

    public GameObject GetSecondary()
    {
        return secondaryBlock;
    }

    public void RemoveFromAdjacentMatching(GameObject g)
    {
        adjacentMatching.Remove(g);
    }

    public void DestroyObject()
    {
        setToDestroy = true;

        foreach (GameObject b in adjacentMatching)
        {
            BlockScript bScript = b.GetComponent<BlockScript>();
            if (!bScript.GetSetToDestroy())
            {
                bScript.DestroyObject();
            }
        }
    }

    public bool GetSetToDestroy()
    {
        return setToDestroy;
    }

    private void ActuallyDestroy()
    {
        // Destroy(gameObject);

        parentBS.RemoveBlock(gameObject);
        
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.velocity = (Vector3.up * 20) + (Vector3.right * Random.Range(-10f, 10f)) + (Vector3.forward * -150);

        rb.AddTorque(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));

        GetComponent<Collider>().enabled = false;
    }

    private void SetSpriteAndColor()
    {
        float highColor = .6f;
        float lowColor = .2f;

        if (piece == "pawn")
        {
            pieceRend.sprite = pawnSprite;

            rend.material.SetColor("_Color", new Color(highColor, lowColor, lowColor));
        }
        else if (piece == "knight")
        {
            pieceRend.sprite = knightSprite;

            rend.material.SetColor("_Color", new Color(highColor, highColor, lowColor));
        }
        else if (piece == "bishop")
        {
            pieceRend.sprite = bishopSprite;

            rend.material.SetColor("_Color", new Color(lowColor, highColor, lowColor));
        }
        else if (piece == "rook")
        {
            pieceRend.sprite = rookSprite;

            rend.material.SetColor("_Color", new Color(lowColor, highColor, highColor));
        }
        else if (piece == "queen")
        {
            pieceRend.sprite = queenSprite;

            rend.material.SetColor("_Color", new Color(lowColor, lowColor, highColor));
        }
        else if (piece == "king")
        {
            pieceRend.sprite = kingSprite;

            rend.material.SetColor("_Color", new Color(highColor, lowColor, highColor));
        }
    }

    public void SetSecondary()
    {
        isSecondary = true;
    }

    public void SetPrimaryBlock(GameObject newPrimaryBlock)
    {
        primaryBlock = newPrimaryBlock;
    }

    public void SpawnSecondary(GameObject block)
    {
        GameObject newBlock = Instantiate(block, transform.position + transform.right, Quaternion.identity);
        newBlock.transform.SetParent(transform.parent);
        newBlock.GetComponent<BlockScript>().SetAllBlocks(allBlocks);
        newBlock.GetComponent<BlockScript>().SetBoardScript(parentBS);
        secondaryBlock = newBlock;
        allBlocks.Add(newBlock);
        newBlock.GetComponent<BlockScript>().SetPrimaryBlock(gameObject);
    }

    private void MakeHighlightCircles()
    {
        /*
        childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
        childHighlightCircle.GetComponent<HighlightCircleScript>().MakeNextCircle(5, 1, 1, transform.parent, gameObject);
        */

        if (piece == "pawn")
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, 1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, -1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
        else if (piece == "bishop")
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
        else if (piece == "rook")
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, 0, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, 0, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(15, 0, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(15, 0, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
        else if (piece == "knight")
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, 1, 2, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, 2, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, 2, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, 1, -2, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, -1, 2, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, -2, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, -2, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(0, -1, -2, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
        else if (piece == "queen")
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, 0, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, 0, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(15, 0, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(15, 0, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, 1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, -1, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);

            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle(7, 1, -1, transform.parent, gameObject, parentBS);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
    }
}
