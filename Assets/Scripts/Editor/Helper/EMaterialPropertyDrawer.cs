
using UnityEngine;
using UnityEditor;

namespace TEditor
{
    public class MaterialPropertyDrawerBase: MaterialPropertyDrawer
    {
        public virtual bool PropertyTypeCheck(MaterialProperty.PropType type) => true;
        public sealed override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            OnGUI(position,prop,label.text,editor);
        }

        public sealed override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!PropertyTypeCheck(prop.type))
            {
                GUI.Label(position, $"{prop.displayName} Type UnAvailable!", UEGUIStyle_Window.m_ErrorLabel);
                return;
            }
            OnPropertyGUI(position, prop, label, editor);
        }

        public virtual void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            
        }
    }
    public class Vector2Drawer: MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector2,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            prop.vectorValue = EditorGUI.Vector2Field(position, label,prop.vectorValue);
        }
    }
    public class Vector3Drawer : MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector3,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            prop.vectorValue = EditorGUI.Vector3Field(position, label, prop.vectorValue);
        }
    }
    public class FoldDrawer : MaterialPropertyDrawerBase
    {
        private string[] m_Keywords;
        public FoldDrawer(string[] _keywords) { m_Keywords = _keywords; }
        public FoldDrawer(string _kw1) : this(new string[] { _kw1 }) { }
        public FoldDrawer(string _kw1, string _kw2) : this(new string[] { _kw1, _kw2 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3) : this(new string[] { _kw1, _kw2, _kw3 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3, string _kw4) : this(new string[] { _kw1, _kw2, _kw3, _kw4 }) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public bool PropertyTypeCheck(MaterialProperty prop)
        {
            foreach (Material material in prop.targets)
                if (m_Keywords.Any(keyword => material.IsKeywordEnabled(keyword)))
                    return true;
            return false;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            if (PropertyTypeCheck(prop))
                return;
            editor.DefaultShaderProperty(prop, label);
        }
    }
    public class FoldoutDrawer: FoldDrawer
    {
        public FoldoutDrawer(string _kw1) : base(new string[] { _kw1 }) {}
        public FoldoutDrawer(string _kw1, string _kw2) : base(new string[] { _kw1, _kw2 }) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3) : base(new string[] { _kw1, _kw2 ,_kw3}) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3,string _kw4) : base(new string[] { _kw1, _kw2 ,_kw3,_kw4}) { Debug.Log(_kw1); }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!PropertyTypeCheck(prop))
                return;
            
            base.OnPropertyGUI(position, prop, label, editor);
            editor.DefaultShaderProperty(prop, label);
        }
    }

    public class ToggleTexDrawer: MaterialPropertyDrawerBase
    {
        protected readonly string m_Keyword;
        public ToggleTexDrawer(string _keyword) {  m_Keyword = _keyword; }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Texture;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            editor.DefaultShaderProperty(prop, label);
            if (!EditorGUI.EndChangeCheck()) 
                return;
            EnableKeyword(prop);
        }
        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            EnableKeyword(prop);
        }

        void EnableKeyword(MaterialProperty _property)
        {
            foreach (Material material in _property.targets)
                material.EnableKeyword(m_Keyword, _property.textureValue != null);
        }
    }

    public class ColorUsageDrawer : MaterialPropertyDrawerBase
    {
        private bool m_Alpha;
        private bool m_HDR;
        public ColorUsageDrawer(string _alpha,string _hdr)
        {
            m_Alpha = bool.Parse(_alpha);
            m_HDR = bool.Parse(_hdr);
        }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type)=>type== MaterialProperty.PropType.Color;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            prop.colorValue = EditorGUI.ColorField(position,new GUIContent(label), prop.colorValue,true,m_Alpha,m_HDR);
        }
    }
    
    public class MinMaxRangeDrawer : MaterialPropertyDrawerBase
    {
        private float m_Min;
        private float m_Max;
        private float m_ValueMin;
        private float m_ValueMax;
        private MaterialProperty property;
        public float GetFloat(MaterialEditor editor, string propertyName, out bool hasMixedValue)
        {
            hasMixedValue = editor.targets.Length > 1;
            return  ((Material) editor.targets[0]).HasFloat(propertyName)?((Material) editor.targets[0]) .GetFloat(propertyName):0;;
        }

        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Range;

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var prop0 = MaterialEditor.GetMaterialProperty(editor.targets, prop.name);
            var prop1 =  MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "End");

            float value0 = prop0.floatValue;
            float value1 =prop1.floatValue;
            
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0.0f;
            // EditorGUI.showMixedValue = hasMixedValue1;
            EditorGUI.BeginChangeCheck();

            Rect minmaxRect = position.Collapse(new Vector2(position.size.x / 5, 0f),new Vector2(0f,0f));
            EditorGUI.MinMaxSlider(minmaxRect,label,ref value0,ref value1,prop.rangeLimits.x,prop.rangeLimits.y);
            Rect labelRect = position.Collapse(new Vector2(position.size.x*4f / 5, 0f),new Vector2(1f,0f)).Move(new Vector2(2f,0f));
            GUI.Label(labelRect,$"{value0:F1}-{value1:F1}");
            
            if (EditorGUI.EndChangeCheck())
            {
                prop0.floatValue = value0;
                prop1.floatValue = value1;
            }
            
            EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}
