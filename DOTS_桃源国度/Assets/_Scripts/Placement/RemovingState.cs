using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    SoundFeedback soundFeedback;
	PathMap map;


	public RemovingState(Grid grid,
                         PreviewSystem previewSystem,
                         GridData floorData,
                         GridData furnitureData,
                         ObjectPlacer objectPlacer,
                         SoundFeedback soundFeedback,
						 PathMap map)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;
		this.map = map;


		previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        GridData selectedData = null;
        if(furnitureData.CanPlaceObejctAt(gridPosition,Vector2Int.one) == false)
        {
            selectedData = furnitureData;
        }
        else if(floorData.CanPlaceObejctAt(gridPosition, Vector2Int.one) == false)
        {
            selectedData = floorData;
        }

        if(selectedData == null)
        {
            //sound
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        else
        {
            soundFeedback.PlaySound(SoundType.Remove);
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);

			//µÀÂ·
			var ID = selectedData.GetIDByPosistion(gridPosition);
			var size = selectedData.GetSizeByPos(gridPosition);
			if (300 < ID && ID < 400)
			{
				List<Vector2> offsets = new();
				offsets.Add(new Vector2(0.25f, 0.25f));
				offsets.Add(new Vector2(0.75f, 0.25f));
				offsets.Add(new Vector2(0.75f, 0.75f));
				offsets.Add(new Vector2(0.25f, 0.75f));

				map.RemoveMovableBlock(new MovableBlock(new Vector2(gridPosition.x, gridPosition.z), offsets, 1f));
			}

			if (gameObjectIndex == -1)
                return;
            selectedData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(gameObjectIndex, gridPosition, size);



		}
        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(furnitureData.CanPlaceObejctAt(gridPosition, Vector2Int.one) &&
            floorData.CanPlaceObejctAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }
}
