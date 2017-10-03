using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public GameObject boardCube;

    List<GameObject> boardCubeList = new List<GameObject>();

    public GameObject block;

    GameObject currentBlock;
    List<GameObject> blockList = new List<GameObject>();

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

            Material newBlockMat = newBoardCube.GetComponent<Renderer>().material;
            if (yPos % 2 == xPos % 2)
            {
                newBlockMat.SetColor("_Color", Color.black);
            }
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (currentBlock != null && currentBlock.GetComponent<Rigidbody>().velocity.magnitude < .1f)
        {

            currentBlock = null;
        }

		if (currentBlock == null)
        {
            CreateBlock();
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
        newBlock.GetComponent<BlockScript>().SetAllBlocks(blockList);
        newBlock.GetComponent<BlockScript>().SetBoardScript(this);
        currentBlock = newBlock;
        blockList.Add(newBlock);
    }

    public void RemoveBlock(GameObject b)
    {
        blockList.Remove(b);
    }
}
