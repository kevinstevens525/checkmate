﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public GameObject boardCube;

    List<GameObject> boardCubeList = new List<GameObject>();

    public GameObject block;

    public GameObject highlightBox;

    GameObject currentBlock;
    List<GameObject> blockList = new List<GameObject>();

    List<GameObject> highlightList = new List<GameObject>();

    List<GameObject> highlightBoxList = new List<GameObject>();

    Vector3 blockStartPos = new Vector3(3, 16, 0);

	// Use this for initialization
	void Start ()
    {
		for (int i = 0; i < 128; i++)
        {
            int xPos = i % 8;
            int yPos = (i - xPos) / 8;
            Vector3 newPos = new Vector3(xPos, yPos, 1);

            GameObject newBoardCube = Instantiate(boardCube, transform.position + newPos, Quaternion.identity);
            newBoardCube.transform.SetParent(transform);
            boardCubeList.Add(newBoardCube);

            GameObject newHighlightCube = Instantiate(highlightBox, transform.position + newPos - Vector3.forward, Quaternion.identity);
            newHighlightCube.transform.SetParent(transform);
            newHighlightCube.name = "HighlightCube" + i;
            highlightBoxList.Add(newHighlightCube);

            Material newBlockMat = newBoardCube.GetComponent<Renderer>().material;
            if (yPos % 2 == xPos % 2)
            {
                newBlockMat.SetColor("_Color", new Color(.5f, .5f, .5f));
            }
        }

        ResetBoard();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (currentBlock != null &&
            //(currentBlock.GetComponent<Rigidbody>().velocity.magnitude < .1f || !currentBlock.GetComponent<Collider>().enabled))
            (currentBlock.GetComponent<BlockScript>().locked || !currentBlock.GetComponent<Collider>().enabled))
        {

            currentBlock = null;
        }

		if (currentBlock == null)
        {
            CreateBlock();
        }

        for (int i = 0; i < highlightBoxList.Count; i++)
        {
            HighlightBoxScript hBoxScript = highlightBoxList[i].GetComponent<HighlightBoxScript>();

            if (highlightBoxList[i].GetComponent<Renderer>().material.GetColor("_Color").r > .5f)
            {
                

                hBoxScript.EnableAllBorders();

                if (i % 8 != 0)
                {
                    if (i - 1 >= 0 && highlightBoxList[i - 1].GetComponent<Renderer>().material.GetColor("_Color").r > .5f)
                    {
                        hBoxScript.DisableBorder(0);
                    }
                }

                if (i % 8 != 7)
                {
                    if (i + 1 <= highlightBoxList.Count - 1 && highlightBoxList[i + 1].GetComponent<Renderer>().material.GetColor("_Color").r > .5f)
                    {
                        hBoxScript.DisableBorder(1);
                    }
                }

                if (i - 8 >= 0 && highlightBoxList[i - 8].GetComponent<Renderer>().material.GetColor("_Color").r > .5f)
                {
                    hBoxScript.DisableBorder(3);
                }

                if (i + 8 <= highlightBoxList.Count - 1 && highlightBoxList[i + 8].GetComponent<Renderer>().material.GetColor("_Color").r > .5f)
                {
                    hBoxScript.DisableBorder(2);
                }
            }
            else
            {
                hBoxScript.DisableAllBorders();
            }
        }
        
	}

    public GameObject GetCurrentBlock()
    {
        return (currentBlock);
    }
    
    private void CreateBlock()
    {
        GameObject newBlock = Instantiate(block, blockStartPos, Quaternion.identity);
        newBlock.transform.SetParent(transform);
        newBlock.transform.localPosition = newBlock.transform.position;
        newBlock.GetComponent<BlockScript>().SetAllBlocks(blockList);
        // newBlock.GetComponent<BlockScript>().SetHighlightBoxList(highlightBoxList);
        newBlock.GetComponent<BlockScript>().SetBoardScript(this);
        currentBlock = newBlock;
        blockList.Add(newBlock);
        newBlock.GetComponent<BlockScript>().SpawnSecondary(block);

        foreach (GameObject b in blockList)
        {
            b.GetComponent<BlockScript>().GetAdjacent(true);
        }
    }

    private void CreateEnemyBlock(float newXPos, float newYPos, string newPiece)
    {
        GameObject newBlock = Instantiate(block, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
        newBlock.transform.SetParent(transform);
        newBlock.transform.localPosition = newBlock.transform.position;
        newBlock.GetComponent<BlockScript>().SetAllBlocks(blockList);
        // newBlock.GetComponent<BlockScript>().SetHighlightBoxList(highlightBoxList);
        newBlock.GetComponent<BlockScript>().SetBoardScript(this);
        blockList.Add(newBlock);
        newBlock.GetComponent<BlockScript>().SetPiece(newPiece, false);

        foreach (GameObject b in blockList)
        {
            b.GetComponent<BlockScript>().GetAdjacent(true);
        }
    }

    private void ResetBoard()
    {
        int level = 4;

        for (int i = 0; i < level; i++)
        {
            int newKingX = Random.Range(0, 7);
            int newKingY = Random.Range(0, 9);

            bool goodToGo = false;

            while (!goodToGo)
            {
                goodToGo = true;

                newKingX = Random.Range(0, 7);
                newKingY = Random.Range(0, 9);

                foreach (GameObject g in blockList)
                {
                    if (Mathf.Abs(g.transform.localPosition.x - newKingX) < .2f
                        && Mathf.Abs(g.transform.localPosition.y - newKingY) < .2f)
                    {
                        goodToGo = false;
                    }
                }
            }

            CreateEnemyBlock(newKingX, newKingY, "king");
        }
    }

    public void RemoveBlock(GameObject b)
    {
        blockList.Remove(b);
    }

    public void AddHighlight(GameObject h)
    {
        if (!highlightList.Contains(h))
        {
            highlightList.Add(h);
        }
    }

    public void RemoveHighlight(GameObject h)
    {
        if (highlightList.Contains(h))
        {
            highlightList.Remove(h);
        }
    }

    public List<GameObject> GetHighlightList()
    {
        return highlightList;
    }

    public List<GameObject> GetHighlightBoxList()
    {
        return highlightBoxList;
    }

    public List<GameObject> GetBlockList()
    {
        return blockList;
    }
}
