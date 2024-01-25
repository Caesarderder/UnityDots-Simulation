using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleController : MonoBehaviour
{
	#region Ѱ·
	public Vector2 EndPos;
	public Vector2 curPos;
	//Test
	public PathTest test;
	public bool o;

	public RolePathFinding pathFinding;

	public List<Point> PathToMove;
	#endregion

	// Start is called before the first frame update
	void Start()
    {
		pathFinding = new RolePathFinding(PlacementSystem.Instance.map, test);

	}

    // Update is called once per frame
    void Update()
    {
		curPos = CurPos();
		if(Input.GetKeyDown(KeyCode.W))
		{
			
			FindPathToTargetPostion();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			test.DeleteBall();
			
		}
		
	}

	public void FindPathToTargetPostion()
	{


		if(pathFinding.FindPath(curPos, EndPos)==null)
		{
			Debug.Log("No Path!");
		}
		else
		{
			Debug.Log("Path!");
		}

	}

	public Vector2 CurPos()
	{
		var pos= transform.position;

		var x1 = pos.x>=0f?0f:-1f;
		var y1 = pos.z >= 0f ? 0f : -1f;

		var curGrid=new Vector2 ((int)pos.x+x1, (int)pos.z+y1);
		
		var x= pos.x-curGrid.x;
		var y = pos.z - curGrid.y;
		return new Vector2(x>=0.5f?0.75f:0.25f, y >= 0.5f ? 0.75f : 0.25f)+curGrid;
	}

	

}
