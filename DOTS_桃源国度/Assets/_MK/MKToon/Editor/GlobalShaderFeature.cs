//////////////////////////////////////////////////////
// MK Toon Global Shader Feature             		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections.Generic;

namespace MK.Toon.Editor
{
    public enum MGlobalShaderFeatureOutlineFadingMode
    {
        Off = 0,
        Linear = 1,
        Exponential = 2,
        InverseExponential = 3
    }

    public enum GlobalShaderFeatureMode
    {
        Off = 0,
        On = 1
    }

    [System.Serializable]
    public abstract class GlobalShaderFeatureBase
    {
        private enum DynamicEnum { };

        public abstract void DrawInspector();

        public System.Enum modeEnum { get{ return (System.Enum) System.Enum.ToObject(typeof(DynamicEnum), mode); } }

        protected void DrawProperty<T>() where T : System.Enum
        {
            T convertedEnum = (T) System.Enum.ToObject(typeof(T) , mode);
            EditorGUI.BeginChangeCheck();
            convertedEnum = (T) EditorGUILayout.EnumPopup(new UnityEngine.GUIContent(name, tooltip), convertedEnum);
            if(EditorGUI.EndChangeCheck())
            {
                mode = System.Convert.ToInt32(convertedEnum);
            }
        }
        
        public GlobalShaderFeatureBase(System.Enum mode, List<string> identifiers, List<string> compileDirectives, string name, string tooltip)
        {
            this._name = name;
            this._mode = System.Convert.ToInt32(mode);
            this._identifiers = identifiers;
            this._tooltip = tooltip;
            this._compileDirectives = compileDirectives;
        }

        [UnityEngine.SerializeField]
        private int _mode;
        public int mode { get { return _mode; } set { _mode = value; } }
        [UnityEngine.SerializeField]
        private List<string> _identifiers;
        public List<string> identifiers { get { return _identifiers; } }
        [UnityEngine.SerializeField]
        private List<string> _compileDirectives;
        public List<string> compileDirectives { get { return _compileDirectives; } }
        [UnityEngine.SerializeField]
        private string _name;
        public string name { get { return _name; } }
        [UnityEngine.SerializeField]
        private string _tooltip;
        public string tooltip { get { return _tooltip; } }
    }

    [System.Serializable]
    public class GlobalShaderFeature : GlobalShaderFeatureBase
    {
        public GlobalShaderFeature(System.Enum mode, List<string> identifiers, List<string> compileDirectives, string name, string tooltip) : base(mode, identifiers, compileDirectives, name, tooltip){}

        public override void DrawInspector()
        {
            DrawProperty<GlobalShaderFeatureMode>();
        }
    }

    [System.Serializable]
    public class GlobalShaderFeatureOutlineFading : GlobalShaderFeatureBase
    {
        public GlobalShaderFeatureOutlineFading(System.Enum mode, List<string> identifiers, List<string> compileDirectives, string name, string tooltip) : base(mode, identifiers, compileDirectives, name, tooltip){}

        public override void DrawInspector()
        {
            DrawProperty<MGlobalShaderFeatureOutlineFadingMode>();
        }
    }
}
#endif
