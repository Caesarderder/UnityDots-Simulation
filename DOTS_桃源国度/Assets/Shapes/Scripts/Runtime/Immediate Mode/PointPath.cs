using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class PointPath<T> : DisposableMesh {
		
		protected List<T> path = new List<T>();
		public int Count => path.Count;
		public T LastPoint => path[path.Count - 1];

		protected void OnSetFirstDataPoint() {
			hasData = true;
			meshDirty = true;
		}
		
		public void ClearAllPoints() {
			path.Clear();
			hasData = false;
		}
		
		public T this[ int i ] {
			get => path[i];
			set {
				path[i] = value;
				meshDirty = true;
			}
		}

		public void SetPoint( int index, T point ) {
			path[index] = point;
			meshDirty = true;
		}
		
		public void AddPoint( T p ) {
			if( hasData == false )
				OnSetFirstDataPoint();
			path.Add( p );
			hasData = true;
			meshDirty = true;
		}
		
		public void AddPoints( params T[] pts ) => AddPoints( (IEnumerable<T>)pts );
		
		public void AddPoints( IEnumerable<T> ptsToAdd ) {
			int prevCount = path.Count;
			path.AddRange( ptsToAdd );
			int pathCount = path.Count;
			int addedPtCount = pathCount - prevCount;

			if( addedPtCount > 0 ) {
				if( hasData == false )
					OnSetFirstDataPoint();
				hasData = true;
				meshDirty = true;
			}
		}
		
		protected bool CheckCanAddContinuePoint( [CallerMemberName] string callerName = null ) {
			if( hasData == false ) {
				Debug.LogWarning( $"{callerName} requires adding a point before calling it, to determine starting point" );
				return true;
			}

			return false;
		}

	}

}