using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Role
{
	public class RoleCustomization : MonoBehaviour
	{
		[SerializeField]
		private Transform
			head,
			back,
			lHand,
			rHand;

		[SerializeField,Header("肤色")]
		public ESkinColor skinColor;


		[SerializeField, Header("头型")]
		public EHeadType headType;
	

		[SerializeField, Header("表情")]
		public EExpression expression;


		[SerializeField, Header("头发")]
		public EHair hair;

		[SerializeField, Header("帽子")]
		public EHat hat;

		[SerializeField, Header("背部工具")]
		public EBackTool backTool;

		[SerializeField, Header("手部工具")]
		public EBackTool lHandTool;
		public EBackTool rHandTool;

		// Start is called before the first frame update
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{

		}
	}

	public enum ESkinColor
	{
		Yellow,
		White,
		Black,
	}

	public enum EHeadType
	{
		Small,
		Big,
	}

	public enum EHair
	{
		None = -1,

	}

	public enum EHat
	{
		None = -1,
		Farmer,
		Traders,


	}

	public enum EExpression
	{
		Smell,
		Sad,
		Angry,
	}

	public enum EBackTool
	{
		None = -1,
	}
	public enum EHandTool
	{
		None = -1,
	}

}

