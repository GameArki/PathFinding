using System.Collections.Generic;
using GameArki.PathFinding.AStar;
using GameArki.PathFinding.Generic;
using UnityEngine;

namespace GameArki.PathFinding.Sample
{

    public class AStarSample : MonoBehaviour
    {

        [Header("地图宽度")]
        public int width;

        [Header("地图高度")]
        public int height;

        [Header("起点")]
        public Transform start;

        [Header("终点")]
        public Transform end;

        [Header("允许斜线移动")]
        public bool allowDiagonalMove;

        [Header("开启路径平滑处理")]
        public bool needPathSmooth;

        byte[,] map;
        bool isRunning = false;

        List<AStarNode> path;

        void Awake()
        {
            map = new byte[width, height];
            isRunning = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    GetXY(hit.point, out int x, out int y);
                    var lenX = map.GetLength(0);
                    var lenY = map.GetLength(1);
                    if (!(x < 0 || x >= lenX || y < 0 || y >= lenY))
                    {
                        map[x, y] = map[x, y] == 1 ? map[x, y] = 0 : map[x, y] = 1;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            GetXY(start.position, out int startX, out int startY);
            GetXY(end.position, out int endX, out int endY);
            path = AStarUtil.FindPath(map, startX, startY, endX, endY, allowDiagonalMove);
            if (path != null)
            {
                if (needPathSmooth)
                {
                    var smoothPath = AStarUtil.GetSmoothPath(map, path);
                    path = smoothPath;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!isRunning) return;

            Gizmos.color = Color.black;
            DrawMapLine();
            DrawObstacles();
            DrawPath();
        }

        void DrawPath()
        {
            if (path != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var pos1 = path[i].pos;
                    var pos2 = path[i + 1].pos;
                    var p1 = new Vector3(pos1.X, pos1.Y);
                    var p2 = new Vector3(pos2.X, pos2.Y);
                    p1 += new Vector3(0.5f, 0.5f, 0.5f);
                    p2 += new Vector3(0.5f, 0.5f, 0.5f);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawSphere(p1, 0.2f);
                }
                var pos = path[path.Count - 1].pos;
                var p = new Vector3(pos.X, pos.Y);
                p += new Vector3(0.5f, 0.5f, 0.5f);
                Gizmos.DrawSphere(p, 0.2f);
            }
        }

        void DrawObstacles()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] != 0)
                    {
                        Gizmos.DrawCube(new Vector3(i + 0.5f, j + 0.5f), new Vector3(0.8f, 0.8f, 1f));
                    }
                }
            }
        }

        void DrawMapLine()
        {
            // Row
            for (int i = 0; i <= height; i++)
            {
                Gizmos.DrawLine(new Vector3(0, i, 0), new Vector3(width, i, 0));
            }
            // Column
            for (int i = 0; i <= width; i++)
            {
                Gizmos.DrawLine(new Vector3(i, 0, 0), new Vector3(i, height, 0));
            }
        }

        void GetXY(Vector3 pos, out int x, out int y)
        {
            var posX = pos.x;
            var posY = pos.y;
            x = Mathf.FloorToInt(posX);
            y = Mathf.FloorToInt(posY);
        }

        void OnGUI()
        {
            int pathNodeCount = path != null ? path.Count : 0;
            GUILayout.Label($"路径点个数:{pathNodeCount}");
        }

    }

}
