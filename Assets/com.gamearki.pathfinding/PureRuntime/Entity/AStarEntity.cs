using System;
using System.Collections.Generic;
using GameArki.PathFinding.Generic;

namespace GameArki.PathFinding.AStar {

    public class AstarEntity {

        int width;
        public int Width => width;

        public int length;
        public int Length => length;

        int[,] heightMap;
        int capacity;

        Dictionary<int, bool> closedInfo;
        Dictionary<int, bool> opennedInfo;
        Heap<AStarNode> openList;
        ObjectPool<AStarNode> nodePool;

        static readonly int MOVE_DIAGONAL_COST = 141;
        static readonly int MOVE_STRAIGHT_COST = 100;

        public AstarEntity(int width, int length) {
            this.width = width;
            this.length = length;
            this.capacity = width * length;
            heightMap = new int[width, length];
            closedInfo = new Dictionary<int, bool>();
            opennedInfo = new Dictionary<int, bool>();
            openList = new Heap<AStarNode>(capacity);
            nodePool = new ObjectPool<AStarNode>(capacity);
        }

        public List<Int2> FindPath(in Int2 startPos, in Int2 endPos, in Int2 walkableHeightDiffRange, bool allowDiagonalMove) {
            closedInfo.Clear();
            opennedInfo.Clear();
            openList.Foreach((node) => {
                node.f = 0;
                node.g = 0;
                node.h = 0;
                node.pos = Int2.Zero;
                node.parent = null;
                nodePool.Enqueue(node);
            });
            openList.Clear();

            // 初始化起点和终点
            if (!nodePool.TryDequeue(out var startNode)) {
                startNode = new AStarNode();
            }
            startNode.pos = startPos;
            startNode.g = 0;
            startNode.h = GetManhattanDistance(startPos, endPos);
            startNode.f = 0;

            if (!nodePool.TryDequeue(out var endNode)) {
                endNode = new AStarNode();
            }
            endNode.pos = endPos;

            // 创建开启列表和关闭列表

            if (!IsInBoundary(startNode.pos)) {
                return null;
            }
            if (!IsInBoundary(endNode.pos)) {
                return null;
            }

            // 将起点添加到开启列表中
            openList.Push(startNode);
            AStarNode currentNode = startNode;

            while (openList.Count > 0) {
                // 找到开启列表中F值
                currentNode = GetLowestFNode(openList, endNode);
                var curNodePos = currentNode.pos;
                var endNodePos = endNode.pos;

                // 从开启列表中移除当前节点，并将其添加到关闭列表中
                opennedInfo[curNodePos.X + curNodePos.Y * width] = false;
                closedInfo.Add(curNodePos.X + curNodePos.Y * width, true);

                // 如果当前节点为终点，则找到了最短路径
                if (curNodePos.ValueEquals(endNodePos)) {
                    // 使用栈来保存路径
                    Stack<AStarNode> pathStack = new Stack<AStarNode>();
                    while (currentNode != null) {
                        pathStack.Push(currentNode);
                        currentNode = currentNode.parent;
                    }
                    List<Int2> path = new List<Int2>(pathStack.Count);
                    while (pathStack.TryPop(out var node)) {
                        path.Add(node.pos);
                    }
                    return path;
                }

                // 获取当前节点的周围节点
                List<AStarNode> neighbours = GetWalkableNeighbours(currentNode, walkableHeightDiffRange, allowDiagonalMove);
                for (int i = 0; i < neighbours.Count; i++) {
                    var neighbour = neighbours[i];
                    var neighbourPos = neighbour.pos;
                    // 计算新的G值
                    var g_offset = GetDistance(currentNode, neighbour, allowDiagonalMove);
                    int newG = currentNode.g + g_offset;

                    // 如果新的G值比原来的G值小,计算新的F值 
                    bool neighbourExits = opennedInfo.TryGetValue(neighbourPos.X + neighbourPos.Y * width, out var flag) && flag;
                    if (!neighbourExits || newG < neighbour.g) {
                        neighbour.g = newG;
                        neighbour.h = GetDistance(neighbour, endNode, allowDiagonalMove);
                        neighbour.f = neighbour.g + neighbour.h;
                        neighbour.parent = currentNode;
                    }

                    // 如果节点不在开启列表中，则将其添加到开启列表中 
                    if (!neighbourExits) {
                        openList.Push(neighbour);
                        opennedInfo.Add(neighbourPos.X + neighbourPos.Y * width, true);
                    }
                }

            }

            // 如果开启列表为空，则无法找到路径
            return null;

        }

