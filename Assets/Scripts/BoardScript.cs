using System.Collections;
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

    private List<string> pieceList = new List<string>();

    private int level = 1;

    private bool gameOver = false;

    public Texture lightSquare;
    public Texture darkSquare;

    public Material lightSquareMat;
    public Material darkSquareMat;

    private int leftButton = 0;
    private int rightButton = 0;
    private int downButton = 0;

    private GameObject nextPieceDisplay;
    private GameObject nextPieceDisplayChild;

    private List<string> newPiecePieces = new List<string>(2);

    private List<int> pieceProbabilities = new List<int>();
    private List<string> pieceNames = new List<string>();

    // Use this for initialization
    void Start ()
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

        newPiecePieces.Add("queen");
        newPiecePieces.Add("queen");

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
                // newBlockMat.SetColor("_Color", new Color(.5f, .5f, .5f));
                //newBlockMat.SetTexture("_MainTex", darkSquare);
                newBoardCube.GetComponent<Renderer>().material = darkSquareMat;
                newBoardCube.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(.25f, .25f));
                newBoardCube.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f)));
            }
            else
            {
                // newBlockMat.SetColor("_Color", new Color(0f, 0f, 0f));
                //newBlockMat.SetTexture("_MainTex", lightSquare);
                newBoardCube.GetComponent<Renderer>().material = lightSquareMat;
                newBoardCube.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(.25f, .25f));
                newBoardCube.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f)));
            }
        }

        pieceList.Add("pawn");
        pieceList.Add("knight");
        pieceList.Add("bishop");
        pieceList.Add("rook");
        pieceList.Add("queen");

        RandomizeNextPiece();

        CreateNextBlock();

        ResetBoard();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Controls();

        if (!gameOver)
        {

            if (currentBlock != null &&
            (currentBlock.GetComponent<BlockScript>().locked || !currentBlock.GetComponent<Collider>().enabled))
            {

                currentBlock = null;
            }

            foreach (GameObject g in blockList)
            {
                if (currentBlock != null
                    && g != currentBlock && g != currentBlock.GetComponent<BlockScript>().GetSecondary()
                    && g.transform.position.y > blockStartPos.y - 1.5f
                    && g.transform.position.x > blockStartPos.x - .5f
                    && g.transform.position.x < blockStartPos.x + 1.5f)
                {
                    gameOver = true;
                }
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

            bool stillEnemies = false;

            foreach (GameObject g in blockList)
            {
                if (g.GetComponent<BlockScript>().GetPiece() == "king")
                {
                    stillEnemies = true;
                }
            }

            if (!stillEnemies)
            {
                level++;

                ResetBoard();
            }
        }
        else
        {
            DestroyAll();

            for (int i = 0; i < highlightBoxList.Count; i++)
            {
                HighlightBoxScript hBoxScript = highlightBoxList[i].GetComponent<HighlightBoxScript>();

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

        newBlock.GetComponent<BlockScript>().SetPiece(newPiecePieces[0], true);
        newBlock.GetComponent<BlockScript>().GetSecondary().GetComponent<BlockScript>().SetPiece(newPiecePieces[1], true);


        foreach (GameObject b in blockList)
        {
            b.GetComponent<BlockScript>().GetAdjacent(true);
        }

        RandomizeNextPiece();

        nextPieceDisplay.GetComponent<BlockScript>().SetPiece(newPiecePieces[0], true);
        nextPieceDisplayChild.GetComponent<BlockScript>().SetPiece(newPiecePieces[1], true);
    }

    private void RandomizeNextPiece()
    { 
        for (int n = 0; n < newPiecePieces.Count; n++)
        {
            int pieceProbMax = 0;
            int pieceProbCurrent = 0;

            foreach (int i in pieceProbabilities)
            {
                pieceProbMax += i;
            }

            int pieceProbRandom = Random.Range(1, pieceProbMax + 1);

            for (int i = 0; i < pieceNames.Count; i++)
            {
                if (pieceProbRandom > pieceProbCurrent)
                {
                    newPiecePieces[n] = pieceNames[i];
                }

                pieceProbCurrent += pieceProbabilities[i];
            }
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
        newBlock.transform.localScale = Vector3.zero;

        foreach (GameObject b in blockList)
        {
            b.GetComponent<BlockScript>().GetAdjacent(true);
        }
    }

    private void CreateNextBlock()
    {
        GameObject newBlock = Instantiate(block, new Vector3(9, 14, 0), Quaternion.identity);
        newBlock.transform.SetParent(transform);
        newBlock.transform.localPosition = newBlock.transform.position;
        
        newBlock.GetComponent<BlockScript>().SetBoardScript(this);

        nextPieceDisplay = newBlock;

        GameObject newBlock2 = Instantiate(block, new Vector3(10, 14, 0), Quaternion.identity);
        newBlock2.transform.SetParent(transform);
        newBlock2.transform.localPosition = newBlock2.transform.position;

        newBlock2.GetComponent<BlockScript>().SetBoardScript(this);

        nextPieceDisplayChild = newBlock2;
    }

    private void DestroyAll()
    {
        foreach (GameObject g in blockList)
        {
            g.GetComponent<BlockScript>().DestroyObject();
        }
    }

    private void ResetBoard()
    {
        foreach(GameObject g in blockList)
        {
            g.GetComponent<BlockScript>().DestroyObject();
        }

        List<string> randomizeList = new List<string>();
        
        for (int i = 0; i < level; i++)
        {
            randomizeList.Add("king");
            randomizeList.Add(pieceList[Random.Range(0, pieceList.Count)]);
        }

        while (randomizeList.Count < 80)
        {
            randomizeList.Add("");
        }

        for (int i = 0; i < randomizeList.Count; i++)
        {
            int newIndex = Random.Range(0, randomizeList.Count - 1);

            string temp = randomizeList[newIndex];

            randomizeList[newIndex] = randomizeList[i];

            randomizeList[i] = temp;
        }

        for (int i = 0; i < randomizeList.Count; i++)
        {
            if (randomizeList[i].Length > 1)
            {
                CreateEnemyBlock(i % 8, Mathf.Floor(i / 8), randomizeList[i]);
            }
        }

        
    }

    private void Controls()
    {
        if (Input.GetAxisRaw("Horizontal") < -.2f)
        {
            if (leftButton == 0)
            {
                leftButton = 2;
            }
            else
            {
                leftButton = 1;
            }
        }
        else
        {
            leftButton = 0;
        }

        if (Input.GetAxisRaw("Horizontal") > .2f)
        {
            if (rightButton == 0)
            {
                rightButton = 2;
            }
            else
            {
                rightButton = 1;
            }
        }
        else
        {
            rightButton = 0;
        }

        if (Input.GetAxisRaw("Vertical") < -.4f)
        {
            if (downButton == 0)
            {
                downButton = 2;
            }
            else
            {
                downButton = 1;
            }
        }
        else
        {
            downButton = 0;
        }
    }

    public int GetLeftButton()
    {
        return leftButton;
    }

    public int GetRightButton()
    {
        return rightButton;
    }

    public int GetDownButton()
    {
        return downButton;
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
