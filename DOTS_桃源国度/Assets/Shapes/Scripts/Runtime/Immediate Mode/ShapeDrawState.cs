using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	struct ShapeDrawState {
		public Mesh mesh;
		public Material mat;
		public int submesh;

		internal bool CompatibleWith( ShapeDrawState other ) => mesh == other.mesh && submesh == other.submesh && mat == other.mat;
	}

	// this is the type that is submitted to the render call
	struct ShapeDrawCall {
		public ShapeDrawState drawState;
		public MaterialPropertyBlock mpb;
		public int count;
		public Matrix4x4 matrix;
		public Matrix4x4[] matrices;
		bool instanced;

		public ShapeDrawCall( ShapeDrawState drawState, Matrix4x4 matrix ) {
			this.count = 1;
			this.drawState = drawState;
			this.matrix = matrix;
			this.instanced = false;
			this.mpb = ObjectPool<MaterialPropertyBlock>.Alloc();
			matrices = null;
		}

		public ShapeDrawCall( ShapeDrawState drawState, int count, Matrix4x4[] matrices ) {
			this.count = count;
			this.drawState = drawState;
			this.matrices = matrices;
			this.instanced = true;
			this.mpb = ObjectPool<MaterialPropertyBlock>.Alloc();
			matrix = default;
		}

		public void AddToCommandBuffer( CommandBuffer cmd ) {
			if( instanced )
				cmd.DrawMeshInstanced( drawState.mesh, drawState.submesh, drawState.mat, 0, matrices, count, mpb );
			else
				cmd.DrawMesh( drawState.mesh, matrix, drawState.mat, drawState.submesh, 0, mpb );
		}

		public void Cleanup() { // called after this draw call has rendered
			mpb.Clear();
			ObjectPool<MaterialPropertyBlock>.Free( mpb );
			if( instanced )
				ArrayPool<Matrix4x4>.Free( matrices );
			drawState.mat = null; // to ensure we don't have references to assets lying around
			drawState.mesh = null;
		}

	}

}