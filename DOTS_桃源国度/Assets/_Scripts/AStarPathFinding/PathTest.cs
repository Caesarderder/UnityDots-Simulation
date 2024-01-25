using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathTest : MonoBehaviour
{
	public PathMap map;
	public RolePathFinding PathFinding ;

	//Test
	public GameObject Ball;
	public GameObject Blockk;
	public GameObject PathGo;

	private void Awake()
	{
		map = new PathMap();	
		PathFinding = new RolePathFinding(map,this);
	}

	public void CreateBall(Vector2 pos, float F)
	{
		var ball = Instantiate(Ball, new Vector3(pos.x,0f,pos.y),transform.rotation,transform);
		ball.GetComponentInChildren<TMP_Text>().text = "" + F;
	}

	public void CreatePathBall(Vector2 pos)
	{
		Instantiate(PathGo, new Vector3(pos.x, 0f, pos.y), transform.rotation, transform);

	}

	public void DeleteBall()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		/*List<Vector2> points = new List<Vector2>();
		points.Add(new Vector2(0.25f, 0.25f));
		points.Add(new Vector2(0.75f, 0.25f));
		points.Add(new Vector2(0.75f, 0.75f));
		points.Add(new Vector2(0.25f, 0.75f));

		var center = new Vector2(-1, 0);
		Instantiate(Blockk, center,Blockk.transform.rotation);
		MovableBlock path = new MovableBlock(center, points,1f);
		map.AddMovableBlock(path);

		center = new Vector2(0, 0);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(1, 0);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(2, 0);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(3, 0);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(4, 0);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(-1, -1);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(0, -1);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(1, -1);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		//10
		center = new Vector2(4, -1);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(- 1, -2);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(1, -2);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(2, -2);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(4, -2);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(-1, -3);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(2, -3);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(3, -3);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(4, -3);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(-1, -4);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(3, -4);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(-1, -5);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(0, -5);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(1, -5);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(2, -5);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);

		center = new Vector2(3, -5);
		path = new MovableBlock(center, points, 1f);
		map.AddMovableBlock(path);
		Instantiate(Blockk, new Vector3(center.x, 0.1f, center.y), Blockk.transform.rotation);
*/
	}

	private void Update()
	{
/*		if(Input.GetKeyDown(KeyCode.Q))
		{
			
			var start = new Vector2(0.25f, 0.75f);
			var end = new Vector2(3.25f, -2.25f);
			PathFinding.FindPath(start, end);

		}
		if(Input.GetKeyDown(KeyCode.Space))
		{
			PathFinding.test = true;
		}*/
	}
}
