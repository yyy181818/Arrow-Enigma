using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TexaXRFramework.Core;

[CustomEditor(typeof(LBEAreaDefine))]
[CanEditMultipleObjects]
public class LBEAreaDefineEditor : Editor
{
    private LBEAreaDefine self;
    public float CapSize = 0.3f;

    private float _lastValue = 0;

    private int _lastOrder = -1; // 记录上一次的索引值，用于避免不必要的刷新

    private void OnEnable()
    {
        self = target as LBEAreaDefine;
        // 初始化时获取一次索引
        UpdateOrderFromParentIndex();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        self.AreaName = $"{scene.name}_{self.gameObject.name}";

        // 更新Order为父物体下的索引，并仅在值变化时刷新
        UpdateOrderFromParentIndex();

        if (self.ContentAreaNodeLocalPosList.Count < 1 || self.ContentAreaNodePosList.Count < 1)
        {
            var bluePoints = GenerateRectangle(self.transform.position);
            foreach (var pp in bluePoints)
            {
                self.ContentAreaNodePosList.Add(pp);
                self.ContentAreaNodeLocalPosList.Add(pp - self.transform.position);
            }

            var orangePoints = GenerateRectangle(self.transform.position, 4f, 2f);
            foreach (var pp in orangePoints)
            {
                self.ContentBoundaryNodePosList.Add(pp);
                self.ContentBoundaryNodeLocalPosList.Add(pp - self.transform.position);
            }
        }

        GUILayout.Space(20);

        if (GUILayout.Button("重置边界"))
        {
            ResetBoundary();
            EditorUtility.SetDirty(self);
        }
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("增加单个内容区域节点(蓝色)"))
        {
            Vector3 firstNode = self.ContentAreaNodePosList[0];
            Vector3 lastNode = self.ContentAreaNodePosList[self.ContentAreaNodePosList.Count - 1];
            Vector3 newPoint = (firstNode + lastNode) / 2;
            self.ContentAreaNodePosList.Add(newPoint);
            self.ContentAreaNodeLocalPosList.Add(newPoint - self.transform.position);
            EditorUtility.SetDirty(self);
            SceneView.RepaintAll();
        }
        if (self.ContentAreaNodePosList.Count > 4)
        {
            if (GUILayout.Button("删除单个内容区域节点(蓝色)"))
            {
                self.ContentAreaNodePosList.RemoveAt(self.ContentAreaNodePosList.Count - 1);
                self.ContentAreaNodeLocalPosList.RemoveAt(self.ContentAreaNodeLocalPosList.Count - 1);
                EditorUtility.SetDirty(self);
                SceneView.RepaintAll();
            }
        }
        GUILayout.EndHorizontal();
                GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("增加单个内容边界节点(红色)"))
        {
            Vector3 firstNode = self.ContentBoundaryNodePosList[0];
            Vector3 lastNode = self.ContentBoundaryNodePosList[self.ContentBoundaryNodePosList.Count - 1];
            Vector3 newPoint = (firstNode + lastNode) / 2;
            self.ContentBoundaryNodePosList.Add(newPoint);
            self.ContentBoundaryNodeLocalPosList.Add(newPoint - self.transform.position);
            EditorUtility.SetDirty(self);
            SceneView.RepaintAll();
        }
        if (self.ContentBoundaryNodePosList.Count > 4)
        {
            if (GUILayout.Button("删除单个内容边界节点(红色)"))
            {
                self.ContentBoundaryNodePosList.RemoveAt(self.ContentBoundaryNodePosList.Count - 1);
                self.ContentBoundaryNodeLocalPosList.RemoveAt(self.ContentBoundaryNodeLocalPosList.Count - 1);
                EditorUtility.SetDirty(self);
                SceneView.RepaintAll();
            }
        }
        GUILayout.EndHorizontal();
        float sum = 0;
        foreach (var item in self.ContentAreaNodePosList)
        {
            sum += item.x;
            sum += item.y;
        }
        foreach (var item in self.ContentBoundaryNodePosList)
        {
            sum += item.x;
            sum += item.y;
        }

        var dis = sum - _lastValue;
        if (Math.Abs(dis) > 0.1f)
        {
            _lastValue = sum;
            EditorUtility.SetDirty(self);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 从父物体索引更新Order值
    /// </summary>
    private void UpdateOrderFromParentIndex()
    {
        if (self == null) return;
        if(self.Order== _lastOrder) return;
        Transform parent = self.transform.parent;
        int currentOrder = -1;

        // 如果存在父物体，计算当前物体在父物体中的索引
        if (parent != null)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == self.transform)
                {
                    currentOrder = i;
                    break;
                }
            }
        }
        else
        {
            // 无父物体时给出提示
            Debug.LogWarning($"{self.gameObject.name} 没有父物体，Order将设为-1", self.gameObject);
        }

        // 仅在值发生变化时才更新，避免频繁刷新
        if (currentOrder != _lastOrder|| self.Order!=(currentOrder+1))
        {
            self.Order =(byte)(currentOrder+1);
            _lastOrder = currentOrder;
        }
    }

    private void ResetBoundary()
    {
        self.ContentAreaNodePosList.Clear();
        self.ContentBoundaryNodePosList.Clear();
        self.ContentAreaNodeLocalPosList.Clear();
        self.ContentBoundaryNodeLocalPosList.Clear();
    }

    public Vector3[] GenerateRectangle(Vector3 position, float width = 2f, float height = 1f)
    {
        Vector3[] rectanglePoints = new Vector3[4];
        rectanglePoints[0] = position + new Vector3(-width / 2, 0, -height / 2); // 左下
        rectanglePoints[1] = position + new Vector3(width / 2, 0, -height / 2);  // 右下
        rectanglePoints[2] = position + new Vector3(width / 2, 0, height / 2);   // 右上
        rectanglePoints[3] = position + new Vector3(-width / 2, 0, height / 2);  // 左上
        return rectanglePoints;
    }

    private void OnSceneGUI()
    {
        if (self.IsEditContentAreaBoundary)
        {
            DrawBoundary(self.ContentAreaNodePosList, self.ContentAreaNodeLocalPosList, Color.blue);
        }

        if (self.IsEditContentBoundaryy)
        {
            DrawBoundary(self.ContentBoundaryNodePosList, self.ContentBoundaryNodeLocalPosList, Color.red);
        }
    }

    private void DrawBoundary(List<Vector3> pointsList, List<Vector3> localWaypoints, Color color)
    {
        if (pointsList.Count < 1)
        {
            return;
        }
        Handles.color = Color.green;
        for (int i = 0; i < pointsList.Count - 1; i++)
        {
            Handles.DrawLine(pointsList[i], pointsList[i + 1]);
        }

        if (pointsList.Count > 1)
        {
            Handles.DrawLine(pointsList[0], pointsList[pointsList.Count - 1]);
        }

        Handles.color = color;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        for (int i = 0; i < pointsList.Count; i++)
        {
            Handles.SphereHandleCap(0, pointsList[i], Quaternion.identity, CapSize, EventType.Repaint);
        }

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        for (int i = 0; i < pointsList.Count; i++)
        {
            pointsList[i] = Handles.PositionHandle(self.transform.position + localWaypoints[i], Quaternion.identity);
            localWaypoints[i] = pointsList[i] - self.transform.position;
        }
    }
}

