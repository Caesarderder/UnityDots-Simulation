using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>The base type of all shape components</summary>
	[DisallowMultipleComponent]
	public abstract class ShapeRenderer : MonoBehaviour {

		bool initializedComponents = false;
		MeshRenderer rnd;
		MeshFilter mf;
		int meshOwnerID;
		MaterialPropertyBlock mpb;
		MaterialPropertyBlock Mpb => mpb ?? ( mpb = new MaterialPropertyBlock() ); // hecking, gosh, I want the C#8 ??= operator
		Material[] instancedMaterials = null; // used when pass tags are anything but the default (eg ZTest != Less Equal, or scale offset is set, or a weird blend mode)
		internal bool IsUsingUniqueMaterials => IsInstanced == false;

		/// <summary>Used to mark this mesh as changed. Only required when manually editing the point lists of polygons and polylines, otherwise you shouldn't ever need to use this</summary>
		[System.NonSerialized] public bool meshOutOfDate = true;

		/// <summary>The current mesh in use by this shape. Note: Do not directly modify this mesh, it may not update properly or your changes will be overwritten or you'll modify assets. It's here for reading purposes only</summary>
		public Mesh Mesh {
			get => mf.sharedMesh;
			private set => mf.sharedMesh = value;
		}
		/// <summary>The sorting layer ID of this renderer</summary>
		public int SortingLayerID {
			get => MakeSureComponentExists( ref rnd, out _ ).sortingLayerID;
			set => MakeSureComponentExists( ref rnd, out _ ).sortingLayerID = value;
		}
		/// <summary>The sorting order of this renderer</summary>
		public int SortingOrder {
			get => MakeSureComponentExists( ref rnd, out _ ).sortingOrder;
			set => MakeSureComponentExists( ref rnd, out _ ).sortingOrder = value;
		}

		/// <summary>Gets the name of the current sorting layer of this renderer</summary>
		public string SortingLayerName => SortingLayer.IDToName( SortingLayerID );


		// Properties
		[SerializeField] ShapesBlendMode blendMode = ShapesBlendMode.Transparent;
		/// <summary>What blending mode to use</summary>
		public ShapesBlendMode BlendMode {
			get => blendMode;
			set {
				blendMode = value;
				UpdateMaterial();
			}
		}
		[SerializeField] ScaleMode scaleMode = ScaleMode.Uniform;
		/// <summary>Sets how this shape should behave when scaled</summary>
		public ScaleMode ScaleMode {
			get => scaleMode;
			set => SetIntNow( ShapesMaterialUtils.propScaleMode, (int)( scaleMode = value ) );
		}
		[SerializeField] [ShapesColorField( true )] private protected Color color = Color.white;
		/// <summary>The color of this shape. The alpha channel is used for opacity/intensity in all blend modes</summary>
		public virtual Color Color {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] private protected DetailLevel detailLevel = DetailLevel.Medium;
		/// <summary>What detail level to use for 3D primitives (3D Lines/Sphere/Torus/Cone)</summary>
		public virtual DetailLevel DetailLevel {
			get => detailLevel;
			set {
				detailLevel = value;
				UpdateMesh( force: true );
			}
		}

		#region instancing breaking pass tags

		// if and only if all are set to their default values
		bool IsInstanced => UsingDefaultZTests && UsingDefaultMasking && UsingDefaultRenderQueue;

		// render queue offset
		bool UsingDefaultRenderQueue => renderQueue == DEFAULT_RENDER_QUEUE_AUTO;
		public int RenderQueue {
			get => renderQueue;
			set {
				renderQueue = value;
				if( IsUsingUniqueMaterials ) {
					UpdateMaterial();
					foreach( Material instancedMaterial in instancedMaterials )
						instancedMaterial.renderQueue = renderQueue;
				}
			}
		}
		[SerializeField] int renderQueue = DEFAULT_RENDER_QUEUE_AUTO;

		/// <summary>The default render queue, which is auto-set based on blend mode</summary>
		public const int DEFAULT_RENDER_QUEUE_AUTO = -1;

		// Z testing properties
		/// <summary>The default Z test (depth test) value of LessEqual</summary>
		public const CompareFunction DEFAULT_ZTEST = CompareFunction.LessEqual;

		/// <summary>The default Z offset factor of 0</summary>
		public const float DEFAULT_ZOFS_FACTOR = 0f;

		/// <summary>The default Z offset units of 0</summary>
		public const int DEFAULT_ZOFS_UNITS = 0;

		/// <summary>The default ColorMask of 15 (RGBA)</summary>
		public const ColorWriteMask DEFAULT_COLOR_MASK = ColorWriteMask.All;

		bool UsingDefaultZTests => zTest == DEFAULT_ZTEST && zOffsetFactor == DEFAULT_ZOFS_FACTOR && zOffsetUnits == DEFAULT_ZOFS_UNITS;
		[SerializeField] CompareFunction zTest = DEFAULT_ZTEST;
		/// <inheritdoc cref="RenderState.zTest"/>
		public CompareFunction ZTest {
			get => zTest;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propZTest, (int)( zTest = value ) );
		}
		[SerializeField] float zOffsetFactor = DEFAULT_ZOFS_FACTOR;
		/// <inheritdoc cref="RenderState.zOffsetFactor"/>
		public float ZOffsetFactor {
			get => zOffsetFactor;
			set => SetFloatOnAllInstancedMaterials( ShapesMaterialUtils.propZOffsetFactor, zOffsetFactor = value );
		}
		[SerializeField] int zOffsetUnits = DEFAULT_ZOFS_UNITS;
		/// <inheritdoc cref="RenderState.zOffsetUnits"/>
		public int ZOffsetUnits {
			get => zOffsetUnits;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propZOffsetUnits, zOffsetUnits = value );
		}

		[SerializeField] ColorWriteMask colorMask = DEFAULT_COLOR_MASK;
		/// <inheritdoc cref="RenderState.colorMask"/>
		public ColorWriteMask ColorMask {
			get => colorMask;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propColorMask, (int)( colorMask = value ) );
		}

		// stencil
		/// <summary>The default stencil compare function of CompareFunction.Always</summary>
		public const CompareFunction DEFAULT_STENCIL_COMP = CompareFunction.Always;

		/// <summary>The default stencil operation of StencilOp.Keep</summary>
		public const StencilOp DEFAULT_STENCIL_OP = StencilOp.Keep;

		/// <summary>The default stencil ref id 0</summary>
		public const byte DEFAULT_STENCIL_REF_ID = 0;

		/// <summary>The default stencil mask of 255</summary>
		public const byte DEFAULT_STENCIL_MASK = 255; // read & write

		bool UsingDefaultMasking => stencilComp == DEFAULT_STENCIL_COMP && stencilOpPass == DEFAULT_STENCIL_OP && stencilRefID == DEFAULT_STENCIL_REF_ID && stencilReadMask == DEFAULT_STENCIL_MASK && stencilWriteMask == DEFAULT_STENCIL_MASK && colorMask == DEFAULT_COLOR_MASK;
		[SerializeField] CompareFunction stencilComp = DEFAULT_STENCIL_COMP;
		/// <inheritdoc cref="RenderState.colorMask"/>
		public CompareFunction StencilComp {
			get => stencilComp;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propStencilComp, (int)( stencilComp = value ) );
		}
		[SerializeField] StencilOp stencilOpPass = DEFAULT_STENCIL_OP;
		/// <inheritdoc cref="RenderState.stencilComp"/>
		public StencilOp StencilOpPass {
			get => stencilOpPass;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propStencilOpPass, (int)( stencilOpPass = value ) );
		}
		[SerializeField] byte stencilRefID = DEFAULT_STENCIL_REF_ID;
		/// <inheritdoc cref="RenderState.stencilOpPass"/>
		public byte StencilRefID {
			get => stencilRefID;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propStencilID, stencilRefID = value );
		}
		[SerializeField] byte stencilReadMask = DEFAULT_STENCIL_MASK;
		/// <inheritdoc cref="RenderState.stencilRefID"/>
		public byte StencilReadMask {
			get => stencilReadMask;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propStencilReadMask, stencilReadMask = value );
		}
		[SerializeField] byte stencilWriteMask = DEFAULT_STENCIL_MASK;
		/// <inheritdoc cref="RenderState.stencilReadMask"/>
		public byte StencilWriteMask {
			get => stencilWriteMask;
			set => SetIntOnAllInstancedMaterials( ShapesMaterialUtils.propStencilWriteMask, stencilWriteMask = value );
		}

		#endregion

		#if UNITY_EDITOR

		public virtual void OnValidate() {
			// OnValidate can get called before awake in editor, so make sure the required things are initialized
			if( rnd == null ) rnd = GetComponent<MeshRenderer>(); // Needed for ApplyProperties
			if( mf == null ) mf = GetComponent<MeshFilter>(); // Needed for UpdateMesh
			ShapeClampRanges();
			UpdateAllMaterialProperties();
			ApplyProperties();

			if( MeshUpdateMode == MeshUpdateMode.SelfGenerated )
				meshOutOfDate = true; // we can't update the mesh in OnValidate because of https://issuetracker.unity3d.com/issues/warning-appears-when-changing-meshfilter-dot-sharedmesh-during-onvalidate
			// UpdateMesh( force:true ); gosh I wish I could do this it would solve so many problems but Unity has some WEIRD quirks here
		}

		internal void HideMeshFilterRenderer() {
			VerifyComponents();
			const HideFlags flags = HideFlags.HideInInspector; // Hide mesh renderer and filter
			rnd.hideFlags = flags;
			mf.hideFlags = flags;
		}
		#endif

		T MakeSureComponentExists<T>( ref T field, out bool created ) where T : Component {
			if( field == null ) {
				field = GetComponent<T>();
				if( field == null ) {
					field = gameObject.AddComponent<T>();
					created = true;
				}

				field.hideFlags = HideFlags.HideInInspector;
			}

			created = false;
			return field;
		}

		void VerifyComponents() {
			if( initializedComponents == false ) {
				initializedComponents = true;
				MakeSureComponentExists( ref mf, out _ );
				MakeSureComponentExists( ref rnd, out bool createdRnd );
			}

			// these have to be turned off aggressively, otherwise batching can break
			if( rnd.receiveShadows )
				rnd.receiveShadows = false;
			if( rnd.shadowCastingMode != ShadowCastingMode.Off )
				rnd.shadowCastingMode = ShadowCastingMode.Off;
			if( rnd.lightProbeUsage != LightProbeUsage.Off )
				rnd.lightProbeUsage = LightProbeUsage.Off;
			if( rnd.reflectionProbeUsage != ReflectionProbeUsage.Off )
				rnd.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}

		public virtual void Awake() {
			VerifyComponents();
			UpdateMaterial();
			UpdateMesh();
			UpdateAllMaterialProperties();
		}

		bool HasGeneratedOrCopyOfMesh => MeshUpdateMode == MeshUpdateMode.SelfGenerated || MeshUpdateMode == MeshUpdateMode.UseAssetCopy;

		public virtual void OnEnable() {
			UpdateMesh();
			rnd.enabled = true;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				UnityEditor.Undo.undoRedoPerformed += UpdateMeshOnUndoRedo;
			#endif
			if( UseCamOnPreCull )
				SubscribeCamPreCull();
		}

		void OnDisable() {
			if( rnd != null )
				rnd.enabled = false;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				UnityEditor.Undo.undoRedoPerformed -= UpdateMeshOnUndoRedo;
			#endif
			if( UseCamOnPreCull )
				UnsubscribeCamPreCull();
		}

		// These are used for polyline and polygon to detect mesh changes and lazily update before rendering
		void OnPreCamCullWithCam( Camera cam ) => CamOnPreCull();

		#if UNITY_2019_1_OR_NEWER
		void OnPreCamCullWithCam( ScriptableRenderContext ctx, Camera cam ) => CamOnPreCull();
		#endif

		void SubscribeCamPreCull() {
			if( UnityInfo.UsingSRP ) {
				#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering += OnPreCamCullWithCam;
				#else
				UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += OnPreCamCullWithCam;
				#endif
			} else
				Camera.onPreCull += OnPreCamCullWithCam;
		}

		void UnsubscribeCamPreCull() {
			if( UnityInfo.UsingSRP ) {
				#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= OnPreCamCullWithCam;
				#else
				UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= OnPreCamCullWithCam;
				#endif
			} else
				Camera.onPreCull -= OnPreCamCullWithCam;
		}


		#if UNITY_EDITOR
		void UpdateMeshOnUndoRedo() => UpdateMesh( true );
		#endif

		void Reset() {
			UpdateAllMaterialProperties();
			UpdateMesh( true );
		}

		void OnDestroy() {
			if( HasGeneratedOrCopyOfMesh && Mesh != null )
				DestroyImmediate( Mesh );
			this.TryDestroyInOnDestroy( rnd );
			this.TryDestroyInOnDestroy( mf );
			TryDestroyInstancedMaterials( inOnDestroy: true );
		}

		private protected abstract Bounds GetBounds_Internal();
		private protected abstract void SetAllMaterialProperties();
		private protected virtual void ShapeClampRanges() => _ = 0;
		private protected abstract Material[] GetMaterials();
		private protected virtual void GenerateMesh() => _ = 0;
		private protected virtual Mesh GetInitialMeshAsset() => ShapesMeshUtils.QuadMesh[HasDetailLevels ? (int)DetailLevel.Medium : 0];
		private protected virtual MeshUpdateMode MeshUpdateMode => MeshUpdateMode.UseAsset;
		internal virtual bool HasScaleModes => true;
		internal virtual bool HasDetailLevels => true;
		private protected virtual bool UseCamOnPreCull => false;
		internal virtual void CamOnPreCull() => _ = 0;

		void UpdateMeshBounds() => Mesh.bounds = GetBounds_Internal();


		void TryDestroyInstancedMaterials( bool inOnDestroy = false ) {
			if( instancedMaterials != null ) {
				for( int i = 0; i < instancedMaterials.Length; i++ ) {
					if( instancedMaterials[i] != null ) {
						if( inOnDestroy )
							this.TryDestroyInOnDestroy( instancedMaterials[i] );
						else
							instancedMaterials[i].DestroyBranched();
					}
				}
			}
		}

		void MakeSureMaterialInstancesAreGood( Material[] sourceMats ) {
			Material InstantiateMaterial( int index ) => new Material( sourceMats[index] ) { name = sourceMats[index].name + " (instance)" };

			void PopulateAll() {
				instancedMaterials = new Material[sourceMats.Length];
				for( int i = 0; i < sourceMats.Length; i++ )
					instancedMaterials[i] = InstantiateMaterial( i );
			}

			if( instancedMaterials == null ) {
				// no instanced materials exist, create all
				PopulateAll();
			} else {
				if( instancedMaterials.Length != sourceMats.Length ) {
					// length mismatch, regenerate all
					TryDestroyInstancedMaterials();
					PopulateAll();
				} else {
					// same length! make sure they are all matching
					for( int i = 0; i < sourceMats.Length; i++ ) {
						if( instancedMaterials[i] == null ) {
							// if null, create instance
							instancedMaterials[i] = InstantiateMaterial( i );
						} else {
							// if not null, then make sure the shader is matching
							if( instancedMaterials[i].shader != sourceMats[i].shader ) {
								// mismatch, destroy instance and assign new one
								instancedMaterials[i].DestroyBranched();
								instancedMaterials[i] = InstantiateMaterial( i );
							} else {
								// they do use the same shader, but, make sure they also use the same keywords just to be safe
								instancedMaterials[i].shaderKeywords = sourceMats[i].shaderKeywords;
							}
						}
					}
				}
			}
		}

		private protected void UpdateMaterial() {
			Material[] targetMats = GetMaterials();

			// this means we have unique material properties for this shape, so, instantiate them
			// but! only when they're in the scene
			if( IsUsingUniqueMaterials ) {
				MakeSureMaterialInstancesAreGood( targetMats );
				targetMats = instancedMaterials;
			}

			VerifyComponents();

			#if UNITY_EDITOR
			if( EditorApplication.isPlaying == false )
				UpdateMaterialsEditorMode( targetMats );
			else
				#endif
				rnd.sharedMaterials = targetMats;
		}

		#if UNITY_EDITOR
		void UpdateMaterialsEditorMode( Material[] targetMats ) {
			bool needsUpdate = false;
			if( rnd.sharedMaterials.Length != targetMats.Length ) {
				needsUpdate = true;
			} else {
				for( int i = 0; i < targetMats.Length; i++ ) {
					if( rnd.sharedMaterials[i] != targetMats[i] ) {
						needsUpdate = true;
						break;
					}
				}
			}

			if( needsUpdate ) {
				Undo.RecordObject( rnd, "" );
				rnd.sharedMaterials = targetMats;
			}
		}
		#endif

		/// <summary>Updates the mesh this object is using.
		/// If this mesh generates procedural meshes, such as the polygon or polyline, it will regenerate it if force is set to true</summary>
		/// <param name="force"></param>
		public void UpdateMesh( bool force = false ) {
			MeshUpdateMode mode = MeshUpdateMode;

			// if we're using a mesh asset, we only assign if it's null or mismatching
			if( mode == MeshUpdateMode.UseAsset && ( Mesh == null || Mesh != GetInitialMeshAsset() ) ) {
				Mesh = GetInitialMeshAsset();
				return;
			}

			// the next two modes are copy-sensitive, meaning that if we duplicate this object,
			// we also have to duplicate the mesh and update which mesh the duplicate is pointing to
			int id = gameObject.GetInstanceID();

			bool createMesh = Mesh == null || meshOwnerID != id;

			// create new mesh
			if( createMesh ) {
				meshOwnerID = id;
				if( mode == MeshUpdateMode.UseAssetCopy ) {
					Mesh = Instantiate( GetInitialMeshAsset() );
					Mesh.hideFlags = HideFlags.HideAndDontSave;
					Mesh.MarkDynamic();
				} else if( mode == MeshUpdateMode.SelfGenerated ) {
					Mesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave };
					Mesh.MarkDynamic();
					GenerateMesh();
				}
			} else if( force && mode == MeshUpdateMode.SelfGenerated ) {
				GenerateMesh(); // update existing mesh
			}
		}

		/// <summary><para>Returns the local space bounds</para>
		/// <para>Note: This does not take into account screen space sizing, so it will only behave as you expect when using meters only</para></summary>
		public Bounds GetBounds() => GetBounds_Internal();

		/// <summary><para>Returns the world space bounds of the local space bounds</para>
		/// <para>Note: This does not take into account screen space sizing, so it will only behave as you expect when using meters only</para></summary>
		public Bounds GetWorldBounds() {
			Bounds localBounds = GetBounds_Internal();
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;

			Transform tf = transform;
			for( int x = -1; x <= 1; x += 2 )
				for( int y = -1; y <= 1; y += 2 )
					for( int z = -1; z <= 1; z += 2 ) {
						Vector3 wPt = tf.TransformPoint( localBounds.center + Vector3.Scale( localBounds.extents, new Vector3( x, y, z ) ) );
						min = Vector3.Min( min, wPt );
						max = Vector3.Max( max, wPt );
					}

			return new Bounds( ( max + min ) / 2f, max - min );
		}

		void OnDidApplyAnimationProperties() => UpdateAllMaterialProperties(); // so this is not great but it works don't judge

		void SetIntOnAllInstancedMaterials( int property, int value ) {
			if( IsUsingUniqueMaterials ) {
				UpdateMaterial();
				foreach( Material instancedMaterial in instancedMaterials )
					instancedMaterial.SetInt_Shapes( property, value );
			}
		}

		void SetFloatOnAllInstancedMaterials( int property, float value ) {
			if( IsUsingUniqueMaterials ) {
				UpdateMaterial();
				foreach( Material instancedMaterial in instancedMaterials )
					instancedMaterial.SetFloat( property, value );
			}
		}

		internal void UpdateAllMaterialProperties() {
			if( gameObject.scene.IsValid() == false )
				return; // not in a scene :c

			UpdateMaterial();

			if( IsUsingUniqueMaterials ) // I wish we could material property block these ;-;
				foreach( Material instancedMaterial in instancedMaterials ) {
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propZTest, (int)zTest );
					instancedMaterial.SetFloat( ShapesMaterialUtils.propZOffsetFactor, zOffsetFactor );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propZOffsetUnits, zOffsetUnits );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propColorMask, (int)colorMask );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propStencilComp, (int)stencilComp );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propStencilOpPass, (int)stencilOpPass );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propStencilID, stencilRefID );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propStencilReadMask, stencilReadMask );
					instancedMaterial.SetInt_Shapes( ShapesMaterialUtils.propStencilWriteMask, stencilWriteMask );
					instancedMaterial.renderQueue = renderQueue;
				}

			SetColor( ShapesMaterialUtils.propColor, color );
			if( HasScaleModes )
				SetInt( ShapesMaterialUtils.propScaleMode, (int)scaleMode );
			SetAllMaterialProperties();
			ApplyProperties();
		}

		private protected void ApplyProperties() {
			VerifyComponents(); // make sure components exists. rnd can be uninitialized if you modify an object that has never had awake called
			rnd.SetPropertyBlock( Mpb );
			if( MeshUpdateMode == MeshUpdateMode.UseAssetCopy )
				UpdateMeshBounds();
		}

		private protected void SetAllDashValues( DashStyle style, bool dashed, bool matchSpacingToSize, float thickness, bool setType, bool now ) {
			float netDashSize = style.GetNetAbsoluteSize( dashed, thickness );
			if( dashed ) {
				SetFloat( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing( style, true, matchSpacingToSize, thickness ) );
				SetFloat( ShapesMaterialUtils.propDashOffset, style.offset );
				SetInt( ShapesMaterialUtils.propDashSpace, (int)style.space );
				SetInt( ShapesMaterialUtils.propDashSnap, (int)style.snap );
				if( setType ) {
					SetInt( ShapesMaterialUtils.propDashType, (int)style.type );
					if( style.type.HasModifier() )
						SetFloat( ShapesMaterialUtils.propDashShapeModifier, style.shapeModifier );
				}
			}

			if( now )
				SetFloatNow( ShapesMaterialUtils.propDashSize, netDashSize );
			else
				SetFloat( ShapesMaterialUtils.propDashSize, netDashSize );
		}

		private protected float GetNetDashSpacing( DashStyle style, bool dashed, bool matchSpacingToSize, float thickness ) {
			if( matchSpacingToSize && style.space == DashSpace.FixedCount )
				return 0.5f;
			return matchSpacingToSize ? style.GetNetAbsoluteSize( dashed, thickness ) : style.GetNetAbsoluteSpacing( dashed, thickness );
		}


		private protected void SetColor( int prop, Color value ) {
			if( ShapeGroup.shapeGroupsInScene > 0 ) { // if color tint groups exist, see if we have any
				ShapeGroup[] groups = GetComponentsInParent<ShapeGroup>();
				if( groups != null )
					foreach( ShapeGroup shapeGroup in groups.Where( g => g.IsEnabled ) )
						value *= shapeGroup.Color;
			}

			Mpb.SetColor( prop, value );
		}

		private protected void SetFloat( int prop, float value ) => Mpb.SetFloat( prop, value );
		private protected void SetInt( int prop, int value ) => Mpb.SetInt_Shapes( prop, value );
		private protected void SetVector3( int prop, Vector3 value ) => Mpb.SetVector( prop, value );
		private protected void SetVector4( int prop, Vector4 value ) => Mpb.SetVector( prop, value );

		private protected void SetColorNow( int prop, Color value ) {
			SetColor( prop, value );
			ApplyProperties();
		}

		private protected void SetFloatNow( int prop, float value ) {
			Mpb.SetFloat( prop, value );
			ApplyProperties();
		}

		private protected void SetIntNow( int prop, int value ) {
			Mpb.SetInt_Shapes( prop, value );
			ApplyProperties();
		}

		private protected void SetVector3Now( int prop, Vector3 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}

		private protected void SetVector4Now( int prop, Vector4 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}


	}

}