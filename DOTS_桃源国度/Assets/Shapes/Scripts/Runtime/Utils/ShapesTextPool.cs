using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteAlways] public class ShapesTextPool : MonoBehaviour {

		const int ALLOCATION_COUNT_WARNING = 500;
		const int ALLOCATION_COUNT_CAP = 1000;

		int ElementCount => elementsPassive.Count + elementsActive.Count;
		Stack<TextMeshPro> elementsPassive = new Stack<TextMeshPro>();
		Dictionary<int, TextMeshPro> elementsActive = new Dictionary<int, TextMeshPro>();
		public TextMeshPro ImmediateModeElement => GetElement( -1 );

		public static int InstanceElementCount => InstanceExists ? Instance.ElementCount : 0;
		public static int InstanceElementCountActive => InstanceExists ? Instance.elementsActive.Count : 0;
		public static bool InstanceExists => instance != null;
		static ShapesTextPool instance;
		public static ShapesTextPool Instance {
			get {
				if( instance == null ) {
					instance = Object.FindObjectOfType<ShapesTextPool>();
					if( instance == null )
						instance = ShapesTextPool.CreatePool();
				}

				return instance;
			}
		}

		static ShapesTextPool CreatePool() {
			GameObject holder = new GameObject( "Shapes Text Pool" );
			if( Application.isPlaying )
				DontDestroyOnLoad( holder ); // might be a lil gross, not sure
			ShapesTextPool text = holder.AddComponent<ShapesTextPool>();
			holder.hideFlags = HideFlags.HideAndDontSave;
			return text;
		}

		void ClearData() {
			// clear any residual children if things reload
			for( int i = transform.childCount - 1; i >= 0; i-- )
				transform.GetChild( i ).gameObject.DestroyBranched();
			elementsPassive.Clear();
			elementsActive.Clear();
		}

		void OnEnable() {
			ClearData();
			instance = this;
		}

		void OnDisable() {
			ClearData();
		}

		public TextMeshPro GetElement( int id ) {
			if( elementsActive.TryGetValue( id, out TextMeshPro tmp ) == false )
				tmp = AllocateElement( id );
			return tmp;
		}

		public TextMeshPro AllocateElement( int id ) {
			TextMeshPro elem = null;
			// try find non-null passive elements
			while( elem == null && elementsPassive.Count > 0 )
				elem = elementsPassive.Pop();

			// if no passive elment found, create it
			if( elem == null )
				elem = CreateElement( id );

			// assign it to the active list
			elementsActive.Add( id, elem );
			return elem;
		}

		public void ReleaseElement( int id ) {
			if( elementsActive.TryGetValue( id, out TextMeshPro tmp ) ) {
				elementsActive.Remove( id );
				elementsPassive.Push( tmp );
			} else {
				// Debug.LogError( $"Failed to remove text element [{id}] from text pool" );
			}
		}

		TextMeshPro CreateElement( int id ) {
			int totalCount = ElementCount;
			if( totalCount > ALLOCATION_COUNT_CAP ) {
				Debug.LogError( $"Text element allocation cap of {ALLOCATION_COUNT_CAP} reached. You are probably leaking and not properly disposing text elements" );
				return null;
			}

			if( totalCount > ALLOCATION_COUNT_WARNING )
				Debug.LogWarning( $"Allocating more than {ALLOCATION_COUNT_WARNING} text elements. You are probably leaking and not properly disposing text objects" );

			GameObject elem = new GameObject( id == -1 ? "Immediate Mode Text" : id.ToString() );
			elem.transform.SetParent( transform, false );
			elem.transform.localPosition = Vector3.zero;
			elem.hideFlags = HideFlags.HideAndDontSave;

			TextMeshPro tmp = elem.AddComponent<TextMeshPro>();
			tmp.enableWordWrapping = false;
			tmp.overflowMode = TextOverflowModes.Overflow;

			// mesh renderer should exist now due to TMP requiring the component
			tmp.GetComponent<MeshRenderer>().enabled = false;

			return tmp;
		}

	}

}