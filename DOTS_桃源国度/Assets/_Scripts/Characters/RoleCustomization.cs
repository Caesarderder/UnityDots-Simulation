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

		[SerializeField,Header("��ɫ")]
		public ESkinColor skinColor;


		[SerializeField, Header("ͷ��")]
		public EHeadType headType;
	

		[SerializeField, Header("����")]
		public EExpression expression;


		[SerializeField, Header("ͷ��")]
		public EHair hair;

		[SerializeField, Header("ñ��")]
		public EHat hat;

		[SerializeField, Header("��������")]
		public EBackTool backTool;

		[SerializeField, Header("�ֲ�����")]
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

