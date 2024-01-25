using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class TargetArg {
		public string name;
		public string @default = null;
		public bool HasDefault => @default != null;

		public TargetArg( ParameterInfo param ) {
			name = param.Name;
			@default = ( Attribute.GetCustomAttribute( param, typeof(OvldDefault) ) as OvldDefault )?.@default;
		}

	}

	class TargetMethodCall {
		public string name;
		public TargetArg[] args;

		public TargetMethodCall( MethodInfo method ) {
			name = method.Name;
			args = method.GetParameters().Select( p => new TargetArg( p ) ).ToArray();
		}

	}

	class OverloadGenerator {
		public string overloadName;
		public string objectName;
		public string summary;
		public TargetMethodCall targetCall;
		public List<IParamSelector> paramSelectors = new List<IParamSelector>();
		public Dictionary<string, string> constAssigns = new Dictionary<string, string>();

		public OverloadGenerator( string overloadName, TargetMethodCall targetCall, string summary ) {
			this.summary = summary;
			this.overloadName = overloadName;
			this.objectName = overloadName; // default to this! though not always applicable
			this.targetCall = targetCall;
		}

		public static OverloadGenerator operator +( OverloadGenerator a, IParamSelector b ) {
			a.paramSelectors.Add( b );
			return a;
		}

		public static OverloadGenerator operator +( OverloadGenerator a, string s ) {
			a.paramSelectors.Add( (Param)s );
			return a;
		}

		public void GenerateAndAppend( List<string> lines ) => lines.AddRange( GenerateOverloads( paramSelectors.ToArray() ) );

		List<string> GenerateOverloads( params IParamSelector[] overloadParams ) {
			int totalVariants = overloadParams.Product( o => o.Variants ); // calc variant count
			Debug.Log( $"Gen: {totalVariants} {overloadName} variants" );

			List<string> overloads = new List<string>();
			RecurseParams( new int[overloadParams.Length], 0 );

			void RecurseParams( int[] variantIndices, int paramSelIndex ) {
				if( paramSelIndex < overloadParams.Length ) {
					for( int i = 0; i < overloadParams[paramSelIndex].Variants; i++ ) {
						variantIndices[paramSelIndex] = i;
						RecurseParams( variantIndices, paramSelIndex + 1 );
					}
				} else {
					// we've reached the end of an overload variant
					List<Param> overloadVariantParams = new List<Param>();
					for( int i = 0; i < overloadParams.Length; i++ ) {
						Param[] adds = overloadParams[i].GetVariant( variantIndices[i] );
						if( adds != null )
							overloadVariantParams.AddRange( adds );
					}

					overloads.Add( GetOverloadStr( overloadVariantParams ) );
				}
			}

			return overloads;
		}

		string GetOverloadStr( List<Param> overloadParams ) {
			// docs
			string inlineDocs = $"/// <summary>{summary}</summary>";

			// actual function
			string overloadParamsStr = string.Join( ", ", overloadParams.Select( p => p.FullMethodSig ) );

			// matrix setup and documentation depends only on input parameters
			Param.MtxFlags mtxFlags = Param.MtxFlags.None;
			overloadParams.ForEach( p => mtxFlags |= p.mtxFlags );
			overloadParams.ForEach( p => inlineDocs += $"<param name=\"{p.methodSigName}\">{p.desc.Replace( "[OBJECTNAME]", objectName.ToLowerInvariant() )}</param>" );

			// foreach argument in the target function call
			string callParams = "";
			foreach( TargetArg arg in targetCall.args ) {
				Param overloadParam = overloadParams.FirstOrDefault( p => p.targetArgNames.Contains( arg.name ) );
				if( overloadParam != null ) { // see if we have an overload param
					callParams += overloadParam.methodCallStr;
				} else if( constAssigns.ContainsKey( arg.name ) ) // see if we have any constant arguments
					callParams += constAssigns[arg.name];
				else if( arg.HasDefault ) // see if we have a default value
					callParams += arg.@default;
				else // else default to error :c
					callParams += $"<color=#f00>[MISSING {arg.name}]</color>";

				callParams += ", ";
			}

			callParams = callParams.Substring( 0, callParams.Length - 2 ); // remove last comma

			if( string.IsNullOrEmpty( overloadParamsStr ) == false )
				overloadParamsStr = $" {overloadParamsStr} "; // formatting, make empty ones not have spaces inside
			string functionHeader = $"{inlineDocs}\n[MethodImpl( INLINE )] public static void {overloadName}({overloadParamsStr})";
			string drawFuncCall = CodegenDrawOverloads.DEBUG_WRITE_EMPTY_BODIES ? "_ = 0" : $"{targetCall.name}( {callParams} )";

			if( mtxFlags == Param.MtxFlags.None ) {
				// inline function body
				return $"{functionHeader} => {drawFuncCall};";
			} else {
				// multi-line function body
				return $"{functionHeader} {{\n" +
					   $"{fBodyIndent}Draw.PushMatrix();\n" +
					   $"{fBodyIndent}{GetTransformation( mtxFlags )};\n" +
					   $"{fBodyIndent}{drawFuncCall};\n" +
					   $"{fBodyIndent}Draw.PopMatrix();\n" +
					   $"}}";
			}
		}

		const string fBodyIndent = "\t";

		static string GetTransformation( Param.MtxFlags flags ) {
			switch( flags ) {
				case Param.MtxFlags.Position:  return $"Draw.Translate( pos )"; // position only, no rotation
				case Param.MtxFlags.Angle:     return $"Draw.Rotate( angle )"; // angle only, no rotation (TMP)
				case Param.MtxFlags.PosRot:    return $"Draw.Matrix *= Matrix4x4.TRS( pos, rot, Vector3.one )";
				case Param.MtxFlags.PosNormal: return $"Draw.Matrix *= Matrix4x4.TRS( pos, Quaternion.LookRotation( normal ), Vector3.one )";
				case Param.MtxFlags.PosAngle:  return $"Draw.Translate( pos );\n{fBodyIndent}Draw.Rotate( angle )"; // pos + z angle (TMP)
				default:
					Debug.LogWarning( $"Invalid matrix configuration: {flags.ToString()}" );
					return "";
			}
		}


	}

}