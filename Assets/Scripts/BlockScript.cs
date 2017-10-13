using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    float speed = 2f;

    Rigidbody rb;
    Renderer rend;
    SpriteRenderer pieceRend;

    string piece;

    List<int> pieceProbabilities = new List<int>();
    List<string> pieceNames = new List<string>();

    bool playerColor = true;

    List<GameObject> adjacentMatching = new List<GameObject>(); // Adjacent blocks of the same piece
    List<GameObject> adjacentBlocks = new List<GameObject>(); // Only blocks directly up, down, left, right
    List<GameObject> adjacentDiagonals = new List<GameObject>(); // All adjacent blocks AND all diagonals

    List<GameObject> allBlocks = new List<GameObject>(); // All blocks on the board

    List<GameObject> highlightBoxList = new List<GameObject>();
    List<GameObject> adjacentHighlights = new List<GameObject>();

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

        if (playerColor)
        {
            rb.velocity = -transform.up * speed;
        }
        else
        {
            StopFalling();
        }

        rend = GetComponent<Renderer>();

        pieceRend = GetComponentInChildren<SpriteRenderer>();

        if (piece == null)
        {
            pieceProbabilities.Add(10);
            pieceProbabilities.Add(9);
            pieceProbabilities.Add(7);
            pieceProbabilities.Add(5);
            pieceProbabilities.Add(4);
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
        }
        

        
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetAdjacent(false);

        if (!setToDestroy)
        {
            if (piece == "king")
            {
                CheckCheckmate();
            }

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

            if (playerColor && childHighlightCirclesList.Count == 0 && parentBS != null)
            {
                MakeHighlightCircles();
            }

            foreach (GameObject g in childHighlightCirclesList)
            {
                g.SendMessage("TurnOnHighlight", true, SendMessageOptions.DontRequireReceiver);
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

    private void CheckCheckmate()
    {
        adjacentHighlights.Clear();

        foreach(GameObject h in parentBS.GetHighlightBoxList())
        {
            if ((h.transform.localPosition - transform.localPosition).magnitude < 1.5f
                && h.GetComponent<Renderer>().material.GetColor("_Color").a > 0.01f
                && h.GetComponent<Renderer>().material.GetColor("_Color").g < 0.4f)
            {
                adjacentHighlights.Add(h);
            }
        }

        if (adjacentHighlights.Count >= 9)
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

    public void SetPiece(string newPiece, bool newIsPlayerColor)
    {
        playerColor = newIsPlayerColor;
        piece = newPiece;

        SetSpriteAndColor();
    }

    private void SetSpriteAndColor()
    {
        float highColor = .6f;
        float lowColor = .2f;

        if (pieceRend == null)
        {
            pieceRend = GetComponentInChildren<SpriteRenderer>();
        }

        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }

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

        if (playerColor)
        {
            pieceRend.color = Color.white;
        }
        else
        {
            pieceRend.color = Color.black;
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
        

        List<Vector3> piecePositions = new List<Vector3>();

        if (piece == "pawn")
        {
            piecePositions.Add(new Vector3(0, 1, 1));
            piecePositions.Add(new Vector3(0, -1, 1));
            
        }
        else if (piece == "bishop")
        {
            piecePositions.Add(new Vector3(7, 1, 1));
            piecePositions.Add(new Vector3(7, -1, 1));
            piecePositions.Add(new Vector3(7, -1, -1));
            piecePositions.Add(new Vector3(7, 1, -1));
            
        }
        else if (piece == "rook")
        {
            piecePositions.Add(new Vector3(7, 1, 0));
            piecePositions.Add(new Vector3(7, -1, 0));
            piecePositions.Add(new Vector3(15, 0, 1));
            piecePositions.Add(new Vector3(15, 0, -1));
            
        }
        else if (piece == "knight")
        {
            piecePositions.Add(new Vector3(0, 1, 2));
            piecePositions.Add(new Vector3(0, 2, 1));
            piecePositions.Add(new Vector3(0, 2, -1));
            piecePositions.Add(new Vector3(0, 1, -2));
            piecePositions.Add(new Vector3(0, -1, 2));
            piecePositions.Add(new Vector3(0, -2, 1));
            piecePositions.Add(new Vector3(0, -2, -1));
            piecePositions.Add(new Vector3(0, -1, -2));
            
        }
        else if (piece == "queen")
        {
            piecePositions.Add(new Vector3(7, 1, 1));
            piecePositions.Add(new Vector3(7, -1, 1));
            piecePositions.Add(new Vector3(7, -1, -1));
            piecePositions.Add(new Vector3(7, 1, -1));
            piecePositions.Add(new Vector3(7, 1, 0));
            piecePositions.Add(new Vector3(7, -1, 0));
            piecePositions.Add(new Vector3(15, 0, 1));
            piecePositions.Add(new Vector3(15, 0, -1));
            
        }

        foreach(Vector3 v in piecePositions)
        {
            childHighlightCircle = Instantiate(highlightCirclePrefab, transform.position, Quaternion.identity);
            childHighlightCircle.GetComponent<HighlightMarkerScript>().MakeNextCircle((int)v.x, (int)v.y, (int)v.z, transform.parent, gameObject, parentBS, gameObject);
            childHighlightCirclesList.Add(childHighlightCircle);
        }
    }
}