        public List<Int2> FindSmoothPath(in Int2 startPos, in Int2 endPos, in Int2 walkableHeightDiffRange, bool allowDiagonalMove) {
            var path = FindPath(startPos, endPos, walkableHeightDiffRange, allowDiagonalMove);
            if (path != null) path = GetSmoothPath(path, walkableHeightDiffRange);
            return path;
        }

        public void SetXYHeight(in Int2 pos, int height) {
            heightMap[pos.X, pos.Y] = height;
        }

        public int GetXYHeight(in Int2 pos) {
            return heightMap[pos.X, pos.Y];
        }

        AStarNode GetLowestFNode(Heap<AStarNode> openList, AStarNode endNode) {
            return openList.Pop();
        }

        List<AStarNode> GetWalkableNeighbours(AStarNode currentNode, in Int2 walkableHeightDiffRange, bool allowDiagonalMove) {
            List<AStarNode> neighbours = new List<AStarNode>();
            // 获取当前节点的位置
            var fromPos = currentNode.pos;
            int x = fromPos.X;
            int y = fromPos.Y;

            // 获取四周的节点
            Int2 topPos = new Int2(x, y + 1);
            if (IsWalkableNeighbour(topPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = topPos });
            Int2 bottomPos = new Int2(x, y - 1);
            if (IsWalkableNeighbour(bottomPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = bottomPos });
            Int2 leftPos = new Int2(x - 1, y);
            if (IsWalkableNeighbour(leftPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = leftPos });
            Int2 rightPos = new Int2(x + 1, y);
            if (IsWalkableNeighbour(rightPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = rightPos });

            if (allowDiagonalMove) {
                Int2 top_leftPos = new Int2(x - 1, y + 1);
                if (IsWalkableNeighbour(top_leftPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = top_leftPos });
                Int2 bottom_leftPos = new Int2(x - 1, y - 1);
                if (IsWalkableNeighbour(bottom_leftPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = bottom_leftPos });
                Int2 top_rightPos = new Int2(x + 1, y + 1);
                if (IsWalkableNeighbour(top_rightPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = top_rightPos });
                Int2 bottom_rightPos = new Int2(x + 1, y - 1);
                if (IsWalkableNeighbour(bottom_rightPos, fromPos, walkableHeightDiffRange)) neighbours.Add(new AStarNode() { pos = bottom_rightPos });
            }

            return neighbours;
        }

        int GetDistance(AStarNode node1, AStarNode node2, bool allowDiagonalMove) {
            var pos1 = node1.pos;
            var pos2 = node2.pos;
            if (allowDiagonalMove) {
                int xDistance = Math.Abs(pos1.X - pos2.X);
                int yDistance = Math.Abs(pos1.Y - pos2.Y);
                int remaining = Math.Abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            } else {
                return GetManhattanDistance(pos1, pos2);
            }
        }

        int GetManhattanDistance(in Int2 pos1, in Int2 pos2) {
            return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
        }

        bool IsWalkableNeighbour(in Int2 tarPos, in Int2 fromPos, in Int2 walkableHeightDiffRange) {
            if (!IsCanReach(tarPos, fromPos, walkableHeightDiffRange)) return false;
            if (closedInfo.TryGetValue(tarPos.X + tarPos.Y * heightMap.GetLength(0), out var flag) && flag) return false;
            return true;
        }

        bool IsInBoundary(in Int2 pos) {
            var x = pos.X;
            var y = pos.Y;
            var width = heightMap.GetLength(0);
            var length = heightMap.GetLength(1);
            if (x >= width || x < 0 || y >= length || y < 0) {
                return false;
            }

            return true;
        }

