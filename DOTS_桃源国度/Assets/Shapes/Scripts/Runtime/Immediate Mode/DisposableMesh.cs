using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class DisposableMesh : IDisposable {

		static int activeMeshCount;
		public static int ActiveMeshCount => activeMeshCount;

		protected Mesh mesh;
		protected bool meshDirty = false;
		protected bool hasData = false; // used to detect whether or not we can/should generate a mesh on draw
		bool hasMesh = false; // used to detect if mesh needs to be created on the fly on draw
		bool disposeWhenFullyReleased = false;

		internal List<DrawCommand> usedByCommands = null;

		protected void EnsureMeshExists() {
			if( hasData == false ) {
				Debug.LogError( "Mesh requested, but there's no data to generate a mesh from" );
				return;
			}

			if( hasMesh == false || mesh == null ) {
				mesh = new Mesh { name = "Disposable Mesh", hideFlags = HideFlags.DontSave };
				activeMeshCount++;
				hasMesh = true;
			}
		}

		// called when rendering a polyline
		internal void RegisterToCommandBuffer( DrawCommand cmd ) {
			if( usedByCommands == null ) {
				usedByCommands = ListPool<DrawCommand>.Alloc();
				Add();
			} else if( usedByCommands.Contains( cmd ) == false )
				Add();

			void Add() {
				usedByCommands.Add( cmd );
				cmd.cachedMeshes.Add( this );
			}
		}

		// called when a command is done rendering (or cleared)
		internal void ReleaseFromCommand( DrawCommand cmd ) {
			usedByCommands.Remove( cmd );
			if( usedByCommands.Count == 0 && disposeWhenFullyReleased )
				Dispose(); // now we can dispose this mesh data, it's no longer used
		}

		// • Draw.Polyline_Internal, Draw.Polygon_Internal calls RegisterToCommandBuffer if a buffer is being used
		// • if a Disposable mesh is disposed while registered, it will be marked for deletion later
		// • else if Dispose is called, it will delete/actually dispose the asset
		// • when a command buffer is done rendering, it will call ReleaseFromCommand(),
		// which will unregister and delete if no other buffers are using it
		public void Dispose() {
			disposeWhenFullyReleased = true; // when called inside a DrawCommand that still needs this mesh

			// if no commands are using this, release the command list
			bool hasCommandList = usedByCommands != null;
			if( hasCommandList && usedByCommands.Count == 0 ) {
				ListPool<DrawCommand>.Free( usedByCommands );
				usedByCommands = null;
				hasCommandList = false;
			}

			// if a mesh exists, and no commands are using it, remove it
			if( hasMesh && hasCommandList == false ) {
				mesh.DestroyEndOfFrameEmulated(); // Object.Destroy( mesh );
				activeMeshCount--;
				hasMesh = false;
			}
		}

		protected void ClearMesh() {
			if( hasMesh )
				mesh.Clear();
		}

		protected virtual bool ExternallyDirty() => false;
		protected virtual void UpdateMesh() => _ = 0;

		protected bool EnsureMeshIsReadyToRender( out Mesh outMesh, Action updateMesh ) {
			if( hasData == false ) { // no mesh exists because no data was added
				outMesh = null;
				return false;
			}

			if( hasMesh == false ) { // has data, but this is the first time we need a mesh
				EnsureMeshExists(); // create it
				updateMesh(); // update mesh
				meshDirty = false;
			} else if( meshDirty ) { // already has a mesh, but it's outdated
				updateMesh();
				meshDirty = false;
			}

			outMesh = mesh;
			return hasMesh;
		}

	}

}