using System;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static partial class Draw {

		static Matrix4x4 matrix = Matrix4x4.identity;

		/// <summary>The current immediate-mode drawing matrix. Setting this to transform.localToWorldMatrix will make following draw calls be relative to that object</summary>
		public static Matrix4x4 Matrix {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => matrix;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => matrix = value;
		}

		/// <summary>Resets the matrix to Matrix4x4.identity</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ResetMatrix() => matrix = Matrix4x4.identity;

		/// <summary>using( MatrixScope ){ /*code*/ } lets you modify the matrix within that scope, automatically restoring the previous state once you leave the scope</summary>
		public static MatrixStack MatrixScope => new MatrixStack( Draw.Matrix );

		/// <summary>Pushes the current drawing matrix onto the matrix stack. Calling <see cref="Draw.PopMatrix()"/> will restore this state</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void PushMatrix() => MatrixStack.Push( Draw.Matrix );

		/// <summary>Restores the drawing matrix to the previously pushed matrix from the stack</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void PopMatrix() => MatrixStack.Pop();

		/// <summary>Multiplies the drawing matrix by the input matrix. Equivalent to <see cref="Draw.Matrix"/> *= matrix;</summary>
		/// <param name="matrix">The right hand side matrix to multiply by</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void ApplyMatrix( Matrix4x4 matrix ) => Matrix *= matrix;


		#region Get/Set Components

		/// <summary>Get or set the origin of the drawing matrix in world space</summary>
		public static Vector3 Position {
			get => new Vector3( matrix.m03, matrix.m13, matrix.m23 );
			set {
				matrix.m03 = value.x;
				matrix.m13 = value.y;
				matrix.m23 = value.z;
			}
		}
		/// <summary>Get or set the origin of the drawing matrix X and Y coordinates in world space</summary>
		public static Vector2 Position2D {
			get => new Vector2( matrix.m03, matrix.m13 );
			set {
				matrix.m03 = value.x;
				matrix.m13 = value.y;
			}
		}

		[Obsolete( "Please use Draw.Position instead (I done messed up, did a typo, I'm sorry~)", true )]
		public static Vector3 Postition {
			get => Position;
			set => Position = value;
		}

		[Obsolete( "Please use Draw.Position2D instead (I done messed up, did a typo, I'm sorry~)", true )]
		public static Vector2 Postition2D {
			get => Position2D;
			set => Position2D = value;
		}

		/// <summary>Attempts to get or directly set the rotation of the drawing matrix in world space. 
		/// Note that, while rare, non-orthogonal matrices cannot be represented with a quaternion rotation, so the getter might not behave as you expect it to.
		/// Setting this rotation frequently may be expensive as it will also have to calculate scale in order to retain its per-axis local scale</summary>
		public static Quaternion Rotation {
			get => matrix.rotation;
			set => MtxSetRotationKeepScale( ref matrix, value );
		}

		/// <summary>Gets or sets the 2D angle (in radians) that the X axis is pointing in on the world space 2D XY plane. Note that this may have undesired effects if the drawing matrix is not aligned with the Z plane</summary>
		public static float Angle2D {
			get => ShapesMath.DirToAng( RightBasis );
			set => MtxRotateZLhs( ref matrix, value - Angle2D ); // R * M
		}

		/// <summary>The world space X axis direction of the current drawing matrix (normalized). If you don't need this normalized, or if you know you haven't scaled your matrix, then RightBasis is computationally less expensive</summary>
		public static Vector3 Right => RightBasis.normalized;

		/// <summary>The world space Y axis direction of the current drawing matrix (normalized). If you don't need this normalized, or if you know you haven't scaled your matrix, then UpBasis is computationally less expensive</summary>
		public static Vector3 Up => UpBasis.normalized;

		/// <summary>The world space Z axis direction of the current drawing matrix (normalized). If you don't need this normalized, or if you know you haven't scaled your matrix, then ForwardBasis is computationally less expensive</summary>
		public static Vector3 Forward => ForwardBasis.normalized;

		/// <summary>The world space X axis basis vector of the current drawing matrix. The magnitude of this vector is the local X scale</summary>		
		public static Vector3 RightBasis => matrix.GetColumn( 0 );

		/// <summary>The world space Y axis basis vector of the current drawing matrix. The magnitude of this vector is the local Y scale</summary>
		public static Vector3 UpBasis => matrix.GetColumn( 1 );

		/// <summary>The world space Z axis basis vector of the current drawing matrix. The magnitude of this vector is the local Z scale</summary>
		public static Vector3 ForwardBasis => matrix.GetColumn( 2 );

		/// <summary>Gets or sets the local per-axis scale of the current drawing matrix. This is relatively expensive as it needs to calculate the magnitude of each basis vector</summary>
		public static Vector3 LocalScale {
			get => new Vector3( RightBasis.magnitude, UpBasis.magnitude, ForwardBasis.magnitude );
			set {
				// v.normalized * scale = (v/||v||) * scale = v * scale/||v||
				float xScale = value.x / RightBasis.magnitude;
				float yScale = value.y / UpBasis.magnitude;
				float zScale = value.z / ForwardBasis.magnitude;
				Scale( xScale, yScale, zScale );
			}
		}

		#endregion

		#region Translate

		/// <summary>Translates (moves) the current drawing matrix by this amount along each of its axes</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate( float x, float y ) => MtxTranslateXY( ref matrix, x, y );

		/// <summary>Translates (moves) the current drawing matrix by this amount along each of its axes</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate( float x, float y, float z ) => MtxTranslateXYZ( ref matrix, x, y, z );

		/// <summary>Translates (moves) the current drawing matrix by this amount along each of its axes</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate( Vector2 displacement ) => Translate( displacement.x, displacement.y );

		/// <summary>Translates (moves) the current drawing matrix by this amount along each of its axes</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Translate( Vector3 displacement ) => Translate( displacement.x, displacement.y, displacement.z );

		#endregion

		#region Rotate

		/// <summary>Rotates the drawing matrix by this angle around its Z axis</summary>
		/// <param name="angle">Angle to rotate by, in radians</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Rotate( float angle ) => MtxRotateZ( ref matrix, angle );

		/// <summary>Rotates the drawing matrix by these angles around each axis, in radians</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Rotate( float x, float y, float z ) => Rotate( Quaternion.Euler( x * Mathf.Rad2Deg, y * Mathf.Rad2Deg, z * Mathf.Rad2Deg ) );

		/// <summary>Rotates the drawing matrix by this angle (in radians) around the given axis</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Rotate( float angle, Vector3 axis ) => Rotate( Quaternion.AngleAxis( angle * Mathf.Rad2Deg, axis ) );

		/// <summary>Rotates the drawing matrix by this quaternion</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Rotate( Quaternion rotation ) => matrix *= Matrix4x4.Rotate( rotation );

		#endregion

		#region Scale

		/// <summary>Scales the drawing matrix uniformly on each axis by this amount</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale( float uniformScale ) => MtxScaleXYZ( ref matrix, uniformScale, uniformScale, uniformScale );

		/// <summary>Scales the drawing matrix by this amount per axis</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale( float x, float y ) => MtxScaleXY( ref matrix, x, y );

		/// <summary>Scales the drawing matrix by this amount per axis</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale( float x, float y, float z ) => MtxScaleXYZ( ref matrix, x, y, z );

		/// <summary>Scales the drawing matrix by this amount per axis</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale( Vector2 scale ) => Scale( scale.x, scale.y );

		/// <summary>Scales the drawing matrix by this amount per axis</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void Scale( Vector3 scale ) => Scale( scale.x, scale.y, scale.z );

		#endregion

		#region SetMatrix

		/// <summary>Sets the drawing matrix. Equivalent to directly assigning to Draw.Matrix</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMatrix( Matrix4x4 matrix ) => Draw.Matrix = matrix;

		/// <summary>Sets the drawing matrix to this position, rotation and scale</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMatrix( Vector3 position, Quaternion rotation, Vector3 scale ) => Draw.Matrix = Matrix4x4.TRS( position, rotation, scale );

		/// <summary>Sets the drawing matrix to match this transform, effectively making you draw in the local space of this object</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMatrix( Transform transform ) => Draw.Matrix = transform.localToWorldMatrix;

		#endregion

		#region Internal Matrix Helpers

		// Internal matrix math! doing some of this is faster than a full matrix multiply,
		// so uh, I'm gonna chop things up a bit it's fine everything's fine
		//
		// xd	yd	zd	pos
		// m00	m01	m02	m03
		// m10	m11	m12 m13
		// m20	m21	m22 m23
		// m30	m31	m32 m33

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxSetRotationKeepScale( ref Matrix4x4 m, Quaternion rotation ) {
			Matrix4x4 rotMtx = Matrix4x4.Rotate( rotation );
			float scaleX = ( (Vector3)m.GetColumn( 0 ) ).magnitude;
			float scaleY = ( (Vector3)m.GetColumn( 1 ) ).magnitude;
			float scaleZ = ( (Vector3)m.GetColumn( 2 ) ).magnitude;
			m.m00 = rotMtx.m00 * scaleX;
			m.m10 = rotMtx.m10 * scaleX;
			m.m20 = rotMtx.m20 * scaleX;
			m.m01 = rotMtx.m01 * scaleY;
			m.m11 = rotMtx.m11 * scaleY;
			m.m21 = rotMtx.m21 * scaleY;
			m.m02 = rotMtx.m02 * scaleZ;
			m.m12 = rotMtx.m12 * scaleZ;
			m.m22 = rotMtx.m22 * scaleZ;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxRotateZLhs( ref Matrix4x4 rhs, float a ) {
			double x = Math.Cos( a );
			double y = Math.Sin( a );
			double rhs00 = rhs.m00;
			double rhs01 = rhs.m01;
			double rhs02 = rhs.m02;
			double rhs03 = rhs.m03;
			rhs.m00 = (float)( x * rhs00 - y * rhs.m10 );
			rhs.m01 = (float)( x * rhs01 - y * rhs.m11 );
			rhs.m02 = (float)( x * rhs02 - y * rhs.m12 );
			rhs.m03 = (float)( x * rhs03 - y * rhs.m13 );
			rhs.m10 = (float)( y * rhs00 + x * rhs.m10 );
			rhs.m11 = (float)( y * rhs01 + x * rhs.m11 );
			rhs.m12 = (float)( y * rhs02 + x * rhs.m12 );
			rhs.m13 = (float)( y * rhs03 + x * rhs.m13 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxTranslateXYZ( ref Matrix4x4 lhs, double x, double y, double z ) {
			lhs.m03 = (float)( lhs.m00 * x + lhs.m01 * y + lhs.m02 * z + lhs.m03 );
			lhs.m13 = (float)( lhs.m10 * x + lhs.m11 * y + lhs.m12 * z + lhs.m13 );
			lhs.m23 = (float)( lhs.m20 * x + lhs.m21 * y + lhs.m22 * z + lhs.m23 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxTranslateXY( ref Matrix4x4 lhs, double x, double y ) {
			lhs.m03 = (float)( lhs.m00 * x + lhs.m01 * y + lhs.m03 );
			lhs.m13 = (float)( lhs.m10 * x + lhs.m11 * y + lhs.m13 );
			lhs.m23 = (float)( lhs.m20 * x + lhs.m21 * y + lhs.m23 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxRotateZ( ref Matrix4x4 lhs, float a ) {
			double x = Math.Cos( a );
			double y = Math.Sin( a );

			float m00 = lhs.m00;
			float m01 = lhs.m01;
			float m10 = lhs.m10;
			float m11 = lhs.m11;
			float m20 = lhs.m20;
			float m21 = lhs.m21;

			lhs.m00 = (float)( m00 * x + m01 * y );
			lhs.m01 = (float)( m00 * -y + m01 * x );
			lhs.m10 = (float)( m10 * x + m11 * y );
			lhs.m11 = (float)( m10 * -y + m11 * x );
			lhs.m20 = (float)( m20 * x + m21 * y );
			lhs.m21 = (float)( m20 * -y + m21 * x );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxScaleXYZ( ref Matrix4x4 m, double x, double y, double z ) {
			m.m00 = (float)( m.m00 * x );
			m.m10 = (float)( m.m10 * x );
			m.m20 = (float)( m.m20 * x );
			m.m01 = (float)( m.m01 * y );
			m.m11 = (float)( m.m11 * y );
			m.m21 = (float)( m.m21 * y );
			m.m02 = (float)( m.m02 * z );
			m.m12 = (float)( m.m12 * z );
			m.m22 = (float)( m.m22 * z );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxScaleXY( ref Matrix4x4 m, double x, double y ) {
			m.m00 = (float)( m.m00 * x );
			m.m10 = (float)( m.m10 * x );
			m.m20 = (float)( m.m20 * x );
			m.m01 = (float)( m.m01 * y );
			m.m11 = (float)( m.m11 * y );
			m.m21 = (float)( m.m21 * y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetToXYZ( out Matrix4x4 m, float x, float y, float z ) {
			m = Matrix4x4.identity;
			m.m03 = x;
			m.m13 = y;
			m.m23 = z;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetToXY( out Matrix4x4 m, float x, float y ) {
			m = Matrix4x4.identity;
			m.m03 = x;
			m.m13 = y;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetToPosXYatAngle( out Matrix4x4 lhs, float x, float y, float a ) {
			MtxResetToXY( out lhs, x, y );
			MtxResetScaleSetAngleZ( ref lhs, a );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetToPosXYatDirection( out Matrix4x4 lhs, float x, float y, Vector2 dir ) {
			MtxResetToXY( out lhs, x, y );
			MtxResetScaleSetDirX( ref lhs, dir );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetScaleSetAngleZ( ref Matrix4x4 lhs, float a ) {
			float x = Mathf.Cos( a );
			float y = Mathf.Sin( a );
			lhs.m00 = x;
			lhs.m10 = y;
			lhs.m20 = 0;
			lhs.m01 = -y;
			lhs.m11 = x;
			lhs.m21 = 0;
			lhs.m02 = 0;
			lhs.m12 = 0;
			lhs.m22 = 1;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static void MtxResetScaleSetDirX( ref Matrix4x4 lhs, Vector2 dir ) {
			dir.Normalize();
			lhs.m00 = dir.x;
			lhs.m10 = dir.y;
			lhs.m20 = 0;
			lhs.m01 = -dir.y;
			lhs.m11 = dir.x;
			lhs.m21 = 0;
			lhs.m02 = 0;
			lhs.m12 = 0;
			lhs.m22 = 1;
		}

		#endregion

		// The functions below are currently disabled because there's an annoying ambiguity between
		// "Reset to the identity matrix and set it to position V" vs
		// "Set only the position to V but keep rotation and scale" and so forth.
		// I think these functions are mostly unnecessary as you can always reset the matrix and then
		// use the translate/rotate functions, and it'll be pretty much just as cheap anyway.
		// Anyhow, I'm keeping them below just in case I want them at some point in the future!

		#region SetPos

		/*
		/// <summary>Sets the drawing matrix to this world space position (without rotation, using a scale of 1)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetPosition( float x, float y ) => MtxResetToXY( out matrix, x, y );

		/// <summary>Sets the drawing matrix to this world space position (without rotation, using a scale of 1)</summary>
		public static void SetPosition( float x, float y, float z ) => MtxResetToXYZ( out matrix, x, y, z );

		/// <summary>Sets the drawing matrix to this world space position (without rotation, using a scale of 1)</summary>
		public static void SetPosition( Vector2 position ) => SetPosition( position.x, position.y );

		/// <summary>Sets the drawing matrix to this world space position (without rotation, using a scale of 1)</summary>
		public static void SetPosition( Vector3 position ) => SetPosition( position.x, position.y, position.z );
		*/

		#endregion

		#region SetPosRot

		/*
		/// <summary>Sets the drawing matrix to this world space position and rotation (using a scale of 1)</summary>
		/// <param name="angle">Angle in radians to set the Z axis rotation to</param>
		/// <param name="x">X position (in world space)</param>
		/// <param name="y">Y position (in world space)</param>
		public static void SetPositionAngle( float x, float y, float angle ) => MtxResetToPosXYatAngle( out matrix, x, y, angle );

		/// <summary>Sets the drawing matrix to this world space position and rotation (using a scale of 1)</summary>
		/// <param name="direction">The direction of the X axis</param>
		/// <param name="x">X position (in world space)</param>
		/// <param name="y">Y position (in world space)</param>
		public static void SetPositionDirection( float x, float y, Vector2 direction ) => MtxResetToPosXYatAngle( out matrix, x, y, ShapesMath.DirToAng( direction ) );

		/// <summary>Sets the drawing matrix to this world space position and rotation (using a scale of 1)</summary>
		/// <param name="x">X position (in world space)</param>
		/// <param name="y">Y position (in world space)</param>
		/// <param name="z">Z position (in world space)</param>
		/// <param name="rotation">Rotation (in world space)</param>
		public static void SetPositionRotation( float x, float y, float z, Quaternion rotation ) => SetPositionRotation( new Vector3( x, y, z ), rotation );

		/// <summary>Sets the drawing matrix to this world space position and rotation (using a scale of 1)</summary>
		/// <param name="position">Position (in world space)</param>
		/// <param name="rotation">Rotation (in world space)</param>
		public static void SetPositionRotation( Vector3 position, Quaternion rotation ) => Draw.Matrix = Matrix4x4.TRS( position, rotation, Vector3.one );

		/// <summary>Sets the drawing matrix to this world space position and rotation by angle-axis (using a scale of 1)</summary>
		/// <param name="position">Position (in world space)</param>
		/// <param name="angle">Angle of the rotation in radians</param>
		/// <param name="axis">Axis of the rotation (in world space)</param>
		public static void SetPositionRotation( Vector3 position, float angle, Vector3 axis ) => SetPositionRotation( position, Quaternion.AngleAxis( angle * Mathf.Rad2Deg, axis ) );
		*/

		#endregion


	}


}