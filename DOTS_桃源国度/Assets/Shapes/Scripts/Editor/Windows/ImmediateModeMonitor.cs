using System;
using System.Linq;
using Shapes;
using UnityEditor;
using UnityEngine;

public class ImmediateModeMonitor : EditorWindow {

	[SerializeField] Vector2 scrollPos;

	void OnGUI() {
		void CountSuffixLabel( int count, string str ) {
			string suffix = Math.Abs( count ) == 1 ? "" : "s";
			GUILayout.Label( string.Format( str, count, suffix ) );
		}

		GUILayout.Label( "• The purpose of this window is primarily to check for leaks - if you see any of these amounts increase indefinitely, something is likely wrong\n\n" +
						 "• Material Cache contains a complete list of cached materials used by immediate mode drawing\n\n" +
						 "• The number of polygon/polyline path assets may show too high or too low numbers in case these were destroyed or leaked at weird Unity events. " +
						 "This number naively increments when you create polygon/polyline paths, and decrements when they are disposed\n\n" +
						 "• Active Command Buffers may say 0 even though commands were rendered this frame. This is because commands can be added and removed before the UI updates\n\n" +
						 "• Individual command buffers have an ID at the end of their name, this number increases by 1 after every draw. " +
						 "Seeing this number rapidly increase is normal, as long as you don't see additional command buffers being added every frame\n\n" +
						 "• If a command buffer is added every single frame, it means you are likely adding draw commands without rendering/consuming them. " +
						 "A common pitfall is if you issue draw commands in Update(), but the camera isn't currently rendering, which can easily happen if you press play in the editor, but hide the game view. " +
						 $"This will stack commands. Generally you'll want to issue draw commands on the {UnityInfo.ON_PRE_RENDER_NAME} event\n\n" +
						 "• Registered cameras without buffers is generally not an issue", EditorStyles.helpBox );

		using( ShapesUI.Group ) {
			CountSuffixLabel( IMMaterialPool.pool.Count, "{0} material{1} in the material cache" );
			CountSuffixLabel( ShapesTextPool.InstanceElementCount, $"{{0}} text element{{1}} in the text cache. Usage: {ShapesTextPool.InstanceElementCountActive}/{ShapesTextPool.InstanceElementCount}" );
			CountSuffixLabel( DisposableMesh.ActiveMeshCount, "{0} polygon/polyline path asset{1}" );
			CountSuffixLabel( DrawCommand.cBuffersRendering.Count, "{0} camera{1} registered" );
			CountSuffixLabel( DrawCommand.cBuffersRendering.Values.Sum( list => list.Count ), "{0} active command buffer{1}" );
		}

		GUILayout.Space( 4 );
		using( var scroll = new EditorGUILayout.ScrollViewScope( scrollPos, false, false ) ) {
			GUILayout.Label( "Active Command Buffers:" );
			if( DrawCommand.cBuffersRendering.Count == 0 )
				GUILayout.Label( "(none)", EditorStyles.miniLabel );

			foreach( var kvp in DrawCommand.cBuffersRendering ) {
				using( ShapesUI.Horizontal ) {
					EditorGUILayout.ObjectField( GUIContent.none, kvp.Key, typeof(Camera), allowSceneObjects: true, GUILayout.Width( 120 ) );
					GUILayout.Label( $"has {kvp.Value.Count} buffer{( kvp.Value.Count != 1 ? "s" : "" )}:" );
				}

				using( new EditorGUI.IndentLevelScope() ) {
					foreach( DrawCommand cmd in kvp.Value ) {
						EditorGUILayout.LabelField( "Shapes Cmd " + cmd.id );
					}
				}
			}

			GUILayout.Label( "Material Cache:" );
			if( IMMaterialPool.pool.Count == 0 )
				GUILayout.Label( "(none)", EditorStyles.miniLabel );
			using( new EditorGUI.IndentLevelScope() ) {
				foreach( var kvp in IMMaterialPool.pool ) {
					EditorGUILayout.ObjectField( kvp.Value, typeof(Material), allowSceneObjects: true );
				}
			}

			scrollPos = scroll.scrollPosition;
		}

		if( Event.current.type == EventType.MouseDown )
			GUI.FocusControl( "" );
	}

	void Update() => Repaint();


}