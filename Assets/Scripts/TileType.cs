using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string nameTile;
    public GameObject TilePrefab;
    public GameObject UnitonTile;
    public bool isWalkable = true;
    public float MoveCost = 1;
}
