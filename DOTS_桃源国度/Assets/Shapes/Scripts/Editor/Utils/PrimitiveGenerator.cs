using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class PrimitiveGenerator {

		public static void Generate3DPrimitiveAssets() {
			ShapesConfig config = ShapesConfig.Instance;

			// delete all
			// ShapesAssets assets = ShapesAssets.Instance;
			// string assetPath = AssetDatabase.GetAssetPath( assets );
			// Mesh[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath( assetPath ).OfType<Mesh>().ToArray();
			// for( int i = 0; i < assetsAtPath.Length; i++ ) {
			// 	GameObject.DestroyImmediate( assetsAtPath[i], true );
			// }
			// return;

			EditorUtility.SetDirty( ShapesAssets.Instance );
			UpdatePrimitiveMesh( "quad", 0, config.boundsSizeQuad, Quad, ref ShapesAssets.Instance.meshQuad, 1 );
			UpdatePrimitiveMesh( "triangle", 0, config.boundsSizeTriangle, Triangle, ref ShapesAssets.Instance.meshTriangle, 1 );
			UpdatePrimitiveMesh( "cube", 0, config.boundsSizeCuboid, Cube, ref ShapesAssets.Instance.meshCube, 1 );
			for( int d = 0; d < 5; d++ ) {
				UpdatePrimitiveMesh( "sphere", d, config.boundsSizeSphere, GenerateIcosphere( config.sphereDetail[d] ), ref ShapesAssets.Instance.meshSphere );
				UpdatePrimitiveMesh( "capsule", d, config.boundsSizeCapsule, GenerateCapsule( config.capsuleDivs[d] ), ref ShapesAssets.Instance.meshCapsule );
				UpdatePrimitiveMesh( "cylinder", d, config.boundsSizeCylinder, GenerateCylinder( config.cylinderDivs[d] ), ref ShapesAssets.Instance.meshCylinder );
				UpdatePrimitiveMesh( "cone", d, config.boundsSizeCone, GenerateCone( config.coneDivs[d], true ), ref ShapesAssets.Instance.meshCone );
				UpdatePrimitiveMesh( "cone_uncapped", d, config.boundsSizeCone, GenerateCone( config.coneDivs[d], true ), ref ShapesAssets.Instance.meshConeUncapped );
				UpdatePrimitiveMesh( "torus", d, config.boundsSizeTorus, GenerateTorus( config.torusDivsMinorMajor[d].x, config.torusDivsMinorMajor[d].y, 1f, 2f ), ref ShapesAssets.Instance.meshTorus );
			}

			AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath( ShapesAssets.Instance ) );
			Debug.Log( "Shapes primitives regenerated" );
		}

		static Mesh UpdatePrimitiveMesh( string primitive, int detail, float boundsSize, BasicMeshData meshData, ref Mesh[] meshArray, int detailLevelCount = 5 ) {
			// make sure array is prepared
			if( meshArray == null || meshArray.Length != detailLevelCount ) {
				// array incorrectly set up
				meshArray = new Mesh[detailLevelCount];
				Debug.Log( $"reinitialized {primitive} mesh array to {detailLevelCount}" );
			}

			// field is missing a ref
			if( meshArray[detail] == null ) {
				string meshName = $"{primitive}_{detail}";
				// find existing mesh
				string assetPath = AssetDatabase.GetAssetPath( ShapesAssets.Instance );
				Mesh[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath( assetPath ).OfType<Mesh>().ToArray();
				Mesh existingMesh = assetsAtPath.FirstOrDefault( x => x.name == meshName );
				if( existingMesh != null ) {
					// assign existing mesh
					Debug.Log( "Assigning missing mesh ref " + meshName );
					meshArray[detail] = existingMesh;
				} else {
					// create it if it's not found
					Debug.Log( "Creating missing mesh " + meshName );
					meshArray[detail] = new Mesh { name = meshName };
					AssetDatabase.AddObjectToAsset( meshArray[detail], ShapesAssets.Instance );
				}
			}

			Mesh m = meshArray[detail];
			meshData.ApplyTo( m );
			m.bounds = new Bounds( Vector3.zero, Vector3.one * boundsSize );
			meshArray[detail] = m;
			return m;
		}

		// Icosahedron base topology, vertex radius 1
		public static readonly BasicMeshData Icosahedron = new BasicMeshData {
			tris = new List<int> { 0, 1, 2, 2, 6, 0, 0, 6, 5, 5, 7, 0, 0, 7, 1, 1, 7, 3, 3, 8, 1, 1, 8, 2, 2, 8, 4, 2, 4, 6, 6, 4, 10, 6, 10, 5, 5, 10, 11, 5, 11, 7, 7, 11, 3, 9, 10, 4, 4, 8, 9, 9, 8, 3, 3, 11, 9, 9, 11, 10 },
			verts = new List<Vector3> { new Vector3( 0, -0.5257311f, -0.8506508f ), new Vector3( -0.5257311f, -0.8506508f, 0 ), new Vector3( -0.8506508f, 0, -0.5257311f ), new Vector3( 0, -0.5257311f, 0.8506508f ), new Vector3( -0.5257311f, 0.8506508f, 0 ), new Vector3( 0.8506508f, 0, -0.5257311f ), new Vector3( 0, 0.5257311f, -0.8506508f ), new Vector3( 0.5257311f, -0.8506508f, 0 ), new Vector3( -0.8506508f, 0, 0.5257311f ), new Vector3( 0, 0.5257311f, 0.8506508f ), new Vector3( 0.5257311f, 0.8506508f, 0 ), new Vector3( 0.8506508f, 0, 0.5257311f ) }
		};

		public static readonly BasicMeshData Triangle = new BasicMeshData {
			tris = new List<int> { 0, 1, 2 },
			verts = new List<Vector3> { new Vector3( 1, 0, 0 ), new Vector3( 0, 1, 0 ), new Vector3( 0, 0, 1 ) },
			normals = new List<Vector3> { Vector3.one.normalized, Vector3.one.normalized, Vector3.one.normalized } // normals mostly required to suppress warnings
		};

		public static readonly BasicMeshData Quad = new BasicMeshData {
			verts = new List<Vector3> { new Vector3( -1, -1 ), new Vector3( -1, 1 ), new Vector3( 1, 1 ), new Vector3( 1, -1 ) },
			normals = new List<Vector3> { new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ) },
			uvs = new List<Vector2> { new Vector2( -1, -1 ), new Vector2( -1, 1 ), new Vector2( 1, 1 ), new Vector2( 1, -1 ) },
			colors = new List<Color> { new Color( 1, 0, 0, 0 ), new Color( 0, 1, 0, 0 ), new Color( 0, 0, 1, 0 ), new Color( 0, 0, 0, 1 ) },
			tris = new List<int> { 0, 1, 2, 0, 2, 3 }
		};


		const float INV_SQRT3 = 0.577350269189f;

		public static readonly BasicMeshData Cube = new BasicMeshData {
			verts = new List<Vector3> { new Vector3( -1, -1, -1 ), new Vector3( -1, -1, 1 ), new Vector3( -1, 1, -1 ), new Vector3( -1, 1, 1 ), new Vector3( 1, -1, -1 ), new Vector3( 1, -1, 1 ), new Vector3( 1, 1, -1 ), new Vector3( 1, 1, 1 ) },
			tris = new List<int> { 0, 1, 2, 2, 1, 3, 3, 1, 7, 7, 1, 5, 5, 4, 7, 7, 4, 6, 6, 4, 2, 2, 4, 0, 0, 4, 5, 5, 1, 0, 7, 6, 2, 2, 3, 7 },
			normals = new List<Vector3> { new Vector3( -INV_SQRT3, -INV_SQRT3, -INV_SQRT3 ), new Vector3( -INV_SQRT3, -INV_SQRT3, INV_SQRT3 ), new Vector3( -INV_SQRT3, INV_SQRT3, -INV_SQRT3 ), new Vector3( -INV_SQRT3, INV_SQRT3, INV_SQRT3 ), new Vector3( INV_SQRT3, -INV_SQRT3, -INV_SQRT3 ), new Vector3( INV_SQRT3, -INV_SQRT3, INV_SQRT3 ), new Vector3( INV_SQRT3, INV_SQRT3, -INV_SQRT3 ), new Vector3( INV_SQRT3, INV_SQRT3, INV_SQRT3 ) }
		};

		public static int TriangleCountCapsule( int n ) => 8 * ( n + n * n );

		public static BasicMeshData GenerateCapsule( int divs ) {
			BasicMeshData mesh = new BasicMeshData();
			mesh.normals = new List<Vector3>();

			int sides = divs * 4;

			for( int z = 0; z < 2; z++ ) {
				for( int i = 0; i < sides; i++ ) {
					float t = i / (float)sides;
					Vector3 v = ShapesMath.AngToDir( t * ShapesMath.TAU );
					mesh.normals.Add( v );
					v.z = z;
					mesh.verts.Add( v );
				}
			}

			// sides
			for( int i = 0; i < sides; i++ ) {
				int low0 = i;
				int top0 = sides + i;
				int low1 = ( i + 1 ) % sides;
				int top1 = sides + ( i + 1 ) % sides;
				mesh.tris.Add( low0 );
				mesh.tris.Add( low1 );
				mesh.tris.Add( top1 );
				mesh.tris.Add( top1 );
				mesh.tris.Add( top0 );
				mesh.tris.Add( low0 );
			}

			// round caps!
			int n = divs + 1;
			Vector3[] octaBaseVerts = { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
			for( int z = 0; z < 2; z++ ) {
				// half-octahedron
				for( int s = 0; s < 4; s++ ) {
					Vector3 v0 = z == 0 ? Vector3.back : Vector3.forward; // reverse depending on z
					Vector3 v1 = octaBaseVerts[s];
					Vector3 v2 = octaBaseVerts[( s + 1 ) % 4];

					Vector3[] verts = BarycentricVertices( n, v0, v1, v2 ).ToArray();
					mesh.normals.AddRange( verts );
					if( z == 0 ) {
						mesh.tris.AddRange( BarycentricTriangulation( n, mesh.verts.Count ).Reverse() );
						mesh.verts.AddRange( verts.Select( x => x ) );
					} else {
						mesh.tris.AddRange( BarycentricTriangulation( n, mesh.verts.Count ) );
						mesh.verts.AddRange( verts.Select( x => x + Vector3.forward ) );
					}
				}
			}

			mesh.RemoveDuplicateVertices();

			return mesh;
		}

		public static int TriangleCountCylinder( int divs ) => ( divs - 1 ) * 4;

		public static BasicMeshData GenerateCylinder( int divs ) {
			BasicMeshData mesh = new BasicMeshData();
			mesh.normals = new List<Vector3>();

			for( int z = 0; z < 2; z++ ) {
				for( int i = 0; i < divs; i++ ) {
					float t = i / (float)divs;
					Vector3 v = ShapesMath.AngToDir( t * ShapesMath.TAU );
					mesh.normals.Add( v );
					v.z = z;
					mesh.verts.Add( v );
				}
			}

			// sides
			for( int i = 0; i < divs; i++ ) {
				int low0 = i;
				int top0 = divs + i;
				int low1 = ( i + 1 ) % divs;
				int top1 = divs + ( i + 1 ) % divs;
				mesh.tris.Add( low0 );
				mesh.tris.Add( low1 );
				mesh.tris.Add( top1 );
				mesh.tris.Add( top1 );
				mesh.tris.Add( top0 );
				mesh.tris.Add( low0 );
			}

			// cap bottom
			for( int i = 1; i < divs - 1; i++ ) {
				mesh.tris.Add( 0 );
				mesh.tris.Add( ( i + 1 ) % divs );
				mesh.tris.Add( i );
			}

			// cap top
			for( int i = 1; i < divs - 1; i++ ) {
				mesh.tris.Add( divs + 0 );
				mesh.tris.Add( divs + i );
				mesh.tris.Add( divs + ( i + 1 ) % divs );
			}

			return mesh;
		}

		public static int TriangleCountCone( int divs ) => ( divs - 1 ) * 2;

		public static BasicMeshData GenerateCone( int divs, bool generateCap ) {
			BasicMeshData mesh = new BasicMeshData();

			mesh.verts.Add( Vector3.forward );
			for( int i = 1; i < divs + 1; i++ ) {
				float t = i / (float)divs;
				int iNext = i == divs ? 1 : i + 1;
				mesh.verts.Add( ShapesMath.AngToDir( t * ShapesMath.TAU ) );
				mesh.tris.Add( 0 ); // vertex 0 is the tip
				mesh.tris.Add( i );
				mesh.tris.Add( iNext );

				if( generateCap && i > 1 && i < divs ) {
					mesh.tris.Add( 1 ); // vertex 1 is the root edge vert
					mesh.tris.Add( iNext );
					mesh.tris.Add( i );
				}
			}

			mesh.normals = mesh.verts.Select( v => v ).ToList(); // already normalized

			return mesh;
		}

		public static int TriangleCountIcosphere( int divs ) => divs * divs * 20;

		public static BasicMeshData GenerateIcosphere( int divs ) {
			BasicMeshData mesh = new BasicMeshData();
			mesh.normals = new List<Vector3>();

			// foreach face, generate all vertices and triangles
			for( int i = 0; i < 20; i++ ) {
				Vector3 v0 = Icosahedron.verts[Icosahedron.tris[i * 3]];
				Vector3 v1 = Icosahedron.verts[Icosahedron.tris[i * 3 + 1]];
				Vector3 v2 = Icosahedron.verts[Icosahedron.tris[i * 3 + 2]];
				// add this icosa face to the global list
				int n = divs + 1; // n is number of verts along one side of the triangle 
				int prevVertCount = mesh.verts.Count;
				Vector3[] verts = BarycentricVertices( n, v0, v1, v2 ).ToArray();
				mesh.verts.AddRange( verts );
				mesh.normals.AddRange( verts );
				mesh.tris.AddRange( BarycentricTriangulation( n, globalOffset: prevVertCount ) );
			}

			// cleanup duplicate verts
			mesh.RemoveDuplicateVertices();

			return mesh;
		}


		static IEnumerable<Vector3> BarycentricVertices( int n, Vector3 v0, Vector3 v1, Vector3 v2 ) {
			for( int iy = 0; iy < n; iy++ ) {
				float ty = iy / ( n - 1f );
				for( int ix = 0; ix < n - iy; ix++ ) {
					float tx = iy == n - 1 ? 0f : ix / ( n - iy - 1f );
					Vector3 t = new Vector3( ( 1f - ty ) * ( 1f - tx ), ty, ( 1f - ty ) * tx ); // equivalent to the triple lerp method
					yield return SphericalBarycentricInterpolationEquilateral( v0, v1, v2, t );
				}
			}
		}

		static IEnumerable<int> BarycentricTriangulation( int n, int globalOffset ) {
			int rootIndex = 0; // first index on each row
			for( int iy = 0; iy < n; iy++ ) {
				int rootNext = rootIndex + n - iy;
				int xDotCount = n - iy;
				for( int ix = 0; ix < xDotCount - 1; ix++ ) { // foreach dot in a row (but one less)
					yield return globalOffset + ix + rootIndex + 1;
					yield return globalOffset + ix + rootIndex;
					yield return globalOffset + ix + rootNext;
					if( ix < xDotCount - 2 ) {
						yield return globalOffset + ix + rootNext;
						yield return globalOffset + ix + rootNext + 1;
						yield return globalOffset + ix + rootIndex + 1;
					}
				}

				rootIndex = rootNext;
			}
		}

		public static BasicMeshData GenerateUVSphere( int divsLong, int divsLat ) {
			BasicMeshData mesh = new BasicMeshData();
			mesh.normals = new List<Vector3>();

			int vertCount = divsLong * divsLat;
			int triCount = divsLong * ( divsLat - 1 ) * 2 - divsLong * 2; // subtracting is to remove quads at the pole
			Vector3[] verts = new Vector3[vertCount];
			int iVert = 0;

			// generate verts
			for( int iLo = 0; iLo < divsLong; iLo++ ) {
				float tLong = iLo / (float)divsLong;
				float angLong = tLong * ShapesMath.TAU;
				Vector2 dirXZ = ShapesMath.AngToDir( angLong );
				Vector3 dirLong = new Vector3( dirXZ.x, 0f, dirXZ.y );
				for( int iLa = 0; iLa < divsLat; iLa++ ) {
					float tLat = iLa / ( divsLat - 1f );
					float angLat = Mathf.Lerp( -0.25f, 0.25f, tLat ) * ShapesMath.TAU;
					Vector2 dirProj = ShapesMath.AngToDir( angLat );
					verts[iVert++] = dirLong * dirProj.x + Vector3.up * dirProj.y;
				}
			}

			// generate tris
			int[] tris = new int[triCount * 3];
			int iTri = 0;
			for( int iLo = 0; iLo < divsLong; iLo++ ) {
				for( int iLa = 0; iLa < divsLat - 1; iLa++ ) {
					int iRoot = iLo * divsLat + iLa;
					int iRootNext = ( iRoot + divsLat ) % vertCount;
					if( iLa < divsLat - 2 ) { // skip first and last (triangles at the poles)
						tris[iTri++] = iRoot;
						tris[iTri++] = iRoot + 1;
						tris[iTri++] = iRootNext + 1;
					}

					if( iLa > 0 ) {
						tris[iTri++] = iRootNext + 1;
						tris[iTri++] = iRootNext;
						tris[iTri++] = iRoot;
					}
				}
			}

			mesh.verts.AddRange( verts );
			mesh.tris.AddRange( tris );
			mesh.normals.AddRange( verts );
			mesh.RemoveDuplicateVertices();

			return mesh;
		}

		public static int TriangleCountTorus( Vector2Int divsMinMaj ) => divsMinMaj.x * divsMinMaj.y * 2;

		public static BasicMeshData GenerateTorus( int divsMinor, int divsMajor, float rMinor = 1, float rMajor = 1 ) {
			BasicMeshData mesh = new BasicMeshData();
			mesh.normals = new List<Vector3>();
			for( int iMaj = 0; iMaj < divsMajor; iMaj++ ) {
				float tMaj = iMaj / (float)divsMajor;
				Vector2 dirMaj = ShapesMath.AngToDir( tMaj * ShapesMath.TAU );
				for( int iMin = 0; iMin < divsMinor; iMin++ ) {
					float tMin = iMin / (float)divsMinor;
					Vector2 dirMinLocal = ShapesMath.AngToDir( tMin * ShapesMath.TAU );
					Vector3 dirMin = (Vector3)dirMaj * dirMinLocal.x + new Vector3( 0, 0, dirMinLocal.y );
					mesh.normals.Add( dirMin );
					mesh.verts.Add( (Vector3)dirMaj * rMajor + dirMin * rMinor );
					int maj0min0 = iMaj * divsMinor + iMin;
					int maj1min0 = ( iMaj + 1 ) % divsMajor * divsMinor + iMin;
					int maj0min1 = iMaj * divsMinor + ( iMin + 1 ) % divsMinor;
					int maj1min1 = ( iMaj + 1 ) % divsMajor * divsMinor + ( iMin + 1 ) % divsMinor;
					mesh.tris.Add( maj0min1 );
					mesh.tris.Add( maj0min0 );
					mesh.tris.Add( maj1min1 );
					mesh.tris.Add( maj1min0 );
					mesh.tris.Add( maj1min1 );
					mesh.tris.Add( maj0min0 );
				}
			}

			return mesh;
		}

		public class BasicMeshData {
			public List<Vector3> verts = new List<Vector3>();
			public List<int> tris = new List<int>();
			public List<Vector3> normals = null; // null = unused
			public List<Vector2> uvs = null; // null = unused
			public List<Color> colors = null; // null = unused
			public (Vector3, Vector3, Vector3) GetTriVerts( int triangle ) => ( Icosahedron.verts[Icosahedron.tris[triangle * 3]], Icosahedron.verts[Icosahedron.tris[triangle * 3 + 1]], Icosahedron.verts[Icosahedron.tris[triangle * 3 + 2]] );

			public void ApplyTo( Mesh mesh ) {
				mesh.Clear();
				mesh.SetVertices( verts );
				if( normals != null ) mesh.SetNormals( normals );
				if( uvs != null ) mesh.SetUVs( 0, uvs );
				if( colors != null ) mesh.SetColors( colors );
				mesh.SetTriangles( tris, 0 );
			}

			public void RemoveDuplicateVertices() {
				// find which vertices are similar to a previous one, and create a mapping to existing vertices
				Dictionary<int, int> fromToMap = new Dictionary<int, int>();
				for( int i = 0; i < verts.Count; i++ ) {
					if( fromToMap.ContainsKey( i ) )
						continue; // skip removed vertices
					for( int j = i + 1; j < verts.Count; j++ ) {
						if( Vector3.Distance( verts[i], verts[j] ) < 0.0001f ) { // 10th of a millimeter
							fromToMap[j] = i; // map new vertex to old similar vertex
						}
					}
				}

				// make all triangle indices point to the old ones
				for( int i = 0; i < tris.Count; i++ ) {
					if( fromToMap.TryGetValue( tris[i], out int existingVertexId ) )
						tris[i] = existingVertexId;
				}

				// remove unused verts
				var unusedVerts = fromToMap.Keys.OrderByDescending( x => x );
				foreach( int removeIndex in unusedVerts ) {
					verts.RemoveAt( removeIndex );
					uvs?.RemoveAt( removeIndex );
					normals?.RemoveAt( removeIndex );
					colors?.RemoveAt( removeIndex );
					for( int tri = 0; tri < tris.Count; tri++ ) {
						if( tris[tri] > removeIndex )
							tris[tri]--; // decrease by one
						else if( tris[tri] == removeIndex )
							Debug.LogWarning( "triangle pointing to deleted vertex :(" );
					}
				}

				// Debug.Log( "removed " + fromToMap.Keys.Count() + " verts" );
			}
		}


		static float AngBetweenNormalizedVectors( Vector3 a, Vector3 b ) => Mathf.Acos( Mathf.Clamp( Vector3.Dot( a, b ), -1f, 1f ) );

		static (float a, float b, float c) GetUnitSphereTriangleEdgeLengths( Vector3 a, Vector3 b, Vector3 c ) =>
		(
			AngBetweenNormalizedVectors( b, c ),
			AngBetweenNormalizedVectors( c, a ),
			AngBetweenNormalizedVectors( a, b )
		);


		static float GetUnitSphereTriangleArea( Vector3 A, Vector3 B, Vector3 C ) {
			( float a, float b, float c ) = GetUnitSphereTriangleEdgeLengths( A, B, C ); // equivalent to arc lengths
			float s = ( a + b + c ) / 2;
			return 4 * Mathf.Atan( Mathf.Sqrt( Mathf.Tan( s / 2 ) * Mathf.Tan( ( s - a ) / 2 ) * Mathf.Tan( ( s - b ) / 2 ) * Mathf.Tan( ( s - c ) / 2 ) ) );
		}


		// https://math.stackexchange.com/questions/1151428/point-within-a-spherical-triangle-given-areas
		public static Vector3 SphericalBarycentricInterpolationEquilateral( Vector3 a, Vector3 b, Vector3 c, Vector3 coord ) {
			// Presumes equilateral triangle
			float Ω = GetUnitSphereTriangleArea( a, b, c );
			Vector3 GetV3( Func<int, float> noot ) => new Vector3( noot( 0 ), noot( 1 ), noot( 2 ) );
			Vector3 t = coord; // area proportion
			float θ = AngBetweenNormalizedVectors( a, b ); // length/angle of one side
			float β = 2 * Mathf.Cos( θ ) / ( 1f + Mathf.Cos( θ ) );
			Vector3 λ = GetV3( i => Mathf.Tan( t[i] * Ω / 2 ) / Mathf.Tan( Ω / 2 ) );
			Vector3 v = GetV3( i => λ[i] / ( 1f + β + ( 1f - β ) * λ[i] ) );
			return ( v[0] * a + v[1] * b + v[2] * c ) / ( 1f - v[0] - v[1] - v[2] );
		}


		// https://math.stackexchange.com/questions/1151428/point-within-a-spherical-triangle-given-areas
		public static Vector3 SphericalBarycentricInterpolation( Vector3 a, Vector3 b, Vector3 c, Vector3 coord ) {
			float Ω = GetUnitSphereTriangleArea( a, b, c );
			Vector3 GetV3( Func<int, float> f ) => new Vector3( f( 0 ), f( 1 ), f( 2 ) );
			Vector3[] A = { a, b, c }; // points on the sphere
			Vector3 t = coord; // area proportion
			Vector3 α = GetV3( i => Vector3.Dot( A[( i + 1 ) % 3], A[( i + 2 ) % 3] ) );
			Vector3 β = GetV3( i => ( α[( i + 1 ) % 3] + α[( i + 2 ) % 3] ) / ( 1 + α[i] ) );
			Vector3 λ = GetV3( i => Mathf.Tan( t[i] * Ω / 2 ) / Mathf.Tan( Ω / 2 ) );
			Vector3 v = GetV3( i => λ[i] / ( 1f + β[i] + ( 1f - β[i] ) * λ[i] ) );
			Vector3 u = GetV3( i => v[i] / ( 1f - v[0] - v[1] - v[2] ) ); // vertex weights
			return u[0] * a + u[1] * b + u[2] * c;
		}


	}

}