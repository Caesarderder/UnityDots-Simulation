using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectsDatabaseSO database;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
	PathMap map;
	 

	public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDatabaseSO database,
                          GridData floorData,
                          GridData furnitureData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback,
						  PathMap map
							 )
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
		this.map = map;

		selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
        else
            throw new System.Exception($"No object with ID {iD}");
        
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

		var data = database.objectsData[selectedObjectIndex];



		if (placementValidity == false|| data.WoodCost>SourceBarUI.Instance.source.WoodCur)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);

		var pos = grid.CellToWorld(gridPosition);
		//
		//Debug.Log("MoveCost:"+ data.MoveCost);
		int index = objectPlacer.PlaceObject(pos, data.Size,data.ID, data.WoodCost,data.MoveCost
			);

        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;
        selectedData.AddObjectAt(gridPosition,
            database.objectsData[selectedObjectIndex].Size,
            database.objectsData[selectedObjectIndex].ID,
            index);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);

		//µÀÂ·	ToUpdate
		if (300<database.objectsData[selectedObjectIndex].ID&& database.objectsData[selectedObjectIndex].ID <400)
		{
			List<Vector2> offsets = new();
			offsets.Add(new Vector2(0.25f, 0.25f));
			offsets.Add(new Vector2(0.75f, 0.25f));
			offsets.Add(new Vector2(0.75f, 0.75f));
			offsets.Add(new Vector2(0.25f, 0.75f));

			map.AddMovableBlock(new MovableBlock(new Vector2(gridPosition.x, gridPosition.z), offsets,1f));
		}


	
    }




    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;

        return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }
}

