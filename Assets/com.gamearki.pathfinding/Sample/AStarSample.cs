using System.Collections.Generic;
using GameArki.PathFinding.AStar;
using GameArki.PathFinding.Generic;
using UnityEditor;
using UnityEngine;

namespace GameArki.PathFinding.Sample
{

    public class AStarSample : MonoBehaviour
    {

        [Header("地图宽度")]
        public int width;

        [Header("地图长度")]
        public int length;

        [Header("起点")]
        public Transform start;

        [Header("终点")]
        public Transform end;

        [Header("可行走最大高度差")]
        public int walkableHeightDiff;

        [Header("最大高度")]
        public int maxHeight;

        [Header("允许斜线移动")]
        public bool allowDiagonalMove;

        [Header("开启路径平滑处理")]
        public bool needPathSmooth;

        bool isRunning = false;

        List<Int2> path;

        AstarEntity astarEntity;

        void Awake()
        {
            isRunning = true;
            astarEntity = new AstarEntity(width, length);
        }

        void Update()
        {
            int v = 0;
            if (Input.GetKeyDown(KeyCode.Mouse0)) v = 1;
            if (Input.GetKeyDown(KeyCode.Mouse1)) v = -1;
            if (v != 0)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    GetXY(hit.point, out int x, out int y);
                    if (!(x < 0 || x >= width || y < 0 || y >= length))
                    {
                        var oldHeight = astarEntity.GetXYHeight(x, y);
                        var newHeight = oldHeight + v;
                        newHeight = newHeight > maxHeight ? maxHeight : newHeight;
                        astarEntity.SetXYHeight(x, y, newHeight);
                    }
                }
            }
        }

        void FixedUpdate()
        {
            GetXY(start.position, out int startX, out int startY);
            GetXY(end.position, out int endX, out int endY);
            if (needPathSmooth) path = astarEntity.FindSmoothPath(startX, startY, endX, endY, walkableHeightDiff, allowDiagonalMove);
            else path = astarEntity.FindPath(startX, startY, endX, endY, walkableHeightDiff, allowDiagonalMove);
        }

        void OnDrawGizmos()
        {
            if (!isRunning) return;

            Gizmos.color = Color.gray;
            DrawMapLine();
            DrawHeightMap();
            DrawPath();
        }

        void DrawPath()
        {
            if (path != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var pos1 = path[i];
                    var pos2 = path[i + 1];
                    var p1 = new Vector3(pos1.X, pos1.Y);
                    var p2 = new Vector3(pos2.X, pos2.Y);
                    p1 += new Vector3(0.5f, 0.5f, 0.5f);
                    p2 += new Vector3(0.5f, 0.5f, 0.5f);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawSphere(p1, 0.2f);
                }
                var pos = path[path.Count - 1];
                var p = new Vector3(pos.X, pos.Y);
                p += new Vector3(0.5f, 0.5f, 0.5f);
                Gizmos.DrawSphere(p, 0.2f);
            }
        }

        void DrawHeightMap()
        {
            Color color = Gizmos.color;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var height = astarEntity.GetXYHeight(i, j);
                    color.a = (float)height / maxHeight;
                    Gizmos.color = color;
                    if (height != 0)
                    {
                        Gizmos.DrawCube(new Vector3(i + 0.5f, j + 0.5f), new Vector3(0.8f, 0.8f, 1f));
                    }
                }
            }
        }

        void DrawMapLine()
        {
            // Row
            for (int i = 0; i <= length; i++)
            {
                Gizmos.DrawLine(new Vector3(0, i, 0), new Vector3(width, i, 0));
            }
            // Column
            for (int i = 0; i <= width; i++)
            {
                Gizmos.DrawLine(new Vector3(i, 0, 0), new Vector3(i, length, 0));
            }
        }

        void OnGUI()
        {
            int pathNodeCount = path != null ? path.Count : 0;
            GUILayout.Label($"路径点个数:{pathNodeCount}");
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var pos = new Vector3(i + 0.5f, j + 0.5f);
                    var height = astarEntity.GetXYHeight(i, j);
                    Handles.Label(pos, height.ToString());
                }
            }
        }

        void GetXY(Vector3 pos, out int x, out int y)
        {
            var posX = pos.x;
            var posY = pos.y;
            x = Mathf.FloorToInt(posX);
            y = Mathf.FloorToInt(posY);
        }

    }

}
