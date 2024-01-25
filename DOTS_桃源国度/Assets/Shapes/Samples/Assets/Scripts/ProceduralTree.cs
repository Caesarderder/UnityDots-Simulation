using System.Collections.Generic;
using UnityEngine;

namespace Shapes {

	// I recommend drawing in onPreRender, which ImmediateModeShapeDrawer will handle for you, so let's inherit from that
	[ExecuteAlways] public class ProceduralTree : ImmediateModeShapeDrawer {

		[Header( "Line Style" )] [Range( 0f, 0.1f )] public float lineThickness = 0.1f;
		public Color lineColor = Color.white;

		[Header( "Tree shape" )] public int seed = 0;
		[Range( 1, 2000 )] public int lineCount = 100;
		[Range( 0, 4 )] public int branchesMin = 1;
		[Range( 1, 5 )] public int branchesMax = 5;
		[Range( 0, 1 )] public float branchLengthMin = 0.25f;
		[Range( 0, 1 )] public float branchLengthMax = 1;
		[Range( 0, ShapesMath.TAU / 2 )] public float maxAngDeviation = ShapesMath.TAU / 6;
		public bool use3D = false;


		// This function is called by ImmediateModeShapeDrawer in the appropriate OnPreRender call of each camera
		public override void DrawShapes( Camera cam ) {
			// Draw.Command enqueues a set of draw commands to render in the given camera
			using( Draw.Command( cam ) ) { // all immediate mode drawing should happen within these using-statements

				// Set up draw state
				Draw.ResetAllDrawStates(); // ensure everything is set to their defaults
				Draw.BlendMode = ShapesBlendMode.Additive;
				Draw.Thickness = lineThickness;
				Draw.LineGeometry = use3D ? LineGeometry.Volumetric3D : LineGeometry.Flat2D;
				Draw.ThicknessSpace = ThicknessSpace.Meters;
				Draw.Color = lineColor;

				Random.InitState( seed ); // initialize random state, so we get the same values every frame
				currentLineCount = 0;

				// Draw a branch at the current location. this function is recursive, so all other branches will follow
				BranchFrom( Draw.Matrix );
			}
		}

		// States to track draw limit and draw order
		int currentLineCount = 0;
		readonly Queue<Matrix4x4> mtxQueue = new Queue<Matrix4x4>(); // queue of pending branches to draw

		// Draws a line, moves forward, and then queues up new postions to draw from per new branch
		void BranchFrom( Matrix4x4 mtx ) {
			if( currentLineCount++ >= lineCount )
				return; // stop recursion if we hit our limit
			Draw.Matrix = mtx;
			float lineLength = Mathf.Lerp( branchLengthMin, branchLengthMax, Random.value ); // random branch length
			Vector3 offset = new Vector3( 0, lineLength, 0 ); // offset along the local Y axis
			Draw.Line( Vector3.zero, offset );
			Draw.Translate( offset ); // moves the drawing matrix in its local space

			// create a random number of branches from the current position
			int branchCount = Random.Range( branchesMin, branchesMax + 1 );
			for( int i = 0; i < branchCount; i++ ) {
				using( Draw.MatrixScope ) { // saves the current matrix state, and restores it at the end of this scope
					float angDeviation = Mathf.Lerp( -maxAngDeviation, maxAngDeviation, ShapesMath.RandomGaussian() ); // random angular deviation
					if( use3D )
						Draw.Rotate( angDeviation, ShapesMath.GetRandomPerpendicularVector( Vector3.up ) ); // rotates the current drawing matrix on a random axis
					else
						Draw.Rotate( angDeviation ); // rotates the current drawing matrix on the Z axis
					mtxQueue.Enqueue( Draw.Matrix ); // save the drawing matrix to draw a branch with later
				}
			}

			while( mtxQueue.Count > 0 ) // process all positions in the queue
				BranchFrom( mtxQueue.Dequeue() ); // draw new branches at the positions saved in the queue
		}

	}

}