        bool IsWalkable(in Int2 tarPos, in Int2 fromPos, in Int2 walkableHeightDiffRange) {
            var hDiff = heightMap[tarPos.X, tarPos.Y] - heightMap[fromPos.X, fromPos.Y];
            return hDiff <= walkableHeightDiffRange.Y && hDiff >= walkableHeightDiffRange.X;
        }

        bool IsCanReach(in Int2 tarPos, in Int2 fromPos, in Int2 walkableHeightDiffRange) {
            return IsInBoundary(tarPos) && IsWalkable(tarPos, fromPos, walkableHeightDiffRange);
        }


        #region [路径点优化]

        public List<Int2> GetSmoothPath(List<Int2> path, in Int2 walkableHeightDiffRange) {
            List<Int2> smoothPath = new List<Int2>(path.Count);
            var pos1 = path[0];
            var pos2 = path[1];
            smoothPath.Add(pos1);
            smoothPath.Add(pos2);
            for (int i = 2; i < path.Count; i++) {
                var curPos1 = path[i - 1];
                var curPos2 = path[i];
                if (IsSlopeEqual(pos1, pos2, curPos1, curPos2)) {
                    // 斜率相同为同一条直线路径上,则可去除期间多余路径点
                    // 更新节点
                    pos2 = curPos2;
                    smoothPath.RemoveAt(smoothPath.Count - 1);
                    smoothPath.Add(curPos2);
                } else if (CanGoStraight(pos1, curPos2, walkableHeightDiffRange)) {
                    // 斜率不相同且可以直达,则可去除期间多余路径点
                    // 更新节点
                    pos2 = curPos2;
                    smoothPath.RemoveAt(smoothPath.Count - 1);
                    smoothPath.Add(curPos2);
                } else {
                    // 更新节点
                    pos1 = curPos1;
                    pos2 = curPos2;
                    smoothPath.Add(curPos2);
                }
            }

            return smoothPath;
        }

        bool CanGoStraight(in Int2 startPos, in Int2 endPos, in Int2 walkableHeightDiffRange) {
            bool flag;
            bool flag1;
            bool flag2;
            int k_son = startPos.Y - endPos.Y;
            int k_mom = startPos.X - endPos.X;
            bool isPositive = (k_son > 0 && k_mom > 0) || (k_son < 0 && k_mom < 0);

            flag = IsP2PWalkable(startPos, endPos, startPos, walkableHeightDiffRange);
            if (isPositive) {
                Int2 p1 = new Int2();
                p1.X = startPos.X;
                p1.Y = startPos.Y + 1;
                Int2 p2 = new Int2();
                p2.X = endPos.X;
                p2.Y = endPos.Y + 1;

                flag1 = IsP2PWalkable(p1, p2, startPos, walkableHeightDiffRange);
                Int2 p3 = new Int2(startPos.X + 1, startPos.Y);
                Int2 p4 = new Int2(endPos.X + 1, endPos.Y);
                flag2 = IsP2PWalkable(p3, p4, startPos, walkableHeightDiffRange);
                return flag && flag1 && flag2;
            } else {
                Int2 p1 = new Int2(startPos.X + 1, startPos.Y + 1);
                Int2 p2 = new Int2(endPos.X + 1, endPos.Y + 1);
                flag1 = IsP2PWalkable(p1, p2, startPos, walkableHeightDiffRange);
                Int2 p3 = new Int2(startPos.X + 1, startPos.Y);
                Int2 p4 = new Int2(endPos.X + 1, endPos.Y);
                flag2 = IsP2PWalkable(p3, p4, startPos, walkableHeightDiffRange);
                return flag && flag1 && flag2;
            }

        }

        bool IsSlopeEqual(Int2 pos1, Int2 pos2, Int2 pos3, Int2 pos4) {
            int k1_son = (pos2.Y - pos1.Y);
            int k1_mom = (pos2.X - pos1.X);
            int k2_son = (pos4.Y - pos3.Y);
            int k2_mom = (pos4.X - pos3.X);
            if (k1_son == 0 && k1_mom == 0) return false;
            if (k2_son == 0 && k2_mom == 0) return false;
            return k1_son * k2_mom == k2_son * k1_mom;
        }

