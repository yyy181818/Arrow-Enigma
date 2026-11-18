using System.Collections.Generic;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// 多边形区域
    /// </summary>
    public class AreaRegion
    {
        //public List<Vector2> CurPoints { get; private set; }
        public string Name { get; private set; }
        public Vector2 CenterPoint { get; private set; }


        public AreaRegion(List<Vector3> points, string name)
        {
            //CurPoints = new List<Vector2>();
            Name = name;
            OnUpdatePoints(points);
        }

        public void OnUpdatePoints(List<Vector3> points)
        {
            //CurPoints.Clear();
            float sumX = 0;
            float sumY = 0;
            for (int i = 0; i < points.Count; i++)
            {
                sumX += points[i].x;
                sumY += points[i].z;
                //CurPoints.Add(new Vector2(points[i].x, points[i].z));
            }
            CenterPoint = new Vector2(sumX / points.Count, sumY / points.Count);
        }

        /// <summary>
        /// 判断点是否在多边形内
        /// </summary>
        /// <param name="p">点</param>
        /// <returns>判定</returns>
        public bool IsInside(List<Vector3> points, Vector2 p)
        {
            var poly = points;
            var px = p.x;
            var py = p.y;
            var flag = false;

            int l = poly.Count;
            int j = l - 1;

            for (var i = 0; i < l; i++)
            {
                var sx = poly[i].x;
                var sy = poly[i].z;
                var tx = poly[j].x;
                var ty = poly[j].z;

                // 点与多边形顶点重合
                if ((sx == px && sy == py) || (tx == px && ty == py))
                {
                    return true;
                }

                // 判断线段两端点是否在射线两侧
                if ((sy < py && ty >= py) || (sy >= py && ty < py))
                {
                    // 线段上与射线 Y 坐标相同的点的 X 坐标
                    var x = sx + (py - sy) * (tx - sx) / (ty - sy);

                    // 点在多边形的边上
                    if (x == px)
                    {
                        return true;
                    }

                    // 射线穿过多边形的边界
                    if (x > px)
                    {
                        flag = !flag;
                    }

                }

                j = i;
            }

            // 射线穿过多边形边界的次数为奇数时点在多边形内
            return flag;
        }

        public float CalculateDistanceToPolygon(List<Vector3> points, Vector2 point)
        {
            if (points == null || points.Count < 3)
            {
                Debug.LogError("[多边形区域] 多边形至少需要三个顶点。");
                return -1;
            }

            float minDistance = float.MaxValue;

            // 遍历多边形的每一条边
            for (int i = 0; i < points.Count; i++)
            {
                int nextIndex = (i + 1) % points.Count;
                Vector2 start =new Vector2(points[i].x, points[i].z);
                Vector2 end = new Vector2(points[nextIndex].x, points[nextIndex].z);

                // 计算点到边的距离
                float distance = DistanceToLineSegment(point, start, end);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            return minDistance;
        }

        // 计算点到线段的距离
        private float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 line = end - start;
            float lengthSquared = line.sqrMagnitude;

            if (lengthSquared == 0)
            {
                return Vector2.Distance(point, start);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - start, line) / lengthSquared);
            Vector2 projection = start + t * line;
            return Vector2.Distance(point, projection);
        }

        /// <summary>
        /// 生成墙体
        /// </summary>
        /// <param name="points"></param>
        /// <param name="wallMaterial"></param>
        /// <param name="wallHeight"></param>
        public GameObject GenerateWall(List<Vector3> points, Material wallMaterial, Transform parentNode, float groundHeight,float wallHeight = 4.0f)
        {
            if (points == null || points.Count < 3)
            {
                Debug.LogError("[多边形区域] 至少需要三个点来生成多边形墙体。");
                return null;
            }

            int pointCount = points.Count;
            int vertexCount = pointCount * 2;
            int triangleCount = pointCount * 2;
            int triangleArrayLength = triangleCount * 3;
            GameObject wallObject = new GameObject("GeneratedWall");
            wallObject.transform.SetParent(parentNode);
            wallObject.transform.localPosition = Vector3.zero;
            wallObject.transform.localRotation = Quaternion.identity;
            wallObject.transform.localScale = Vector3.one;
            MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[triangleArrayLength];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];

            Vector3 heightOffset = new Vector3(0f, wallHeight, 0f);

            // 设置底部和顶部顶点
            for (int i = 0; i < pointCount; i++)
            {
                vertices[i] = new Vector3(points[i].x, groundHeight, points[i].z);
                vertices[i + pointCount] = vertices[i] + heightOffset;

                // 计算 UV 坐标
                float u = (float)i / pointCount;
                uvs[i] = new Vector2(u, 0f);
                uvs[i + pointCount] = new Vector2(u, 1f);
            }

            int triangleIndex = 0;
            for (int i = 0; i < pointCount; i++)
            {
                int nextIndex = (i + 1) % pointCount;

                // 第一个三角形
                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = nextIndex;
                triangles[triangleIndex++] = i + pointCount;

                // 第二个三角形
                triangles[triangleIndex++] = nextIndex;
                triangles[triangleIndex++] = nextIndex + pointCount;
                triangles[triangleIndex++] = i + pointCount;
            }

            // 计算指向墙体内侧的法线
            for (int i = 0; i < pointCount; i++)
            {
                int nextIndex = (i + 1) % pointCount;
                Vector3 currentBottom = vertices[i];
                Vector3 nextBottom = vertices[nextIndex];
                Vector3 currentTop = vertices[i + pointCount];

                Vector3 side1 = nextBottom - currentBottom;
                Vector3 side2 = currentTop - currentBottom;

                Vector3 normal = Vector3.Cross(side1, side2).normalized;

                normals[i] = normal;
                normals[i + pointCount] = normal;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uvs;
            if (wallMaterial != null)
            {
                meshRenderer.material = wallMaterial;
            }
            else
            {
                Debug.LogWarning("[多边形区域] 未指定墙体材质，将使用默认材质。");
            }
            return wallObject;
        }
    }
}
