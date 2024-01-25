using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Transforms;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

[DisableAutoCreation]
public partial class BoneVisualizationSystem: SystemBase
{
	EntityQuery boneVisualizeQuery;

	struct BoneGPUData
	{
		public float3 pos0, pos1;
		public float4 colorTri, colorLines;
	}

	ComputeBuffer boneGPUDataCB;
	NativeList<BoneGPUData> boneGPUData;
	Mesh boneMesh;
	Material boneRendererMaterial;
	Bounds bigBBox = new Bounds(Vector3.zero, Vector3.one * 100000000);
	MaterialPropertyBlock mpb;

	int boneDataBuffer_ShaderID;
	int isLines_ShaderID;

/////////////////////////////////////////////////////////////////////////////////

	void CreateBoneMeshes()
	{
		boneMesh = new Mesh();
		boneMesh.subMeshCount = 2;

		var vtx = new Vector3[6];
		vtx[0] = new Vector3(0, 1, 0);
		vtx[5] = new Vector3(0, -1, 0);
		vtx[1] = new Vector3(-1, 0, 0);
		vtx[2] = new Vector3(1, 0, 0);
		vtx[3] = new Vector3(0, 0, -1);
		vtx[4] = new Vector3(0, 0, 1);

		for (int i = 0; i < vtx.Length; ++i)
			vtx[i] *= 0.1f;

		var triIdx = new int[]
		{
			0, 1, 4,
			0, 4, 2,
			0, 2, 3,
			0, 3, 1,

			5, 4, 1,
			5, 2, 4,
			5, 3, 2,
			5, 1, 3,
		};

		var lineIdx = new int[]
		{
			0, 1,
			0, 2, 
			0, 3,
			0, 4,
			5, 1,
			5, 2, 
			5, 3,
			5, 4,
			2, 4,
			1, 4,
			1, 3,
			2, 3,
		};

		boneMesh.SetVertices(vtx);
		boneMesh.SetIndices(triIdx, MeshTopology.Triangles, 0);
		boneMesh.SetIndices(lineIdx, MeshTopology.Lines, 1);
	}

/////////////////////////////////////////////////////////////////////////////////

	Material CreateBoneRendererMaterial()
	{
	#if HDRP_10_0_0_OR_NEWER
		var matName = "RukhankaBoneRendererHDRP";
	#elif URP_10_0_0_OR_NEWER
		var matName = "RukhankaBoneRendererURP";
	#endif

		var mat = Resources.Load<Material>(matName);
		mat.enableInstancing = true;
		return mat;
	}

/////////////////////////////////////////////////////////////////////////////////

	protected override void OnCreate()
	{
		CreateBoneMeshes();
		boneRendererMaterial = CreateBoneRendererMaterial();
		mpb = new MaterialPropertyBlock();

		isLines_ShaderID = Shader.PropertyToID("isLines");
		boneDataBuffer_ShaderID = Shader.PropertyToID("boneDataBuf");

		var ecb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll
		<RigDefinitionComponent,
		#if !RUKHANKA_DEBUG_INFO
			BoneVisualizationComponent,
		#endif
			LocalTransform>();
		boneVisualizeQuery = GetEntityQuery(ecb0);

		boneGPUData = new (Allocator.Persistent);

		RequireForUpdate(boneVisualizeQuery);
	}

/////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		boneGPUDataCB?.Release();
	}

/////////////////////////////////////////////////////////////////////////////////

	JobHandle PrepareGPUDataBuf(NativeList<BoneTransform> bonesBuffer, JobHandle dependsOn)
	{
		var resizeDataJob = new ResizeDataBuffersJob()
		{
			boneTransforms = bonesBuffer,
			boneGPUData = boneGPUData
		};

		var rv = resizeDataJob.Schedule(dependsOn);
		return rv;
	}

/////////////////////////////////////////////////////////////////////////////////

	void RenderBones()
	{
		if (boneGPUData.IsEmpty) return;

		if (boneGPUDataCB == null || boneGPUDataCB.count < boneGPUData.Length)
		{
			boneGPUDataCB?.Release();
			boneGPUDataCB = new ComputeBuffer(boneGPUData.Length, UnsafeUtility.SizeOf<BoneGPUData>());
		}
		boneGPUDataCB.SetData(boneGPUData.AsArray());

		boneRendererMaterial.SetBuffer(boneDataBuffer_ShaderID, boneGPUDataCB);
		mpb.Clear();
		mpb.SetInt(isLines_ShaderID, 0);
		Graphics.DrawMeshInstancedProcedural(boneMesh, 0, boneRendererMaterial, bigBBox, boneGPUData.Length, mpb, ShadowCastingMode.Off);
		mpb.SetInt(isLines_ShaderID, 1);
		Graphics.DrawMeshInstancedProcedural(boneMesh, 1, boneRendererMaterial, bigBBox, boneGPUData.Length, mpb, ShadowCastingMode.Off);
	}

/////////////////////////////////////////////////////////////////////////////////

	protected override void OnUpdate()
	{
		var entityArr = boneVisualizeQuery.ToEntityListAsync(WorldUpdateAllocator, Dependency, out var entityArrJH);
		var rigDefArr = boneVisualizeQuery.ToComponentDataListAsync<RigDefinitionComponent>(WorldUpdateAllocator, Dependency, out var rigDefArrJH);

		var runtimeData = SystemAPI.GetSingleton<RuntimeAnimationData>();
		var prepareDataJH = PrepareGPUDataBuf(runtimeData.animatedBonesBuffer, Dependency);

		var combinedJH = JobHandle.CombineDependencies(entityArrJH, rigDefArrJH, prepareDataJH);
		
	#if RUKHANKA_DEBUG_INFO
		SystemAPI.TryGetSingleton<DebugConfigurationComponent>(out var dcc);
	#else
		DebugConfigurationComponent dcc = default;
	#endif

		var boneVisualizeComponentLookup = SystemAPI.GetComponentLookup<BoneVisualizationComponent>(true);
		var prepareRenderDataJob = new PrepareRenderDataJob()
		{
			boneGPUData = boneGPUData.AsParallelWriter(),
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap,
			bonePoses = runtimeData.animatedBonesBuffer,
			rigDefArr = rigDefArr,
			boneVisComponentLookup = boneVisualizeComponentLookup,
			debugConfig = dcc,
			entityArr = entityArr
		};

		var jh = prepareRenderDataJob.Schedule(entityArr, 16, combinedJH);
		jh.Complete();

		RenderBones();
	}
}
}
