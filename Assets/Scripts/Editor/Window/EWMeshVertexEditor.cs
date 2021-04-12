﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static TEditor.UERender;

namespace TEditor
{
    public class EWMeshVertexEditor : EditorWindow
    {
        public enum enum_EditorMode
        {
            Edit,
            Paint,
        }
        public ValueChecker<Mesh> m_SourceMesh { get; private set; } = new ValueChecker<Mesh>(null);
        public Mesh m_ModifingMesh { get; private set; }
        bool m_Debugging = false;
        public bool m_MaterialOverriding => m_MaterialOverride.m_Value;
        ValueChecker<bool> m_MaterialOverride =new ValueChecker<bool>(false);
        public ValueChecker<Material[]> m_Materials { get; private set; } = new ValueChecker<Material[]>(null);
        GameObject m_MeshObject;
        MeshFilter m_MeshFilter;
        MeshRenderer m_MeshRenderer;

        Dictionary<enum_EditorMode, MeshEditorHelperBase> m_EditorHelpers;
        MeshEditorHelperBase m_Helper=>m_EditorHelpers.ContainsKey(m_EditorMode)? m_EditorHelpers[m_EditorMode]:null;
        enum_EditorMode m_EditorMode;
        private void OnEnable()
        {
            m_MeshObject = new GameObject("Modify Mesh");
            m_MeshObject.hideFlags = HideFlags.HideAndDontSave;
            m_MeshFilter = m_MeshObject.AddComponent<MeshFilter>();
            m_MeshRenderer = m_MeshObject.AddComponent<MeshRenderer>();
            m_EditorHelpers = new Dictionary<enum_EditorMode, MeshEditorHelperBase>() { { enum_EditorMode.Edit, new MeshEditorHelper_Edit(this) }, { enum_EditorMode.Paint,new MeshEditorHelper_Paint(this) } };
            SceneView.duringSceneGui += OnSceneGUI;
            SwitchMode(enum_EditorMode.Paint);
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            m_EditorHelpers.Clear();
            m_SourceMesh = null;
             End();

            if (m_MeshObject) GameObject.DestroyImmediate(m_MeshObject);
            m_MeshFilter = null;
            m_MeshRenderer = null;
            m_MeshObject = null;
        }
        void OnSceneGUI(SceneView _sceneView)
        {
            if (!m_ModifingMesh)
                return;

            OnKeyboradInteract();
            if (m_Debugging)
                m_Helper.OnEditorSceneGUIDebug(_sceneView, m_MeshObject);

            Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
            m_Helper.OnEditorSceneGUI(_sceneView, m_MeshObject, this);
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (!UEGUI.EditorApplicationPlayingCheck())
                return;
            EditorWindowGUI();
            EditorGUILayout.EndVertical();
        }

        void EditorWindowGUI()
        {
            if (!MeshModifingCheck())
                return;
            GUILayout.Label("Editing:" + m_SourceMesh.m_Value.name, UEGUIStyle_Window.m_TitleLabel);
            m_EditorMode = (enum_EditorMode)EditorGUILayout.EnumPopup("Edit Mode (~)",m_EditorMode);

            m_Debugging = GUILayout.Toggle(m_Debugging, "Collision Debug");
            if (m_MaterialOverride.Check(GUILayout.Toggle(m_MaterialOverride.m_Value, "Material Override")))
                SetMaterial();
            if (m_MaterialOverride.m_Value && m_Materials.Check(UEGUI.Layout.ArrayField(m_Materials.m_Value)))
                SetMaterial(m_Materials.m_Value);

            GUILayout.Label("Commands:", UEGUIStyle_Window.m_TitleLabel);

            m_Helper.OnEditorWindowGUI();

            if (GUILayout.Button("Save"))
                Save();
        }

