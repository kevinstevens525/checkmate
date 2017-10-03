using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    float speed = 6f;

    Rigidbody rb;
    Renderer rend;
    SpriteRenderer pieceRend;

    string piece = "pawn";

    bool playerColor = true;

    List<GameObject> adjacentMatching = new List<GameObject>(); // Adjacent blocks of the same piece
    List<GameObject> adjacentBlocks = new List<GameObject>(); // Only blocks directly up, down, left, right
    List<GameObject> adjacentDiagonals = new List<GameObject>(); // All adjacent blocks AND all diagonals

    List<GameObject> allBlocks = new List<GameObject>(); // All blocks on the board

    bool setToDestroy = false;

    BoardScript parentBS;

    public Sprite pawnSprite;
    public Sprite knightSprite;
    public Sprite bishopSprite;
    public Sprite rookSprite;
    public Sprite queenSprite;
    public Sprite kingSprite;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = -transform.up * speed;

        rend = GetComponent<Renderer>();

        pieceRend = GetComponentInChildren<SpriteRenderer>();

        int pieceRandom = Random.Range(1, 6);

        // pieceRandom = 1;

        if (pieceRandom == 1)
        {
            piece = "pawn";
        }
        else if (pieceRandom == 2)
        {
            piece = "knight";
        }
        else if (pieceRandom == 3)
        {
            piece = "bishop";
        }
        else if (pieceRandom == 4)
        {
            piece = "rook";
        }
        else if (pieceRandom == 5)
        {
            piece = "queen";
        }

        SetSpriteAndColor();

        playerColor = false;

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
        if (!setToDestroy)
        {
            GetAdjacent();

            if (parentBS.GetCurrentBlock() == this.gameObject)
            {
                Controls();
            }
            else
            {
                Vector3 newPos = transform.localPosition;
                newPos.x = Mathf.Round(newPos.x);
                newPos.y = Mathf.Round(newPos.y);
                transform.localPosition = newPos;
            }
        }
        else if (GetComponent<Collider>().enabled)
        {
            ActuallyDestroy();
        }

        if (transform.position.y < -10)
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
            if (b.transform.position.x < transform.position.x - .1f)
            {
                canGoLeft = false;
            }

            if (b.transform.position.x > transform.position.x + .1f)
            {
                canGoRight = false;
            }
        }

        if (transform.localPosition.x < .9f)
        {
            canGoLeft = false;
        }

        if (transform.localPosition.x > 6.1f)
        {
            canGoRight = false;
        }

        if (Input.GetButtonDown("Left") && canGoLeft)
        {
            transform.position -= transform.right;
        }

        if (Input.GetButtonDown("Right") && canGoRight)
        {
            transform.position += transform.right;
        }
    }

    void GetAdjacent()
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
                    && b.GetComponent<Rigidbody>().velocity.magnitude < .05f)
                {
                    adjacentMatching.Add(b);
                }
            }
        }

        if (adjacentMatching.Count >= 3)
        {
            DestroyObject();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!setToDestroy && collision.gameObject.tag == gameObject.tag)
        {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void SetAllBlocks(List<GameObject> newBlockList)
    {
        allBlocks = newBlockList;
    }

    public void SetBoardScript(BoardScript bs)
    {
        parentBS = bs;
    }

    public string GetPiece()
    {
        return piece;
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
        rb.velocity = (Vector3.up * 20) + (Vector3.right * Random.Range(-10f, 10f)) + (Vector3.forward * -25);

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
}
