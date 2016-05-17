﻿using UnityEngine;
using System.Collections;

public class LevelGeneratorScript : MonoBehaviour
{
    int gridSize { get { return SpawnScript.Instance.gridSize; } }
    int blockSize { get { return SpawnScript.Instance.blockSize; } }
    GameObject[,] gridGO { get { return GridScript.Instance.gridGO; } set { GridScript.Instance.gridGO = value; } }
    public int gridFree;

    void Start()
    {
        gridFree = gridSize * gridSize;
    }

    public void Click1()
    {
        for (int i = 0; i <= gridSize - blockSize; i++)
            for (int j = 0; j <= gridSize - blockSize; j++)
            {
                Debug.Log(GridLocID(new Vector2(i, j)));
                CompleteGrid(i, j);
            }
                

        FillEmptyGrid();
    }

    public void Click2()
    {
        for (int n = 0; n < 100; n++)
        {
            Vector2 ponto = new Vector2(Random.Range(0, (gridSize - blockSize) + 1), Random.Range(0, (gridSize - blockSize) + 1));

            int squaresNumber = Random.Range(4, 6);
            GameObject block = SpawnScript.Instance.blocksList[squaresNumber][Random.Range(0, SpawnScript.Instance.blocksList[squaresNumber].Count)] as GameObject;
            GameObject randomBlock = Instantiate(block, SpawnScript.Instance.spawnLocations[SpawnScript.Instance.activeBlocksNumber].transform.position, Quaternion.identity) as GameObject;

            for (int rot = 0; rot < Random.Range(0, 5); rot++)
                randomBlock.GetComponent<BlockScript>().RotateMatrix(true);

            if (CheckPosition(randomBlock, ponto))
            {
                MoveBlockGrid(randomBlock, ponto);
                Debug.Log(randomBlock + "posicionado em " + ponto.x + " " + ponto.y);
                gridFree -= randomBlock.GetComponent<BlockScript>().sList.Count;
            }
            else
            {
                SpawnScript.Instance.activeBlocksNumber--;
                Destroy(randomBlock);
            }

        }
    }

    //Cria string de um local 3x3 do grid. 1 = livre, 0 = ocupado
    string GridLocID(Vector2 position)
    {
        string blockID;
        int bitPos = 0;
        int bID = 0;

        for (int y = (int)position.y; y < (int)position.y + blockSize; y++)
        {
            for (int x = (int)position.x + (blockSize - 1); x >= (int)position.x; x--)
            {
                if (gridGO[x, y].GetComponent<SquareScript>().sType == SquareType.GridEmpty)
                {
                    bID += (int)Mathf.Pow(2, bitPos);
                }
                bitPos++;
            }
        }
        blockID = System.Convert.ToString(bID, 2);
        char[] removeChars = { '0' };
        return blockID.TrimEnd(removeChars);
    }

    //Checa todos os espaços 3x3 do grid e tenta preencher com um bloco
    void CompleteGrid(int i, int j)
    {
        bool teste = false;
        string gridSpace = GridLocID(new Vector2(i, j));
        int nBlocks = gridSpace.Split('1').Length - 1;

        if (nBlocks >= 2 && nBlocks <= 5)
        {
            foreach (GameObject block in SpawnScript.Instance.blocksList[nBlocks])
            {
                GameObject blockGO = Instantiate(block, SpawnScript.Instance.spawnLocations[SpawnScript.Instance.activeBlocksNumber].transform.position, Quaternion.identity) as GameObject;
                for (int r = 0; r < 4; r++)
                {
                    if (gridSpace == blockGO.GetComponent<BlockScript>().rotationIDs[r])
                    {
                        for (int rotations = r; rotations > 0; rotations--)
                            blockGO.GetComponent<BlockScript>().RotateMatrix(true);


                        for (int x = -2; x < blockSize; x++)
                        {
                            for (int y = -2; y < blockSize; y++)
                            {
                                if ((i + x >= 0) && (i + x < gridSize) && (j + y >= 0) && (j + y < gridSize) && !teste)
                                {
                                    if (CheckPosition(blockGO, new Vector2(i + x, j + y)))
                                    {
                                        MoveBlockGrid(blockGO, new Vector2(i + x, j + y));
                                        Debug.Log(blockGO + "completado em " + (i + x).ToString() + " " + (j + y).ToString());
                                        teste = true;
                                        gridFree -= blockGO.GetComponent<BlockScript>().sList.Count;
                                    }
                                }
                            }
                        }

                    }
                }
                if (!teste)
                {
                    SpawnScript.Instance.activeBlocksNumber--;
                    Destroy(blockGO);
                }
                teste = false;
            }
        }
    }

    void FillEmptyGrid()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (GridScript.Instance.gridGO[i, j].GetComponent<SquareScript>().sType == SquareType.GridEmpty)
                    GridScript.Instance.FillGrid(new Vector2(i, j));
            }
        }
    }

    bool CheckPosition(GameObject blockGO, Vector2 destiny)
    {

        foreach (SquareScript square in blockGO.GetComponent<BlockScript>().sList)
        {
            //Bloco esta dentro do grid?
            Vector2 squarePos = destiny + square.relativePos;
            if (squarePos.x < 0 || squarePos.x >= SpawnScript.Instance.gridSize || squarePos.y < 0 || squarePos.y >= SpawnScript.Instance.gridSize)
            {
                //Debug.Log("Bloco fora do grid!");
                return false;
            }

            //Existem outros blocos la?
            if (GridScript.Instance.gridGO[(int)squarePos.x, (int)squarePos.y].GetComponent<SquareScript>().sType != SquareType.GridEmpty)
            {
                //Debug.Log("Não pode ser colocado " + blockGO + " em " + destiny.x + "," + destiny.y + " pois ja existe um bloco la");
                blockGO.GetComponent<BlockScript>().bPlaced = false;
                return false;
            }
        }
        return true;
    }

    void MoveBlockGrid(GameObject blockGO, Vector2 destiny)
    {
        Color color = GridScript.Instance.blocksColor[SpawnScript.Instance.activeBlocksNumber];
        foreach (SquareScript square in blockGO.GetComponent<BlockScript>().sList)
        {
            Vector2 pos = destiny + square.relativePos;
            SquareScript gridSquare = GridScript.Instance.gridGO[(int)pos.x, (int)pos.y].GetComponent<SquareScript>();
            gridSquare.sType = SquareType.GridUsed;
            gridSquare.squareGridPos = pos;
            gridSquare.relativePos = square.relativePos;
            gridSquare.parentBlock = blockGO.GetComponent<BlockScript>();
            gridSquare.bNumber = gridSquare.parentBlock.bNumber;
            gridSquare.GetComponent<SpriteRenderer>().color = color;
            gridSquare.GetComponent<SpriteRenderer>().sortingLayerName = "block";
            blockGO.GetComponent<BlockScript>().bPlaced = true;
            blockGO.GetComponent<BlockScript>().bPos = destiny;

            square.transform.localPosition = new Vector3(square.relativePos.x, square.relativePos.y, -1);
        }
        blockGO.transform.position = SpawnScript.Instance.spawnLocations[blockGO.GetComponent<BlockScript>().bNumber - 1].transform.position - Vector3.forward;

    }
}