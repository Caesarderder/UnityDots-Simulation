using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{
partial class BoneVisualizationSystem
{
[BurstCompile]
struct PrepareRenderDataJob: IJobParallelForDefer
{
    [ReadOnly]
    public NativeList<Entity> entityArr;
    [ReadOnly]
    public NativeList<RigDefinitionComponent> rigDefArr;
	[ReadOnly]
	public NativeList<BoneTransform> bonePoses;
	[ReadOnly]
	public NativeParallelHashMap<Entity, int2> entityToDataOffsetMap;
    [ReadOnly]
    public ComponentLookup<BoneVisualizationComponent> boneVisComponentLookup;
    public DebugConfigurationComponent debugConfig;

    [WriteOnly]
    public NativeList<BoneGPUData>.ParallelWriter boneGPUData;

/////////////////////////////////////////////////////////////////////////////////

    public void Execute(int i)
    {
        var rd = rigDefArr[i];
        var e = entityArr[i];
        var bt = RuntimeAnimationData.GetAnimationDataForRigRO(bonePoses, entityToDataOffsetMap, rd, e);

        if (!boneVisComponentLookup.TryGetComponent(e, out var bvc))
        {
            if (!debugConfig.visualizeAllRigs) return;

            bvc = new BoneVisualizationComponent()
            {
                colorLines = debugConfig.colorLines,
                colorTri = debugConfig.colorTri
            };
        }

        var len = bt.Length;
        
        for (int l = rd.rigBlob.Value.rootBoneIndex; l < len; ++l)
        {
            var bgd = new BoneGPUData();
            ref var rb = ref rd.rigBlob.Value.bones[l];

            if (rb.parentBoneIndex < 0)
                continue;

            bgd.pos0 = bt[l].pos;
            bgd.pos1 = bt[rb.parentBoneIndex].pos;
            bgd.colorTri = bvc.colorTri;
            bgd.colorLines = bvc.colorLines;

            if (math.any(math.abs(bgd.pos0 - bgd.pos1)))
                boneGPUData.AddNoResize(bgd);
        }
    }
}

//=================================================================================================================//

struct ResizeDataBuffersJob: IJob
{
	public NativeList<BoneGPUData> boneGPUData;
    public NativeList<BoneTransform> boneTransforms;
    
/////////////////////////////////////////////////////////////////////////////////

    public void Execute()
    {
        var totalBoneCount = boneTransforms.Length;
		if (boneGPUData.Capacity < totalBoneCount)
		{
			boneGPUData.Capacity = totalBoneCount;
		}
		boneGPUData.Clear();
    }
}
 
}
}