        bool MeshModifingCheck()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select A Mesh To Edit:", UEGUIStyle_Window.m_TitleLabel);
            if(m_SourceMesh.Check((Mesh)EditorGUILayout.ObjectField(m_SourceMesh.m_Value, typeof(Mesh), false)))
            {
                Begin();
                SceneView targetView = SceneView.sceneViews[0] as SceneView;
                targetView.pivot = m_MeshObject.transform.localToWorldMatrix.MultiplyPoint(m_SourceMesh.m_Value.bounds.GetPoint(Vector3.back + Vector3.up));
                targetView.rotation = Quaternion.LookRotation(m_MeshObject.transform.position - targetView.pivot);
            }
            GUILayout.EndHorizontal();
            if (!m_SourceMesh.m_Value)
                return false;

            if (!m_ModifingMesh)
            {
                if (GUILayout.Button("Begin Edit"))
                    Begin();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset"))
                    Begin();
                if (GUILayout.Button("Cancel"))
                    End();
                GUILayout.EndHorizontal();
            }
            return m_ModifingMesh;
        }

        public void Begin()
        {
            End();
            m_ModifingMesh = m_SourceMesh.m_Value.Copy();
            m_MeshFilter.sharedMesh = m_ModifingMesh;
            SetMaterial(m_Materials.m_Value);
            m_Helper.Begin();
        }
        public void End()
        {
            m_Helper?.End();
            m_ModifingMesh = null;
            m_MeshFilter.sharedMesh = null;
        }

        static bool m_RightClicking;
        void OnKeyboradInteract()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                m_RightClicking = true;
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                m_RightClicking = false;

            if (m_RightClicking||Event.current.type != EventType.KeyDown)
                return;

