using System.Collections.Generic;
using NS_LBEShareData;
using UnityEngine;
namespace TexaXRFramework.Core
{
    /// <summary>
    /// 区域定义
    /// </summary>
    public class LBEAreaDefine : MonoBehaviour
    {
        #region 对外接口

        [Header("区域顺序(默认从1开始)")]
        /// <summary>
        /// 次序：从1开始
        /// </summary>
        public byte Order = 1;

        [Header("区域最长停留时间(秒)")]
        /// <summary>
        /// 区域最大可停留时间:秒
        /// 默认：5分钟
        /// </summary>
        public int MaxStayTime = 300;

        [Header("区域最短停留时间(秒)")]
        /// <summary>
        /// 区域最短停留时间：秒
        /// </summary>
        public int MinStayTime = 100;

        [Header("区域名(唯一标识)")]
        /// <summary>
        /// 区域名：当前场景内需要唯一标识
        /// </summary>
        public string AreaName;

        [Header("内容区域显示类型")]
        public LBEContentAreaType ContentAreaType = LBEContentAreaType.Hide;
        public LineRenderer BlueLine;

        [Header("内容边界显示类型")]
        public LBEContentBoundaryType ContentBoundaryType = LBEContentBoundaryType.Hide;
        public LineRenderer OrangeLine;
        [Header("是否生成边界墙")]
        public bool IsGenerateBoundaryWalls;
        [Header("边界墙材质")]
        public Material WallMaterial;
        [Header("靠近边界墙显示最小距离")]
        public float ContentBoundaryMinDistance=0.3f;
        [HideInInspector]
        public List<Vector3> ContentAreaNodePosList = new List<Vector3>();

        [HideInInspector]
        public List<Vector3> ContentBoundaryNodePosList = new List<Vector3>();
        [HideInInspector]
        public List<Vector3> ContentAreaNodeLocalPosList = new List<Vector3>();

        [HideInInspector]
        public List<Vector3> ContentBoundaryNodeLocalPosList = new List<Vector3>();

        [Header("区域边界线高度")]
        public float LineHeight = 0.15f;

        /// <summary>
        /// 判断点是否在区域内
        /// </summary>
        /// <param name="pos">点</param>
        /// <param name="areaCenterPoint">区域中心</param>
        /// <returns>判断结果</returns>
        public bool IsContentBoundaryInside(Vector3 pos, out Vector2 areaCenterPoint)
        {
            areaCenterPoint = _areaRegion.CenterPoint;
            return _areaRegion.IsInside(ContentBoundaryNodePosList, new Vector2(pos.x, pos.z));
        }

        /// <summary>
        /// 强制显示内容区域
        /// </summary>
        public void ForceShowContentArea()
        {
            if (!_isForceShowContentArea)
            {
                _isForceShowContentArea = true;
                _lastContentAreaShow = BlueLine.gameObject.activeSelf;
                BlueLine.gameObject.SetActive(true);

            }
        }

        /// <summary>
        /// 强制恢复内容区域
        /// </summary>
        public void ForceRecoveryContentArea()
        {
            if (_isForceShowContentArea)
            {
                BlueLine.gameObject.SetActive(_lastContentAreaShow);
                _isForceShowContentArea = false;
            }
        }
        #endregion

        private List<Vector3> _tempPointsList = new List<Vector3>();

        /// <summary>
        /// 区域范围
        /// </summary>
        private AreaRegion _areaRegion;

        /// <summary>
        /// 是否第一次进入内容区域
        /// </summary>
        private bool _isFirstEntryContentArea = false;
        private Transform _detectionTarget;
        private GameObject _boundaryWallsObj;
        private bool _isForceShowContentArea = false;
        private bool _lastContentAreaShow = false;
        private float _checkInterval = 0.1f; // 检测间隔时间（秒）
        private float _timer = 0f;
        private Vector3 _lastPosition; // 记录上一次的位置，用于检测父节点移动
        private void Awake()
        {
            GetAreaPointLocalPos();
            _areaRegion = new AreaRegion(ContentAreaNodePosList, AreaName);
            _detectionTarget = Camera.main.transform;
            _lastPosition = transform.position;
        }

