﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Object = System.Object;

namespace TEditor
{
    #region Attributes
    public class MainAttributePropertyDrawer<T> : PropertyDrawer where T : Attribute
    {
        static readonly Type kPropertyDrawerType = typeof(PropertyDrawer);
        private PropertyDrawer m_SubPropertyDrawer;
        PropertyDrawer GetSubPropertyDrawer(SerializedProperty _property)
        {
            if (m_SubPropertyDrawer != null)
                return m_SubPropertyDrawer;

            FieldInfo targetField = _property.GetFieldInfo();
            IEnumerable<Attribute> attributes = targetField.GetCustomAttributes();
            int order = attribute.order + 1;
            if (order >= attributes.Count())
                return null;

            Attribute nextAttribute = attributes.ElementAt(order);
            Type attributeType = nextAttribute.GetType();
            Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor").GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { attributeType });
            m_SubPropertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            kPropertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, targetField);
            kPropertyDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, nextAttribute);
            return m_SubPropertyDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawerHeight = GetSubPropertyDrawer(property)?.GetPropertyHeight(property, label);
            return customDrawerHeight ?? EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetSubPropertyDrawer(property);
            if (customDrawer != null)
            {
                customDrawer.OnGUI(position, property, label);
                return;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
        public bool CheckPropertyAvailable(bool fold, SerializedProperty _property, MFoldoutAttribute _attribute)
        {
            IEnumerable<(FieldInfo, object)> fields = _property.AllRelativeFields();
            return _attribute.m_FieldsMatches.All(fieldMatch => fields.Any(field => {
                if (field.Item1.Name != fieldMatch.Key)
                    return false;
                bool equals = fieldMatch.Value?.Contains(field.Item2) ?? field.Item2 is null;
                return fold ? !equals : equals;
            }));
        }
    }
    public class SubAttributePropertyDrawer<T> : PropertyDrawer where T : Attribute
    {
        public bool OnGUIAttributePropertyCheck(Rect _position, SerializedProperty _property, out T _targetAttribute, params SerializedPropertyType[] _checkTypes)
        {
            _targetAttribute = null;
            
            if (_checkTypes.Length!=0&&_checkTypes.All(p => _property.propertyType != p))
            {
                EditorGUI.LabelField(_position,
                    $"<Color=#FF0000>Attribute For {_checkTypes.ToString('|', type => type.ToString())} Only!</Color>", UEGUIStyle_Window.m_TitleLabel);
                return false;
            }
            _targetAttribute = attribute as T;
            return true;
        }
    }
    #region MainAttribute
    [CustomPropertyDrawer(typeof(MTitleAttribute))]
    public class MTitlePropertyDrawer : MainAttributePropertyDrawer<MTitleAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 2f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect titleRect = position;
            titleRect.height = 18;
            EditorGUI.LabelField(titleRect, label, UEGUIStyle_Window.m_TitleLabel);
            label.text = " ";
            base.OnGUI(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(MFoldoutAttribute))]
    public class MFoldoutPropertyDrawer : MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false, property, attribute as MFoldoutAttribute))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false, property, attribute as MFoldoutAttribute))
                return;
            base.OnGUI(position, property, label);
        }
    }
    [CustomPropertyDrawer(typeof(MFoldAttribute))]
    public class MFoldPropertyDrawer : MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(true, property, attribute as MFoldoutAttribute))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(true, property, attribute as MFoldoutAttribute))
                return;
            base.OnGUI(position, property, label);
        }
    }
    #endregion
    #region SubAttribute

    [CustomPropertyDrawer(typeof(IntEnumAttribute))]
    public class IntEnumPropertyDrawer : SubAttributePropertyDrawer<IntEnumAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,out IntEnumAttribute attribute,SerializedPropertyType.Float,SerializedPropertyType.Integer))
                return;
            property.intValue = EditorGUI.IntPopup(position,label,property.intValue,attribute.m_Values.Select(p=>new GUIContent( p.ToString())).ToArray(),attribute.m_Values);
        }
    }
    
    [CustomPropertyDrawer(typeof(ExtendButtonAttribute))]
    public class ButtonPropertyDrawer : SubAttributePropertyDrawer<ExtendButtonAttribute>
    {
        bool GetMethod(SerializedProperty _property, string _methodName, out MethodInfo _info)
        {
            _info = null;
            foreach (var methodInfo in _property.AllMethods())
            {
                if (methodInfo.Name == _methodName)
                {
                    _info = methodInfo;
                    break;
                }
            }
            if(_info==null)
                Debug.LogWarning($"No Method Found:{_methodName}|{_property.serializedObject.targetObject.GetType()}");
            return _info!=null;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var buttonAttribute = attribute as ExtendButtonAttribute;
            return EditorGUI.GetPropertyHeight(property,label) + buttonAttribute.m_Buttons.Length*20f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out var buttonAttribute))
                return;
            
            EditorGUI.PropertyField(position.Resize(position.size-new Vector2(0,20*buttonAttribute.m_Buttons.Length)), property, label,true);
            position = position.Reposition(position.x, position.y + EditorGUI.GetPropertyHeight(property, label,true) + 2);
            foreach (var (title,method,parameters) in buttonAttribute.m_Buttons)
            {
                position = position.Resize(new Vector2(position.size.x, 18));
                if (GUI.Button(position, title))
                {
                    if (!GetMethod(property, method, out var info))
                        continue;
                    info?.Invoke(property.serializedObject.targetObject,parameters);
                }
                
                position = position.Reposition(position.x, position.y +  20);
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampPropertyDrawer : SubAttributePropertyDrawer<ClampAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out ClampAttribute attribute, SerializedPropertyType.Float, SerializedPropertyType.Integer))
                return;

            EditorGUI.PropertyField(position, property, label);
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = Mathf.Clamp(property.intValue, (int)attribute.m_Min, (int)attribute.m_Max);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue, attribute.m_Min, attribute.m_Max);
                    break;
            }
        }
    }

    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : SubAttributePropertyDrawer<CullingMaskAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out CullingMaskAttribute attribute, SerializedPropertyType.Integer))
                return;
            Dictionary<int, string> allLayers = UECommon.GetAllLayers(true);
            List<string> values = new List<string>();
            foreach (int key in allLayers.Keys)
                values.Add(allLayers[key] == string.Empty ? null : allLayers[key]);
            for (int i = allLayers.Count - 1; i >= 0; i--)
            {
                if (allLayers.SelectValue(i) == string.Empty)
                    values.RemoveAt(i);
                else
                    break;
            }

            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, values.ToArray());
        }
    }
    [CustomPropertyDrawer(typeof(PositionAttribute))]
    public class PositionPropertyDrawer:SubAttributePropertyDrawer<PositionAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight( property, label,true)+20f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out PositionAttribute attribute, SerializedPropertyType.Vector3))
                return;

            Rect propertyRect = new Rect(position.position,position.size-new Vector2(0,20));
            EditorGUI.PropertyField(propertyRect, property, label, true);
            float buttonWidth = position.size.x / 5f;
            Rect buttonRect = new Rect(position.position+new Vector2(buttonWidth*4f,EditorGUI.GetPropertyHeight(property,label,true)),new Vector2(buttonWidth,20f));
            if (GUI.Button(buttonRect, "Edit"))
                GUITransformHandles.Begin(property);
        }
    }
    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorPropertyDrawer:SubAttributePropertyDrawer<RangeVectorAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            switch(property.propertyType)
            {
                case SerializedPropertyType.Vector2: return m_Foldout ? 40 : 20;
                case SerializedPropertyType.Vector3: return m_Foldout? 60:20; 
                case SerializedPropertyType.Vector4: return m_Foldout? 60:20;
            }
            return base.GetPropertyHeight(property, label);
        }
        Vector4 m_Vector;
        bool m_Foldout = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out RangeVectorAttribute attribute, SerializedPropertyType.Vector2,SerializedPropertyType.Vector3,SerializedPropertyType.Vector4))
                return;
            string format="";
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2: format = "X:{1:0.00} Y:{2:0.00}"; m_Vector = property.vector2Value; break;
                case SerializedPropertyType.Vector3: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00}"; m_Vector = property.vector3Value; break;
                case SerializedPropertyType.Vector4: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00} W:{4:0.00}"; m_Vector = property.vector4Value; break;
            }
            float halfWidth = position.width / 2;
            float startX = position.x;
            position.width = halfWidth;
            position.height = 18;
            m_Foldout = EditorGUI.Foldout(position,m_Foldout, string.Format("{0} | "+format, label.text,m_Vector.x,m_Vector.y,m_Vector.z,m_Vector.w));
            if (!m_Foldout)
                return;
            position.y += 20;
            m_Vector.x = EditorGUI.Slider(position, m_Vector.x, attribute.m_Min, attribute.m_Max);
            position.x += position.width;
            m_Vector.y = EditorGUI.Slider(position, m_Vector.y, attribute.m_Min, attribute.m_Max);

            if (property.propertyType== SerializedPropertyType.Vector2)
            {
                property.vector2Value = m_Vector;
                return;
            }
            position.x = startX;
            position.y += 20;
            m_Vector.z = EditorGUI.Slider(position, m_Vector.z, attribute.m_Min, attribute.m_Max);
            if(property.propertyType== SerializedPropertyType.Vector3)
            {
                property.vector3Value = m_Vector;
                return;
            }

            position.x += position.width;
            position.width = halfWidth;
            m_Vector.w = EditorGUI.Slider(position, m_Vector.w, attribute.m_Min, attribute.m_Max);
            property.vector4Value = m_Vector;
        }
    }
    
    //To Be Continued(Unitys Property Array)
    // [CustomPropertyDrawer(typeof(PreloadAssetsAttribute))]
    // public class PreloadAssetsPropertyDrawer : SubAttributePropertyDrawer<PreloadAssetsAttribute>
    // {
    //     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //     {
    //         return EditorGUI.GetPropertyHeight(property,label);
    //     }
    //
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         if (!OnGUIAttributePropertyCheck(position, property, out PreloadAssetsAttribute attribute, SerializedPropertyType.ObjectReference))
    //             return;
    //
    //         EditorGUI.PropertyField(position, property, label);
    //     }
    // }
    //
    #endregion
    #endregion
}