            KeyCode _keyCode = Event.current.keyCode;
            switch (_keyCode)
            {
                default:  m_Helper.OnKeyboradInteract(_keyCode);break;
                case KeyCode.BackQuote:SwitchMode(m_EditorMode.Next());break;
                case KeyCode.UpArrow: m_MeshObject.transform.Rotate(90f, 0, 0, Space.World); break;
                case KeyCode.DownArrow: m_MeshObject.transform.Rotate(-90f, 0, 0, Space.World); break;
                case KeyCode.LeftArrow: m_MeshObject.transform.Rotate(0, 90f, 0, Space.World); break;
                case KeyCode.RightArrow: m_MeshObject.transform.Rotate(0, -90f, 0, Space.World); break;
            }
            Repaint();
        }
        void SwitchMode(enum_EditorMode _mode)
        {
            if (m_EditorMode == _mode)
                return;

            if(m_Helper!=null)
                m_Helper.End();
            m_EditorMode=_mode;
            if (!m_MaterialOverride.m_Value)
                SetMaterial();
            if (!m_ModifingMesh)
                return;
            m_Helper.Begin();
        }
        void SetMaterial(Material[] _materials=null)
        {
            if (_materials == null)
                _materials = new Material[] { m_Helper.GetDefaultMaterial() };
            m_Materials.Check(_materials);
            m_MeshRenderer.sharedMaterials = m_Materials.m_Value;
        }

        public void Save()
        {
            if (!UECommon.SaveFilePath(out string filePath, "asset", m_ModifingMesh.name))
                return;

            UECommon.CreateOrReplaceMainAsset(m_ModifingMesh, UEPath.FilePathToAssetPath(filePath));
        }
    }
    public class MeshEditorHelperBase
    {
        public EWMeshVertexEditor m_Parent { get; private set; }
        protected Mesh m_SourceMesh => m_Parent.m_SourceMesh.m_Value;
        protected Mesh m_ModifingMesh => m_Parent.m_ModifingMesh;
        protected MeshPolygon[] m_Polygons { get; private set; }
        public MeshEditorHelperBase(EWMeshVertexEditor _parent) { m_Parent = _parent; }
        public virtual void Begin()
        {
            m_Polygons = m_ModifingMesh.GetPolygons(out int[] triangles);
        }
        public virtual Material GetDefaultMaterial() => new Material(Shader.Find("Game/Lit/Standard_Specular")) { hideFlags = HideFlags.HideAndDontSave };
        public virtual void End() { }
        public virtual void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window) { }
        public virtual void OnEditorWindowGUI() { }
        public virtual void OnKeyboradInteract(KeyCode _keycode) { }
        static Ray mouseRay;
        static Vector3 collisionPoint;
        public virtual void OnEditorSceneGUIDebug(SceneView _sceneView, GameObject _meshObject)
        {
            Handles.color = Color.red;
            Handles_Extend.DrawArrow(mouseRay.origin, mouseRay.direction, .2f , .01f);
            Handles.DrawLine(mouseRay.origin, mouseRay.direction * 10f + mouseRay.origin);
            Handles.matrix = _meshObject.transform.localToWorldMatrix;
            Handles.SphereHandleCap(0, collisionPoint, Quaternion.identity, .05f, EventType.Repaint);
        }

        protected static Ray ObjLocalSpaceRay(SceneView _sceneView,GameObject _meshObj)
        {
            Ray ray = _sceneView.camera.ScreenPointToRay(_sceneView.GetScreenPoint());
            mouseRay = ray;
            ray.origin = _meshObj.transform.worldToLocalMatrix.MultiplyPoint(ray.origin);
            ray.direction = _meshObj.transform.worldToLocalMatrix.MultiplyVector(ray.direction);
            return ray;
        }

        protected static int RayDirectedTriangleIntersect(MeshPolygon[] _polygons,Vector3[] _verticies, Ray _ray,out Vector3 hitPoint)
        {
            collisionPoint = Vector3.zero;
            float minDistance = float.MaxValue;
            int index= _polygons.LastIndex(p =>
            {
                bool intersect = UBoundingCollision.RayDirectedTriangleIntersect(p.GetDirectedTriangle(_verticies), _ray, true, true, out float distance);
                if (intersect && minDistance > distance)
                {
                    collisionPoint = _ray.GetPoint(distance);
                    minDistance = distance;
                    return true;
                }
                return false;
            });
            hitPoint = collisionPoint;
            return index;
        }

    }
    public class MeshEditorHelper_Edit:MeshEditorHelperBase
    {
        public MeshEditorHelper_Edit(EWMeshVertexEditor _parent) : base(_parent) { }
        enum enum_VertexEditMode
        {
            None,
            Position,
            Rotation,
        }
        int m_SelectedPolygon = -1;
        int m_SelectedVertexIndex = -1;

        float m_GUISize = 1f;
        bool m_EditSameVertex = true;
        const float C_VertexSphereRadius = .02f;
        readonly RangeFloat s_GUISizeRange = new RangeFloat(.005f, 4.995f);

        List<int> m_SubPolygons = new List<int>();
        bool m_SelectingPolygon => m_SelectedPolygon >= 0;
        bool m_SelectingVertex => m_SelectedVertexIndex != -1;
        ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>(Vector3.zero);
        ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>(Quaternion.identity);

        enum_VertexEditMode m_VertexEditMode;

        Vector3[] m_Verticies;
        ValueChecker<enum_VertexData> m_VertexDataSource = new ValueChecker<enum_VertexData>(enum_VertexData.Normal);
        List<Vector3> m_VertexDatas = new List<Vector3>();
        bool m_EditingVectors => m_VertexDataSource.m_Value != enum_VertexData.None && m_VertexDatas.Count > 0;
        public override void Begin()
        {
            base.Begin();
            m_Verticies = m_ModifingMesh.vertices;
            SelectVectorData(enum_VertexData.Normal);
            SelectVertex(0);
            SelectPolygon(0);
        }
        public override void End()
        {
            base.End();
            m_PositionChecker.Check(Vector3.zero);
            m_RotationChecker.Check(Quaternion.identity);
            m_VertexDataSource.Check(enum_VertexData.None);
            m_SelectedPolygon = -1;
            m_SelectedVertexIndex = -1;
            m_VertexEditMode = enum_VertexEditMode.Position;
            m_VertexDatas.Clear();
            m_SubPolygons.Clear();
            m_VertexDatas.Clear();
        }
        void SelectPolygon(int _index)
        {
            SelectVertex(-1);
            m_SelectedPolygon = _index;
            m_SubPolygons.Clear();
            if (_index < 0)
                return;
            MeshPolygon mainPolygon = m_Polygons[m_SelectedPolygon];
            Triangle mainTriangle = mainPolygon.GetTriangle(m_Verticies);
            m_Polygons.FindAllIndexes(m_SubPolygons, (index, polygon) => index != m_SelectedPolygon && polygon.GetTriangle(m_Verticies).m_Verticies.Any(subVertex => mainTriangle.m_Verticies.Any(mainVertex => mainVertex == subVertex)));
        }
        void SelectVertex(int _index)
        {
            m_SelectedVertexIndex = _index;
            if (_index < 0)
                return;
            m_PositionChecker.Check(m_Verticies[_index]);
            if (m_EditingVectors)
                m_RotationChecker.Check(Quaternion.LookRotation(m_VertexDatas[_index]));
        }
        void RecalculateBounds()
        {
            m_ModifingMesh.bounds = UBoundsChecker.GetBounds(m_Verticies);
        }
        void SelectVectorData(enum_VertexData _data)
        {
            if (!m_VertexDataSource.Check(_data))
                return;
            if (m_VertexDataSource.m_Value != enum_VertexData.None)
                m_ModifingMesh.GetVertexData(m_VertexDataSource.m_Value, m_VertexDatas);
        }
        public override void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window)
        {
            base.OnEditorWindowGUI();
            OnSceneInteract(_meshObject,_sceneView);
            OnDrawSceneHandles(_sceneView);
        }
        public void OnSceneInteract(GameObject _meshObject, SceneView _sceneView)
        {
            if (OnVertexInteracting())
                return;

            if (!(Event.current.type == EventType.Used && Event.current.button == 0))
                return;

            m_SelectedVertexIndex = -1;
            Ray ray = ObjLocalSpaceRay (_sceneView, _meshObject);
            if (OnSelectVertexCheck(ray))
                return;
            SelectPolygon(RayDirectedTriangleIntersect(m_Polygons, m_Verticies,ray,out Vector3 _hitPoint));
        }
        void OnDrawSceneHandles(SceneView _sceneView)
        {
            Handles.color = Color.white.SetAlpha(.5f);
            Handles.DrawWireCube(m_ModifingMesh.bounds.center, m_ModifingMesh.bounds.size * 1.2f);

            if (!m_SelectingPolygon)
                return;

            MeshPolygon _mainPolygon = m_Polygons[m_SelectedPolygon];

            foreach (var subPolygon in m_SubPolygons)
            {
                DirectedTriangle directedTriangle = m_Polygons[subPolygon].GetDirectedTriangle(m_Verticies);
                if (Vector3.Dot(directedTriangle.m_Normal, _sceneView.camera.transform.forward) > 0)
                    continue;
                Handles.color = Color.yellow.SetAlpha(.1f);
                Handles.DrawAAConvexPolygon(directedTriangle.m_Triangle.m_Verticies);
                Handles.color = Color.yellow;
                Handles.DrawLines(directedTriangle.m_Triangle.GetDrawLinesVerticies());
            }
            Triangle mainTriangle = _mainPolygon.GetTriangle(m_Verticies);
            Handles.color = Color.green.SetAlpha(.3f);
            Handles.DrawAAConvexPolygon(mainTriangle.m_Verticies);
            Handles.color = Color.green;
            Handles.DrawLines(mainTriangle.GetDrawLinesVerticies());

            if (!m_EditingVectors)
                return;
            Handles.color = Color.green;
            foreach (var indice in _mainPolygon.m_Indices)
            {
                Handles_Extend.DrawArrow(m_Verticies[indice], m_VertexDatas[indice], .1f * m_GUISize, .01f * m_GUISize);
                if (m_SelectedVertexIndex == indice)
                    continue;
                Handles_Extend.DrawWireSphere(m_Verticies[indice], m_VertexDatas[indice], C_VertexSphereRadius * m_GUISize);
            }
            Handles.color = Color.yellow;
            foreach (var subPolygon in m_SubPolygons)
            {
                foreach (var indice in m_Polygons[subPolygon].m_Indices)
                    Handles.DrawLine(m_Verticies[indice], m_Verticies[indice] + m_VertexDatas[indice] * .03f * m_GUISize);
            }
        }
        bool OnVertexInteracting()
        {
            if (!m_SelectingVertex)
                return false;

            switch (m_VertexEditMode)
            {
                default: return false;
                case enum_VertexEditMode.Position:
                    {
                        if (m_PositionChecker.Check(Handles.PositionHandle(m_PositionChecker.m_Value, m_EditingVectors ? Quaternion.LookRotation(m_VertexDatas[m_SelectedVertexIndex]) : Quaternion.identity)))
                        {
                            foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                m_Verticies[index] = m_PositionChecker.m_Value;
                            m_ModifingMesh.SetVertices(m_Verticies);
                            RecalculateBounds();
                        }
                    }
                    break;
                case enum_VertexEditMode.Rotation:
                    {
                        if (!m_EditingVectors)
                            return false;

                        if (m_RotationChecker.Check(Handles.RotationHandle(m_RotationChecker.m_Value, m_Verticies[m_SelectedVertexIndex])))
                        {
                            foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                m_VertexDatas[index] = m_RotationChecker.m_Value * Vector3.forward;
                            m_ModifingMesh.SetVertexData(m_VertexDataSource.m_Value, m_VertexDatas);
                        }
                    }
                    break;
            }

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                SelectVertex(-1);
                return false;
            }
            return true;
        }

        List<int> GetModifingIndices(int _srcIndex)
        {
            List<int> modifingIndices = new List<int>();
            modifingIndices.Add(_srcIndex);
            if (m_EditSameVertex)
                modifingIndices.AddRange(m_Verticies.FindAllIndexes(p => p == m_Verticies[_srcIndex]));
            return modifingIndices;
        }

        bool OnSelectVertexCheck(Ray _ray)
        {
            if (!m_SelectingPolygon)
                return false;

            foreach (var indice in m_Polygons[m_SelectedPolygon].m_Indices)
            {
                if (!UBoundingCollision.RayBSIntersect(m_Verticies[indice], C_VertexSphereRadius * m_GUISize, _ray))
                    continue;
                SelectVertex(indice);
                return true;
            }
            return false;
        }
        void ResetVertex(int _index)
        {
            if (_index < 0)
                return;
            Vector3[] verticies = m_SourceMesh.vertices;
            List<int> indices = GetModifingIndices(_index);
            foreach (var index in indices)
                m_Verticies[index] = verticies[index];
            m_PositionChecker.Check(m_Verticies[_index]);
            m_ModifingMesh.SetVertices(m_Verticies);
            RecalculateBounds();

            if (!m_EditingVectors)
                return;
            List<Vector3> vectors = new List<Vector3>();
            m_SourceMesh.GetVertexData(m_VertexDataSource.m_Value, vectors);
            foreach (var index in indices)
                m_VertexDatas[index] = vectors[index];
            m_ModifingMesh.SetVertexData(m_VertexDataSource.m_Value, m_VertexDatas);
            m_RotationChecker.Check(Quaternion.LookRotation(m_VertexDatas[_index]));
        }

        public override void OnEditorWindowGUI()
        {
            base.OnEditorWindowGUI();
            SelectVectorData((enum_VertexData)EditorGUILayout.EnumPopup("Edit Target", m_VertexDataSource.m_Value));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene GUI Size (- , +):");
            m_GUISize = GUILayout.HorizontalSlider(m_GUISize, s_GUISizeRange.start, s_GUISizeRange.end);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Edit Same Vertex (Tab):");
            m_EditSameVertex = GUILayout.Toggle(m_EditSameVertex, "");
            GUILayout.EndHorizontal();
            m_VertexEditMode = (enum_VertexEditMode)EditorGUILayout.EnumPopup("Vertex Edit Mode (Q , E):", m_VertexEditMode);
        }
        public override void OnKeyboradInteract(KeyCode _keycode)
        {
            base.OnKeyboradInteract(_keycode);
            switch (_keycode)
            {
                case KeyCode.W:
                    m_VertexEditMode = enum_VertexEditMode.Position;
                    break;
                case KeyCode.E:
                    m_VertexEditMode = enum_VertexEditMode.Rotation;
                    break;
                case KeyCode.R:
                    ResetVertex(m_SelectedVertexIndex);
                    break;
                case KeyCode.Tab:
                    m_EditSameVertex = !m_EditSameVertex;
                    break;
                case KeyCode.Escape:
                    {
                        if (m_SelectingVertex)
                            SelectVertex(-1);
                        else if (m_SelectingPolygon)
                            SelectPolygon(-1);
                    }
                    break;
                case KeyCode.Minus: m_GUISize = Mathf.Clamp(m_GUISize - .1f, s_GUISizeRange.start, s_GUISizeRange.end); break;
                case KeyCode.Equals: m_GUISize = Mathf.Clamp(m_GUISize + .1f, s_GUISizeRange.start, s_GUISizeRange.end); break;
            }
        }
    }

    public class MeshEditorHelper_Paint : MeshEditorHelperBase
    {
        public override Material GetDefaultMaterial() => new Material(Shader.Find("Hidden/VertexColorVisualize")) { hideFlags = HideFlags.HideAndDontSave };
        static readonly string[] KW_Sample = new string[] { "_SAMPLE_UV0","_SAMPLE_UV1","_SAMPLE_UV2","_SAMPLE_UV3","_SAMPLE_UV4","_SAMPLE_UV5","_SAMPLE_UV6","_SAMPLE_UV7", "_SAMPLE_COLOR", "_SAMPLE_NORMAL", "_SAMPLE_TANGENT" };
        static readonly string[] KW_Color = new string[] { "_VISUALIZE_R", "_VISUALIZE_G" ,"_VISUALIZE_B", "_VISUALIZE_A" };
        public enum enum_PaintMode
        {
            Const,
            Modify,
        }
        public enum enum_PaintColor
        {
            R=1,
            G=2,
            B=3,
            A=4,
        }
        public MeshEditorHelper_Paint(EWMeshVertexEditor _parent) : base(_parent) { }
        Vector3[] m_Verticies;
        Vector3[] m_Normals;
        enum_PaintMode m_PaintMode = enum_PaintMode.Const;
        ValueChecker<enum_PaintColor> m_PaintColor = new ValueChecker<enum_PaintColor>( enum_PaintColor.R);

        float m_PaintRadius =1f;
        float m_PaintValue = .5f;
        static readonly RangeFloat s_PaintScaleRange = new RangeFloat(.01f,2f);
        Vector3 m_PaintPosition;
        List<int> m_PaintAffectedIndices = new List<int>();

        ValueChecker<enum_VertexData> m_VertexDataSource = new ValueChecker<enum_VertexData>(enum_VertexData.None);
        List<Vector4> m_VertexDatas = new List<Vector4>();
        bool m_AvailableDatas => m_VertexDatas.Count > 0;
        public override void Begin()
        {
            base.Begin();
            m_Verticies = m_ModifingMesh.vertices;
            m_Normals = m_ModifingMesh.normals;
            SelectVertexDataSource( enum_VertexData.UV0);
        }
        public override void End()
        {
            base.End();
            SelectVertexDataSource(enum_VertexData.None);
        }
        void SelectVertexDataSource(enum_VertexData _source)
        {
            if (!m_VertexDataSource.Check(_source))
                return;
            m_VertexDatas.Clear();
            if( m_VertexDataSource.m_Value != enum_VertexData.None)
            {
                m_ModifingMesh.GetVertexData(_source, m_VertexDatas);
                if (!m_Parent.m_MaterialOverriding)
                    m_Parent.m_Materials.m_Value[0].EnableKeywords(KW_Sample, (int)m_VertexDataSource.m_Value-1);
            }
        }
        public override void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window)
        {
            base.OnEditorSceneGUI(_sceneView, _meshObject, _window);
            if (!m_AvailableDatas)
                return;
            OnInteractGUI(_sceneView, _meshObject);
            OnDrawHandles();
        }
        void OnDataChange() => m_ModifingMesh.SetVertexData(m_VertexDataSource.m_Value, m_VertexDatas);
        void OnInteractGUI(SceneView _sceneView, GameObject _meshObject)
        {
            Handles.color = Color.green;
            if (m_PaintPosition != Vector3.zero)
                Handles_Extend.DrawWireSphere(m_PaintPosition, Quaternion.identity, m_PaintRadius);

            if (Event.current.type == EventType.MouseMove)
            {
                Vector3 cameraLocal = _meshObject.transform.worldToLocalMatrix.MultiplyPoint(_sceneView.camera.transform.position);
                if (RayDirectedTriangleIntersect(m_Polygons, m_ModifingMesh.vertices, ObjLocalSpaceRay(_sceneView, _meshObject), out Vector3 paintPosition) != -1)
                {
                    m_PaintPosition = paintPosition;
                    m_PaintAffectedIndices.Clear();
                    float sqrRaidus = m_PaintRadius * m_PaintRadius;
                    m_Verticies.FindAllIndexes(m_PaintAffectedIndices, (index, p) => Vector3.Dot(m_Normals[index], p - cameraLocal) < 0 && (paintPosition - p).sqrMagnitude < sqrRaidus);
                }
            }

            if (!m_AvailableDatas)
                return;
            if (Event.current.type == EventType.MouseDown)
            {
                int button = Event.current.button;
                if (button != 0 && button != 2)
                    return;
                switch(m_PaintMode)
                {
                    default:throw new Exception("Invalid Type:" + m_PaintMode);
                    case enum_PaintMode.Const:
                        m_PaintAffectedIndices.Traversal(index => m_VertexDatas[index] = ApplyModify(m_VertexDatas[index], m_PaintValue, m_PaintMode, m_PaintColor.m_Value));
                        break;
                    case enum_PaintMode.Modify:
                        float value = button==0?m_PaintValue:-m_PaintValue;
                        m_PaintAffectedIndices.Traversal(index=>m_VertexDatas[index]=ApplyModify(m_VertexDatas[index],value,m_PaintMode,m_PaintColor.m_Value));
                        break;
                }
                OnDataChange();
            }
        }
        Vector4 ApplyModify(Vector4 _src, float _value, enum_PaintMode _paintMode, enum_PaintColor _targetColor)
        {
            switch (_paintMode)
            {
                default: throw new Exception("Invalid Type:" + _paintMode);
                case enum_PaintMode.Const:
                    switch (_targetColor)
                    {
                        default: throw new Exception("Invalid Target:" + _targetColor);
                        case enum_PaintColor.R: return new Vector4(_value, _src.y, _src.z, _src.w);
                        case enum_PaintColor.G: return new Vector4(_src.x, _value, _src.z, _src.w);
                        case enum_PaintColor.B: return new Vector4(_src.x, _src.y, _value, _src.w);
                        case enum_PaintColor.A: return new Vector4(_src.x, _src.y, _src.z, _value);
                    }
                case enum_PaintMode.Modify:
                    switch (_targetColor)
                    {
                        default: throw new Exception("Invalid Target:" + _targetColor);
                        case enum_PaintColor.R: return new Vector4(Mathf.Clamp(_src.x, 0, 1) + _value, _src.y, _src.z, _src.w);
                        case enum_PaintColor.G: return new Vector4(_src.x, Mathf.Clamp(_src.y + _value, 0, 1), _src.z, _src.w);
                        case enum_PaintColor.B: return new Vector4(_src.x, _src.y, Mathf.Clamp(_src.z + _value, 0, 1), _src.w);
                        case enum_PaintColor.A: return new Vector4(_src.x, _src.y, _src.z, Mathf.Clamp(_src.w + _value, 0, 1));
                    }
            }
        }
        void OnDrawHandles()
        {
            Handles.color = Color.magenta;
            foreach (var indice in m_PaintAffectedIndices)
            {
                Vector4 targetcolor = m_VertexDatas[indice];
                switch(m_PaintColor.m_Value)
                {
                    case enum_PaintColor.R: Handles.color = new Color(targetcolor.x, 0, 0, 1); break;
                    case enum_PaintColor.G: Handles.color = new Color(0, targetcolor.y, 0, 1); break;
                    case enum_PaintColor.B: Handles.color = new Color(0, 0, targetcolor.z, 1); break;
                    case enum_PaintColor.A: Handles.color = new Color(targetcolor.w, targetcolor.w, targetcolor.w, 1); break;
                }
                Handles.DrawLine(m_Verticies[indice], m_Verticies[indice] + m_Normals[indice] * .5f * m_PaintRadius);
            }
        }
        public override void OnEditorWindowGUI()
        {
            base.OnEditorWindowGUI();
            SelectVertexDataSource((enum_VertexData)EditorGUILayout.EnumPopup("Target", m_VertexDataSource.m_Value));
            if(!m_AvailableDatas)
            {
                EditorGUILayout.LabelField("<Color=#FF0000>Empty Vertex Data</Color>", UEGUIStyle_Window.m_ErrorLabel);
                if (GUILayout.Button("Fill With Empty Colors"))
                    for (int i = 0; i < m_Verticies.Length; i++)
                        m_VertexDatas.Add(Vector4.zero);
                return;
            }
            SetPaintColor((enum_PaintColor)EditorGUILayout.EnumPopup("Color (1 2 3 4)", m_PaintColor.m_Value));
            m_PaintRadius = EditorGUILayout.Slider("Scale (+ -)", m_PaintRadius, s_PaintScaleRange.start, s_PaintScaleRange.end);
            m_PaintMode = (enum_PaintMode)EditorGUILayout.EnumPopup("Mode (Tab)", m_PaintMode);
            m_PaintValue = EditorGUILayout.Slider("Value (Q E)",m_PaintValue, 0f, 1f);
        }
        void SetPaintColor(enum_PaintColor _color)
        {
            if (!m_PaintColor.Check(_color))
                return;
            if (!m_Parent.m_MaterialOverriding)
                m_Parent.m_Materials.m_Value[0].EnableKeywords(KW_Color, (int)m_PaintColor.m_Value);
        }

        public override void OnKeyboradInteract(KeyCode _keycode)
        {
            base.OnKeyboradInteract(_keycode);
            switch(_keycode)
            {
                case KeyCode.R:ResetSelected();break;
                case KeyCode.Tab:m_PaintMode = m_PaintMode.Next();break;
                case KeyCode.Alpha1: SetPaintColor(enum_PaintColor.R); break;
                case KeyCode.Alpha2: SetPaintColor(enum_PaintColor.G); break;
                case KeyCode.Alpha3: SetPaintColor(enum_PaintColor.B); break;
                case KeyCode.Alpha4: SetPaintColor(enum_PaintColor.A); break;
                case KeyCode.Q: m_PaintValue = Mathf.Clamp(m_PaintValue - .1f, 0, 1); break;
                case KeyCode.E: m_PaintValue = Mathf.Clamp(m_PaintValue + .1f, 0, 1); break;
                case KeyCode.Minus: m_PaintRadius = Mathf.Clamp(m_PaintRadius - .1f, s_PaintScaleRange.start, s_PaintScaleRange.end); break;
                case KeyCode.Equals: m_PaintRadius = Mathf.Clamp(m_PaintRadius + .1f, s_PaintScaleRange.start, s_PaintScaleRange.end); break;
            }
        }
        void ResetSelected()
        {
            List<Vector4> originDatas = new List<Vector4>();
            m_SourceMesh.GetVertexData(m_VertexDataSource.m_Value,originDatas);
            if (originDatas.Count > 0)
                return;
            m_PaintAffectedIndices.Traversal(index => m_VertexDatas[index] = originDatas[index]);
            OnDataChange();
        }
    }
}