        bool IsP2PWalkable(Int2 p1, Int2 p2, Int2 startPos, in Int2 walkableHeightDiffRange) {
            if (p1.ValueEquals(p2)) return true;

            // 保证顺序
            if (p1.X > p2.X) {
                Int2 tempPos = p1;
                p1 = p2;
                p2 = tempPos;
            }
            if (p1.X == p2.X && p1.Y > p2.Y) {
                Int2 tempPos = p1;
                p1 = p2;
                p2 = tempPos;
            }

            int x1, y1, x2, y2, A, B;
            x1 = p1.X;
            y1 = p1.Y;
            x2 = p2.X;
            y2 = p2.Y;

            Int2 currentPos = p1;
            bool isXSame = x1 == x2;
            bool isYSame = y1 == y2;
            if (isXSame) {
                while (!currentPos.ValueEquals(p2)) {
                    var fromPos = currentPos;
                    currentPos += new Int2(0, 1);
                    if (!IsCanReach(currentPos, fromPos, walkableHeightDiffRange)) return false;
                    currentPos += new Int2(0, 1);
                }
            } else if (isYSame) {
                while (!currentPos.ValueEquals(p2)) {
                    var fromPos = currentPos;
                    currentPos += new Int2(1, 0);
                    if (!IsCanReach(currentPos, fromPos, walkableHeightDiffRange)) return false;
                    currentPos += new Int2(1, 0);
                }
            } else {
                A = y1 - y2;
                B = x2 - x1;
                int d = 0;
                int d1 = 0;
                int d2 = 0;
                Int2 posAdd1 = new Int2();
                Int2 posAdd2 = new Int2();
                var a_abs = Math.Abs(A);
                var b_abs = Math.Abs(B);
                bool isSlopeABSBiggerThanOne = a_abs > b_abs;
                bool isSlopeBiggerThanZero = (A > 0 && -B > 0) || (A < 0 && -B < 0);

                var fromPos = startPos;
                //斜率为正，则不移动，斜率为负，则向下移动一个位置
                Int2 offset = new Int2(0, isSlopeBiggerThanZero ? 0 : -1);
                currentPos += offset;
                p2 += offset;
                if (!IsCanReach(currentPos, fromPos, walkableHeightDiffRange)) {
                    return false;
                }

                // 计算d d1 d2
                if (isSlopeBiggerThanZero) {
                    d = A + B;
                    d2 = A + B;
                } else {
                    d = A - B;
                    d2 = A - B;
                }
                d1 = A;
                if (isSlopeABSBiggerThanOne) d1 = isSlopeBiggerThanZero ? B : -B;
                d = isSlopeBiggerThanZero ? (A + B) : A - B;
                d2 = d;

                // 计算 posAdd1 posAdd2
                if (!isSlopeABSBiggerThanOne) {
                    posAdd1 = new Int2(1, 0);
                    posAdd2 = new Int2(1, isSlopeBiggerThanZero ? 1 : -1);
                } else {
                    //斜率大于1
                    posAdd1 = new Int2(0, isSlopeBiggerThanZero ? 1 : -1);
                    posAdd2 = new Int2(1, isSlopeBiggerThanZero ? 1 : -1);
                }

                // 遍历路经过的点
                while (!currentPos.ValueEquals(p2)) {
                    bool isXYBothChange;
                    if (!isSlopeABSBiggerThanOne) {
                        isXYBothChange = (isSlopeBiggerThanZero && d <= 0) || (!isSlopeBiggerThanZero && d >= 0);
                    } else {
                        //斜率大于1
                        isXYBothChange = (isSlopeBiggerThanZero && d >= 0) || (!isSlopeBiggerThanZero && d <= 0);
                    }

                    if (isXYBothChange) {
                        currentPos += posAdd2;
                        d += d2;
                    } else {
                        currentPos += posAdd1;
                        d += d1;
                    }

                    if (currentPos.ValueEquals(p2)) {
                        return true;
                    }

                    if (!IsCanReach(currentPos, fromPos, walkableHeightDiffRange)) {
                        return false;
                    }

                }

                if (currentPos.Y != p2.Y) return false;
            }

            return true;
        }

        #endregion


    }

}