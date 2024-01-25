
using System.Reflection;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{

public static class AvatarExtensions
{
	static readonly MethodInfo getPreRotationFn = typeof(Avatar).GetMethod("GetPreRotation", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo getPostRotationFn = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo getLimitSignFn = typeof(Avatar).GetMethod("GetLimitSign", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo getZYPostQFn = typeof(Avatar).GetMethod("GetZYPostQ", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo getZYRollFn = typeof(Avatar).GetMethod("GetZYRoll", BindingFlags.NonPublic | BindingFlags.Instance);
	static readonly MethodInfo getAxisLengthFn = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.NonPublic | BindingFlags.Instance);

/////////////////////////////////////////////////////////////////////////////////

    public static Quaternion GetZYPostQ(this Avatar a, int humanId, Quaternion parentQ, Quaternion q)
    {
		return (Quaternion)getZYPostQFn.Invoke(a, new object[] {humanId, parentQ, q});
    }

/////////////////////////////////////////////////////////////////////////////////

    public static Quaternion GetZYRoll(this Avatar a, int humanId, Vector3 uvw)
    {
		return (Quaternion)getZYRollFn.Invoke(a, new object[] {humanId, uvw});
    }

/////////////////////////////////////////////////////////////////////////////////

    public static float GetAxisLength(this Avatar a, int humanId)
    {
		return (float)getAxisLengthFn.Invoke(a, new object[] {humanId});
    }

/////////////////////////////////////////////////////////////////////////////////

    public static Quaternion GetPreRotation(this Avatar a, int humanId)
    {
		return (Quaternion)getPreRotationFn.Invoke(a, new object[] {humanId});
    }

/////////////////////////////////////////////////////////////////////////////////

    public static Quaternion GetPostRotation(this Avatar a, int humanId)
    {
		return (Quaternion)getPostRotationFn.Invoke(a, new object[] {humanId});
    }

/////////////////////////////////////////////////////////////////////////////////

    public static Vector3 GetLimitSign(this Avatar a, int humanId)
    {
		return (Vector3)getLimitSignFn.Invoke(a, new object[] {humanId});
    }

/////////////////////////////////////////////////////////////////////////////////

	public static string GetRootMotionNodeName(this Avatar a)
	{
		if (a == null) return "";

		var fi = typeof(HumanDescription).GetField("m_RootMotionBoneName", BindingFlags.NonPublic | BindingFlags.Instance);
		return fi == null ? "" : (string)fi.GetValue(a.humanDescription);
	}
}
}
