﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockScript : MonoBehaviour
{
    [SerializeField]
    int[] bLocations;
    int maxBlockSize;
    public string[] rotationIDs = new string[4];
    public int IDindex;

    public Color bColor;
    public int[,] bMatrix;
    public List<SquareScript> sList;
    public int bNumber;
    public bool bPlaced;
    public Vector2 bPos;
    

    void Awake()
    {
        maxBlockSize = SpawnScript.Instance.blockSize;
        sList = new List<SquareScript>();;
        bMatrix = new int[maxBlockSize, maxBlockSize];
        bNumber = SpawnScript.Instance.activeBlocksList.Count + 1;
        CreateBlock();                    
    }

    void CreateBlock()
    {
        foreach (int loc in bLocations)
        {
            switch (loc)
            {
                case 1: bMatrix[0, 0] = bNumber; break;
                case 2: bMatrix[1, 0] = bNumber; break;
                case 3: bMatrix[2, 0] = bNumber; break;
                case 4: bMatrix[0, 1] = bNumber; break;
                case 5: bMatrix[1, 1] = bNumber; break;
                case 6: bMatrix[2, 1] = bNumber; break;
                case 7: bMatrix[0, 2] = bNumber; break;
                case 8: bMatrix[1, 2] = bNumber; break;
                case 9: bMatrix[2, 2] = bNumber; break;
                default:
                    Debug.Log("Número inválido para criação de bloco" + loc);
                    break;
            }
        }
        SpawnScript.Instance.activeBlocksList.Add(this.gameObject);
        GetBlockIDs();
        CreateBlockSprite();
    }

    public void CreateBlockSprite()
    {
        for (int y = 0; y < SpawnScript.Instance.blockSize; y++)
        {
            for (int x = SpawnScript.Instance.blockSize - 1; x >= 0; x--)
            {
                if (bMatrix[x, y] == bNumber)
                {
                    GameObject baseSquare = Instantiate(Resources.Load("Prefabs/Base Square")) as GameObject;
                    baseSquare.transform.localPosition = new Vector3(x, y, 0);
                    baseSquare.GetComponent<SpriteRenderer>().color = bColor;
                    baseSquare.GetComponent<SpriteRenderer>().sortingLayerName = "blocks";
                    baseSquare.transform.parent = transform;

                    SquareScript sScript = baseSquare.GetComponent<SquareScript>();
                    sScript.relativePos = new Vector2(x, y);
                    sScript.sType = SquareType.Block;
                    sScript.parentBlock = this;
                    sScript.bNumber = bNumber;
                    baseSquare.name = this.gameObject.name + sScript.relativePos.ToString();
                    sList.Add(sScript);
                }
            }
        }
        //transform.position = SpawnScript.Instance.spawnLocations[bNumber - 1].transform.position - Vector3.forward;
        this.transform.localScale = new Vector3(SpawnScript.Instance.blockScale, SpawnScript.Instance.blockScale, 1f);
                
    }

    public void DestroyBlockSprite()
    {
        sList.Clear();
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));
        
    }

    public void RespawnBlock()
    {
        DestroyBlockSprite();
        CreateBlockSprite();

        foreach (SquareScript square in sList)
        {
            square.transform.localPosition = square.relativePos;
            square.transform.localScale = new Vector3(0.9f, 0.9f, 0);
        }
    }

    public void RotateMatrix(bool clockwise)
    {
        int[,] rotatedMatrix = new int[maxBlockSize, maxBlockSize];
        int[,] originalMatrix = bMatrix;

        //Rotate block matrix
        for (int x = 0; x < maxBlockSize; x++)
        {
            for (int y = 0; y < maxBlockSize; y++)
            {
                if (clockwise)
                    rotatedMatrix[x, y] = originalMatrix[maxBlockSize - y - 1, x];
                else
                    rotatedMatrix[x, y] = originalMatrix[y, maxBlockSize - x - 1];
            }
        }
        bMatrix = rotatedMatrix;
        IDindex = (IDindex % 4);
        RespawnBlock();
    }

    void GetBlockIDs()
    {
        for (int r = 0; r < 4; r++)
        {
            int bitPos = 0;
            int bID = 0;
            for (int y = 0; y < SpawnScript.Instance.blockSize; y++)
            {
                for (int x = SpawnScript.Instance.blockSize - 1; x >= 0; x--)
                {
                    if (bMatrix[x, y] == bNumber)
                    {
                        bID += (int)Mathf.Pow(2, bitPos);
                    }
                    bitPos++;
                }
            }
            rotationIDs[r] = System.Convert.ToString(bID, 2);
            char[] removeChars = { '0' };
            rotationIDs[r] = rotationIDs[r].TrimEnd(removeChars);

            RotateMatrix(true);
        }
    }

}