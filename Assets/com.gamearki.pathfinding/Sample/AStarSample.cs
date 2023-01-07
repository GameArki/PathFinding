using System.Collections.Generic;
using GameArki.PathFinding.AStar;
using GameArki.PathFinding.Generic;
using UnityEditor;
using UnityEngine;

namespace GameArki.PathFinding.Sample {

    public class AStarSample : MonoBehaviour {

        [Header("地图宽度")]
        public int width;

        [Header("地图长度")]
        public int length;

        [Header("起点")]
        public Transform start;

        [Header("终点")]
        public Transform end;

        [Header("允许斜线移动")]
        public bool allowDiagonalMove;

        [Header("开启路径平滑处理")]
        public bool needPathSmooth;

        bool isRunning = false;

        AstarEntity astarEntity;
        Int2 walkableHeightDiffRange;

        List<Int2> path;

        // For GUI
        int findTimes_eachFrame = 1;
        int walkableHeightDiffRangeX = -1;
        int walkableHeightDiffRangeY = 0;
        int maxHeight = 5;

        void Awake() {
            // Heap<int> heap = new Heap<int>(Comparer<int>.Default, 10);
            // heap.Push(5);
            // heap.Push(3);
            // heap.Push(2);
            // heap.Push(6);
            // heap.Push(4);
            // heap.Push(1);
            // heap.Push(4);
            // heap.Push(4);
            // heap.Push(2);
            // heap.Push(2);
            // int? node = heap.Pop();
            // while (node != null)
            // {
            //     Debug.Log($"node:{node}");
            //     node = heap.Pop();
            // }

            isRunning = true;
            astarEntity = new AstarEntity(width, length);
        }

        void Update() {
            if (!isRunning) return;
            int v = 0;
            if (Input.GetKeyDown(KeyCode.Mouse0)) v = 1;
            if (Input.GetKeyDown(KeyCode.Mouse1)) v = -1;
            if (v != 0) {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {

                    var pos = GetXY(hit.point);
                    if (!(pos.X < 0 || pos.X >= width || pos.Y < 0 || pos.Y >= length)) {
                        var oldHeight = astarEntity.GetXYHeight(pos);
                        var newHeight = oldHeight + v;
                        newHeight = newHeight > maxHeight ? maxHeight : newHeight;
                        astarEntity.SetXYHeight(pos, newHeight);
                    }
                }
            }
        }

        void FixedUpdate() {
            if (!isRunning) return;
            var startPos = GetXY(start.position);
            var endPos = GetXY(end.position);
            walkableHeightDiffRange.X = walkableHeightDiffRangeX;
            walkableHeightDiffRange.Y = walkableHeightDiffRangeY;
            for (int i = 0; i < findTimes_eachFrame; i++) {
                if (needPathSmooth) path = astarEntity.FindSmoothPath(startPos, endPos, walkableHeightDiffRange, allowDiagonalMove);
                else path = astarEntity.FindPath(startPos, endPos, walkableHeightDiffRange, allowDiagonalMove);
            }
        }

        void OnDrawGizmos() {
            if (!isRunning) return;

            Gizmos.color = Color.black;
            DrawMapLine();
            DrawHeightMap();
            DrawPath();
        }

        void DrawPath() {
            if (path != null) {
                Gizmos.color = Color.green;
                for (int i = 0; i < path.Count - 1; i++) {
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

        void DrawHeightMap() {
            Color color = Gizmos.color;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < length; j++) {
                    var height = astarEntity.GetXYHeight(new Int2(i, j));
                    color.a = (float)height / maxHeight;
                    Gizmos.color = color;
                    if (height != 0) {
                        Gizmos.DrawCube(new Vector3(i + 0.5f, j + 0.5f), new Vector3(0.8f, 0.8f, 1f));
                    }
                }
            }
        }

        void DrawMapLine() {
            // Row
            for (int i = 0; i <= length; i++) {
                Gizmos.DrawLine(new Vector3(0, i, 0), new Vector3(width, i, 0));
            }
            // Column
            for (int i = 0; i <= width; i++) {
                Gizmos.DrawLine(new Vector3(i, 0, 0), new Vector3(i, length, 0));
            }
        }

        void OnGUI() {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            int pathNodeCount = path != null ? path.Count : 0;
            GUILayout.Label($"路径点个数:{pathNodeCount}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"下坡限制高度差:{walkableHeightDiffRangeX}");
            walkableHeightDiffRangeX = (int)GUILayout.HorizontalSlider(walkableHeightDiffRangeX, -10, 0, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"上坡限制高度差:{walkableHeightDiffRangeY}");
            walkableHeightDiffRangeY = (int)GUILayout.HorizontalSlider(walkableHeightDiffRangeY, 0, 10, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"最大高度:{maxHeight}");
            maxHeight = (int)GUILayout.HorizontalSlider(maxHeight, 0, 10, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"性能测试(每帧寻路次数):{findTimes_eachFrame}");
            findTimes_eachFrame = (int)GUILayout.HorizontalSlider(findTimes_eachFrame, 1, 500, GUILayout.Width(200));
            GUILayout.EndHorizontal();



            for (int i = 0; i < width; i++) {
                for (int j = 0; j < length; j++) {
                    var pos = new Vector3(i + 0.5f, j + 0.5f);
                    var height = astarEntity.GetXYHeight(new Int2(i, j));
                    Handles.Label(pos, height.ToString());
                }
            }
        }

        Int2 GetXY(Vector3 pos) {
            return new Int2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        }

    }

}
