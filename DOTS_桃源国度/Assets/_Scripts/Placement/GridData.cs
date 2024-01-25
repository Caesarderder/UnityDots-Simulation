using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridPosition,
                            Vector2Int objectSize,
                            int ID,
                            int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary already contains this cell positiojn {pos}");
            placedObjects[pos] = data;

		}
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnVal = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnVal;
    }

    public bool CanPlaceObejctAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }

	public Vector2Int GetSizeByPos(Vector3Int gridPosition)
	{
		if (placedObjects.ContainsKey(gridPosition) == false)
		{
			Debug.LogError("None");
			return Vector2Int.zero;
		}
		var postions=placedObjects[gridPosition].occupiedPositions;
		var xMin = postions[0].x;
		var yMin = postions[0].z;
		var xMax = postions[0].x;
		var yMax = postions[0].z;
		foreach (var pos in postions)
		{

			if(pos.z>yMax)
				yMax = pos.z;
			if (pos.x > xMax)
				xMax = pos.x;

			if (pos.z < yMin)
				yMin = pos.z;
			if (pos.x < xMin)
				xMin = pos.x;
		}

		return new Vector2Int(xMax-xMin+1,yMax-yMin+1);
	}

    internal int GetRepresentationIndex(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].PlacedObjectIndex;
    }

	public int GetIDByPosistion(Vector3Int gridPosition)
	{
	
		if (placedObjects.ContainsKey(gridPosition) == false)
		{
			
			return -1;
		}

		return placedObjects[gridPosition].ID;
	}

	internal void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in placedObjects[gridPosition].occupiedPositions)
        {
            placedObjects.Remove(pos);
        }
    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}
