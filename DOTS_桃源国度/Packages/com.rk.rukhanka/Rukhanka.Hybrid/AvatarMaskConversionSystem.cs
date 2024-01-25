using Unity.Collections;
using UnityEngine;
using FixedStringName = Unity.Collections.FixedString512Bytes;
using Hash128 = Unity.Entities.Hash128;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Hybrid
{ 
public class AvatarMaskConversionSystem
{
	static public RTP.AvatarMask PrepareAvatarMaskComputeData(AvatarMask am)
	{
		var rv = new RTP.AvatarMask();
		if (am != null)
		{
			rv.includedBonePaths = new (am.transformCount, Allocator.Persistent);
			rv.name = am.ToString();
			for (int i = 0; am != null && i < am.transformCount; ++i)
			{
				var bonePath = am.GetTransformPath(i);
				var boneActive = am.GetTransformActive(i);
				if (bonePath.Length == 0 || !boneActive) continue;
				var boneNames = bonePath.Split('/');
				var leafBoneName = new FixedStringName(boneNames[boneNames.Length - 1]);
				rv.includedBonePaths.Add(leafBoneName);
			#if RUKHANKA_DEBUG_INFO
				Debug.Log($"Adding avatar mask bone '{leafBoneName}'");
			#endif
			}
			rv.hash = new Hash128((uint)am.GetHashCode(), 12, 13, 14);

			//	Humanoid avatar mask
			var humanBodyPartsCount = (int)AvatarMaskBodyPart.LastBodyPart;
			rv.humanBodyPartsAvatarMask = 0;
			for (int i = 0; i < humanBodyPartsCount; ++i)
			{
				var ambp = (AvatarMaskBodyPart)i;
				if (am.GetHumanoidBodyPartActive(ambp))
					rv.humanBodyPartsAvatarMask |= 1u << i;
			}
		}
		return rv;
	}
}
}