        private void Start()
        {
            if (BlueLine != null)
            {
                switch (ContentAreaType)
                {
                    case LBEContentAreaType.Hide:
                        BlueLine.gameObject.SetActive(false);
                        break;
                    case LBEContentAreaType.EnterHide:
                        BlueLine.gameObject.SetActive(true);
                        break;
                    case LBEContentAreaType.Show:
                        BlueLine.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
            if (OrangeLine != null)
            {
                switch (ContentBoundaryType)
                {
                    case LBEContentBoundaryType.Hide:
                        OrangeLine.gameObject.SetActive(false);
                        break;
                    case LBEContentBoundaryType.Show:
                        OrangeLine.gameObject.SetActive(true);
                        GenerateBoundaryWalls(true);
                        break;
                    case LBEContentBoundaryType.ShowWhenNearby:
                        OrangeLine.gameObject.SetActive(false);
                        GenerateBoundaryWalls(false);
                        break;
                    default:
                        break;
                }
            }
            if (BlueLine != null)
            {
                DrawLine(BlueLine, GetPointsList(ContentAreaNodeLocalPosList));
            }
            if (OrangeLine != null)
            {
                DrawLine(OrangeLine, GetPointsList(ContentBoundaryNodeLocalPosList));
            }
        }

        void GetAreaPointLocalPos()
        {
            if (ContentAreaNodePosList.Count > 0)
            {
                ContentAreaNodeLocalPosList.Clear();
                foreach (var node in ContentAreaNodePosList)
                {
                    ContentAreaNodeLocalPosList.Add(node - transform.position);
                }
            }

            if (ContentBoundaryNodePosList.Count > 0)
            {
                ContentBoundaryNodeLocalPosList.Clear();
                foreach (var node in ContentBoundaryNodePosList)
                {
                    ContentBoundaryNodeLocalPosList.Add(node - transform.position);
                }
            }
        }

        private void Update()
        {
            // 检测父节点是否移动，若移动则更新世界坐标
            if (transform.position != _lastPosition)
            {
                UpdateWorldPositions();
                _lastPosition = transform.position;
            }

            _timer += Time.deltaTime;
            if (_timer >= _checkInterval)
            {
                ContentAreaInRuntime();
                ContentBoundaryInRuntime();
                _timer = 0f;
            }
        }

        /// <summary>
        /// 根据本地坐标重新计算世界坐标，使区域跟随父节点移动
        /// </summary>
        private void UpdateWorldPositions()
        {
            if (ContentAreaNodeLocalPosList.Count > 0)
            {
                ContentAreaNodePosList.Clear();
                foreach (var localPos in ContentAreaNodeLocalPosList)
                {
                    ContentAreaNodePosList.Add(localPos + transform.position);
                }
                _areaRegion.OnUpdatePoints(ContentAreaNodePosList);
            }

            if (ContentBoundaryNodeLocalPosList.Count > 0)
            {
                ContentBoundaryNodePosList.Clear();
                foreach (var localPos in ContentBoundaryNodeLocalPosList)
                {
                    ContentBoundaryNodePosList.Add(localPos + transform.position);
                }
            }
        }

        private void ContentAreaInRuntime()
        {
            if (BlueLine != null && ContentAreaNodeLocalPosList.Count > 0)
            {
                switch (ContentAreaType)
                {
                    case LBEContentAreaType.EnterHide:
          
                        if (!_isFirstEntryContentArea  && BlueLine.gameObject.activeSelf)
                        {
                            if (_detectionTarget!=null&& _areaRegion.IsInside(ContentAreaNodePosList,new Vector2(_detectionTarget.position.x, _detectionTarget.position.z)))
                            {
                                _isFirstEntryContentArea=true;
                                if (!_isForceShowContentArea)
                                {
                                    BlueLine.gameObject.SetActive(false);
                                }               
                            }                           
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void ContentBoundaryInRuntime()
        {
            if (OrangeLine != null && ContentBoundaryNodeLocalPosList.Count > 0)
            {
                switch (ContentBoundaryType)
                {
                    case LBEContentBoundaryType.ShowWhenNearby:
                        var pos = new Vector2(_detectionTarget.position.x, _detectionTarget.position.z);
                        if (_areaRegion.IsInside(ContentBoundaryNodePosList, pos))
                        {

                            if (_areaRegion.CalculateDistanceToPolygon(ContentBoundaryNodePosList, pos) <= ContentBoundaryMinDistance)
                            {
                                if (!OrangeLine.gameObject.activeSelf)
                                {
                                    OrangeLine.gameObject.SetActive(true);
                                    if (_boundaryWallsObj!=null){_boundaryWallsObj.gameObject.SetActive(true);}
                                }
                           
                            }
                            else
                            {
                                if (OrangeLine.gameObject.activeSelf)
                                {
                                    OrangeLine.gameObject.SetActive(false);
                                    if (_boundaryWallsObj != null) { _boundaryWallsObj.gameObject.SetActive(false); }
                                }
                            }
                        }
                        else
                        {
                            if (OrangeLine.gameObject.activeSelf)
                            {
                                OrangeLine.gameObject.SetActive(false);
                                if (_boundaryWallsObj != null) { _boundaryWallsObj.gameObject.SetActive(false); }
                            }
                        }                   
                        break;
                    default:
                        break;
                }
            }
        }

        private List<Vector3> GetPointsList(List<Vector3> nodePosList)
        {
            Vector3 startPos = nodePosList[0];
            var goPos = transform.position;
            _tempPointsList.Clear();
            foreach (var node in nodePosList)
            {
                _tempPointsList.Add(new Vector3(node.x + goPos.x, LineHeight, node.z + goPos.z));
            }
            return _tempPointsList;
        }

        private void DrawLine(LineRenderer line, List<Vector3> pointsList)
        {
            var lineSize = 0.05f;
            line.startWidth = lineSize;
            line.endWidth = lineSize;
            line.positionCount = pointsList.Count;
            line.SetPositions(pointsList.ToArray());
        }

        private void GenerateBoundaryWalls(bool show)
        {
            if (IsGenerateBoundaryWalls)
            {
                _boundaryWallsObj = _areaRegion.GenerateWall(ContentBoundaryNodeLocalPosList, WallMaterial,gameObject.transform, LineHeight);
                _boundaryWallsObj.gameObject.SetActive(show);
            }
        }
        private void OnDestroy()
        {
            if (_boundaryWallsObj != null) { Destroy(_boundaryWallsObj); }    
        }
#if UNITY_EDITOR
        [Header("编辑内容区域")]
        public bool IsEditContentAreaBoundary = false;
        [Header("编辑内容边界")]
        public bool IsEditContentBoundaryy = false;

        void OnDrawGizmos()
        {
            DrawBoundary(ContentAreaNodePosList, Color.blue);
            DrawBoundary(ContentBoundaryNodePosList, Color.red);
        }

        private void DrawBoundary(List<Vector3> pointsList, UnityEngine.Color color)
        {
            Gizmos.matrix = UnityEngine.Matrix4x4.identity;
            for (int i = 0; i < pointsList.Count; i++)
            {
                var node = pointsList[i];
                Gizmos.color = color;
                Gizmos.DrawSphere(node, 0.1f);

                if (i + 1 < pointsList.Count)
                {
                    Gizmos.DrawLine(node, pointsList[i + 1]);
                }
                else
                {
                    Gizmos.DrawLine(node, pointsList[0]);
                }
            }
        }
#endif
    }
}
