using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    float speed = 10f;

    Rigidbody rb;
    Renderer rend;

    int piece = 0;

    List<GameObject> adjacentMatching = new List<GameObject>(); // Adjacent blocks of the same piece
    List<GameObject> adjacentBlocks = new List<GameObject>(); // Only blocks directly up, down, left, right
    List<GameObject> adjacentDiagonals = new List<GameObject>(); // All adjacent blocks AND all diagonals

    List<GameObject> allBlocks = new List<GameObject>(); // All blocks on the board

    bool setToDestroy = false;

    BoardScript parentBS;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = -transform.up * speed;

        rend = GetComponent<Renderer>();

        piece = Random.Range(1, 4);

        if (piece == 1)
        {
            rend.material.SetColor("_Color", Color.red);
        }
        else if (piece == 2)
        {
            rend.material.SetColor("_Color", Color.green);
        }
        else if (piece == 3)
        {
            rend.material.SetColor("_Color", Color.blue);
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

    public int GetPiece()
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
        rb.velocity = (Vector3.up * 20) + (Vector3.right * Random.Range(-10f, 10f)) + (Vector3.forward * -15);
        GetComponent<Collider>().enabled = false;
    }
}
