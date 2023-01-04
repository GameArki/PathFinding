using System;
using System.Collections.Generic;
using System.Linq;
using GameArki.PathFinding.Generic;

namespace GameArki.PathFinding.AStar
{

    public static class AStarUtil
    {

        static readonly int MOVE_DIAGONAL_COST = 141;
        static readonly int MOVE_STRAIGHT_COST = 100;

        public static List<AStarNode> FindPath(byte[,] map, int startX, int startY, int endX, int endY, bool allowDiagonalMove)
        {

            // 初始化起点和终点
            AStarNode startNode = new AStarNode()
            {
                pos = new Int2(startX, startY),
                G = 0,
                H = GetManhattanDistance(new Int2(startX, startY), new Int2(endX, endY)),
                F = 0
            };
            AStarNode endNode = new AStarNode()
            {
                pos = new Int2(endX, endY),
            };

            // 创建开启列表和关闭列表
            var lenX = map.GetLength(0);
            var lenY = map.GetLength(1);
            List<AStarNode> openList = new List<AStarNode>();
            byte[,] closeList = new byte[lenX, lenY];
            byte[,] openListInfo = new byte[lenX, lenY];

            if (!IsInBoundary(map, startNode))
            {
                return null;
            }
            if (!IsInBoundary(map, endNode))
            {
                return null;
            }

            // 将起点添加到开启列表中
            openList.Add(startNode);
            AStarNode currentNode = startNode;

            int count = 0;
            while (openList.Count > 0)
            {
                count++;
                if (count > 1000) return null;

                // 找到开启列表中F值
                currentNode = GetLowestFNode(openList, endNode);
                var curNodePos = currentNode.pos;
                var endNodePos = endNode.pos;

                // 从开启列表中移除当前节点，并将其添加到关闭列表中
                openList.Remove(currentNode);
                openListInfo[curNodePos.X, curNodePos.Y] = 0;
                closeList[curNodePos.X, curNodePos.Y] = 1;

                // 如果当前节点为终点，则找到了最短路径
                if (curNodePos.ValueEquals(endNodePos))
                {
                    // 使用栈来保存路径
                    Stack<AStarNode> path = new Stack<AStarNode>();
                    while (currentNode != null)
                    {
                        path.Push(currentNode);
                        currentNode = currentNode.Parent;
                    }
                    return path.ToList();
                }

                // 获取当前节点的周围节点
                List<AStarNode> neighbours = GetRealNeighbours(map, currentNode, closeList, allowDiagonalMove);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    var neighbour = neighbours[i];
                    var neighbourPos = neighbour.pos;
                    // 计算新的G值
                    var g_offset = GetDistance(currentNode, neighbour, allowDiagonalMove);
                    int newG = currentNode.G + g_offset;

                    // 如果新的G值比原来的G值小,计算新的F值 
                    if (openListInfo[neighbourPos.X, neighbourPos.Y] == 0 || newG < neighbour.G)
                    {
                        neighbour.G = newG;
                        neighbour.H = GetDistance(neighbour, endNode, allowDiagonalMove);
                        neighbour.F = neighbour.G + neighbour.H;
                        neighbour.Parent = currentNode;
                    }

                    // 如果节点不在开启列表中，则将其添加到开启列表中 
                    if (openListInfo[neighbourPos.X, neighbourPos.Y] == 0)
                    {
                        openList.Add(neighbour);
                        openListInfo[neighbourPos.X, neighbourPos.Y] = 1;
                    }
                }

            }

            // 如果开启列表为空，则无法找到路径
            return null;
        }

        static AStarNode GetLowestFNode(List<AStarNode> openList, AStarNode endNode)
        {
            AStarNode lowestFNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                var node = openList[i];
                if (node.F < lowestFNode.F)
                {
                    lowestFNode = node;
                }
            }

            return lowestFNode;
        }

        static List<AStarNode> GetRealNeighbours(byte[,] map, AStarNode currentNode, byte[,] closeList, bool allowDiagonalMove)
        {
            List<AStarNode> neighbours = new List<AStarNode>();
            // 获取当前节点的位置
            var curPos = currentNode.pos;
            int x = curPos.X;
            int y = curPos.Y;

            // 获取四周的节点
            Int2 topPos = new Int2(x, y + 1);
            if (IsRealNeighbour(map, topPos, closeList)) neighbours.Add(new AStarNode() { pos = topPos });
            Int2 bottomPos = new Int2(x, y - 1);
            if (IsRealNeighbour(map, bottomPos, closeList)) neighbours.Add(new AStarNode() { pos = bottomPos });
            Int2 leftPos = new Int2(x - 1, y);
            if (IsRealNeighbour(map, leftPos, closeList)) neighbours.Add(new AStarNode() { pos = leftPos });
            Int2 rightPos = new Int2(x + 1, y);
            if (IsRealNeighbour(map, rightPos, closeList)) neighbours.Add(new AStarNode() { pos = rightPos });

            if (allowDiagonalMove)
            {
                Int2 top_leftPos = new Int2(x - 1, y + 1);
                if (IsRealNeighbour(map, top_leftPos, closeList)) neighbours.Add(new AStarNode() { pos = top_leftPos });
                Int2 bottom_leftPos = new Int2(x - 1, y - 1);
                if (IsRealNeighbour(map, bottom_leftPos, closeList)) neighbours.Add(new AStarNode() { pos = bottom_leftPos });
                Int2 top_rightPos = new Int2(x + 1, y + 1);
                if (IsRealNeighbour(map, top_rightPos, closeList)) neighbours.Add(new AStarNode() { pos = top_rightPos });
                Int2 bottom_rightPos = new Int2(x + 1, y - 1);
                if (IsRealNeighbour(map, bottom_rightPos, closeList)) neighbours.Add(new AStarNode() { pos = bottom_rightPos });
            }

            return neighbours;
        }

        static int GetDistance(AStarNode node1, AStarNode node2, bool allowDiagonalMove)
        {
            var pos1 = node1.pos;
            var pos2 = node2.pos;
            if (allowDiagonalMove)
            {
                int xDistance = Math.Abs(pos1.X - pos2.X);
                int yDistance = Math.Abs(pos1.Y - pos2.Y);
                int remaining = Math.Abs(xDistance - yDistance);
                return MOVE_DIAGONAL_COST * Math.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
            else
            {
                return GetManhattanDistance(node1, node2);
            }
        }

        static int GetManhattanDistance(AStarNode node1, AStarNode node2)
        {
            return GetManhattanDistance(node1.pos, node2.pos);
        }

        static int GetManhattanDistance(Int2 pos1, Int2 pos2)
        {
            return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
        }

        static bool IsRealNeighbour(byte[,] map, Int2 pos, byte[,] closeList)
        {
            if (!IsInBoundary(map, pos)) return false;

            var x = pos.X;
            var y = pos.Y;
            if (map[x, y] != 0)
            {
                return false;
            }
            if (closeList[x, y] != 0)
            {
                return false;
            }

            return true;
        }

        static bool IsInBoundary(byte[,] map, AStarNode node)
        {
            return IsInBoundary(map, node.pos);
        }

        static bool IsInBoundary(byte[,] map, Int2 pos)
        {
            var x = pos.X;
            var y = pos.Y;
            var lenX = map.GetLength(0);
            var lenY = map.GetLength(1);
            if (x >= lenX || x < 0 || y >= lenY || y < 0)
            {
                return false;
            }

            return true;
        }

        static bool IsWalkable(byte[,] map, Int2 pos)
        {
            return map[pos.X, pos.Y] == 0;
        }

        static bool IsCanReach(byte[,] map, Int2 pos)
        {
            return IsInBoundary(map, pos) && IsWalkable(map, pos);
        }

        #region [路径点优化]

        public static List<AStarNode> GetSmoothPath(byte[,] map, List<AStarNode> path)
        {
            List<AStarNode> smoothPath = new List<AStarNode>(path.Count);
            var node1 = path[0];
            var node2 = path[1];
            var pos1 = node1.pos;
            var pos2 = node2.pos;
            smoothPath.Add(node1);
            smoothPath.Add(node2);
            for (int i = 2; i < path.Count; i++)
            {
                var curNode1 = path[i - 1];
                var curNode2 = path[i];
                var curPos1 = curNode1.pos;
                var curPos2 = curNode2.pos;
                if (IsSlopeEqual(pos1, pos2, curPos1, curPos2))
                {
                    // 斜率相同为同一条直线路径上,则可去除期间多余路径点
                    // 更新节点
                    pos2 = curPos2;
                    smoothPath.RemoveAt(smoothPath.Count - 1);
                    smoothPath.Add(curNode2);
                }
                else if (CanGoStraight(map, pos1, curPos2))
                {
                    // 斜率不相同且可以直达,则可去除期间多余路径点
                    // 更新节点
                    pos2 = curPos2;
                    smoothPath.RemoveAt(smoothPath.Count - 1);
                    smoothPath.Add(curNode2);
                }
                else
                {
                    // 更新节点
                    pos1 = curPos1;
                    pos2 = curPos2;
                    smoothPath.Add(curNode2);
                }
            }

            return smoothPath;
        }

        static bool CanGoStraight(byte[,] map, Int2 startNode, Int2 endNode)
        {
            bool flag;
            bool flag1;
            bool flag2;
            int k_son = startNode.Y - endNode.Y;
            int k_mom = startNode.X - endNode.X;
            bool isPositive = (k_son > 0 && k_mom > 0) || (k_son < 0 && k_mom < 0);

            flag = IsP2PWalkable(map, startNode, endNode, isPositive);
            if (isPositive)
            {
                Int2 startNode1 = new Int2();
                startNode1.X = startNode.X;
                startNode1.Y = startNode.Y + 1;
                Int2 endNode1 = new Int2();
                endNode1.X = endNode.X;
                endNode1.Y = endNode.Y + 1;

                flag1 = IsP2PWalkable(map, startNode1, endNode1, false);
                Int2 startNode2 = new Int2(startNode.X + 1, startNode.Y);
                Int2 endNode2 = new Int2(endNode.X + 1, endNode.Y);
                flag2 = IsP2PWalkable(map, startNode2, endNode2, false);
                return flag && flag1 && flag2;
            }
            else
            {
                Int2 startNode1 = new Int2(startNode.X + 1, startNode.Y + 1);
                Int2 endNode1 = new Int2(endNode.X + 1, endNode.Y + 1);
                flag1 = IsP2PWalkable(map, startNode1, endNode1, false);
                Int2 startNode2 = new Int2(startNode.X + 1, startNode.Y);
                Int2 endNode2 = new Int2(endNode.X + 1, endNode.Y);
                flag2 = IsP2PWalkable(map, startNode2, endNode2, true);
                return flag && flag1 && flag2;
            }

        }

        static bool IsSlopeEqual(Int2 pos1, Int2 pos2, Int2 pos3, Int2 pos4)
        {
            int k1_son = (pos2.Y - pos1.Y);
            int k1_mom = (pos2.X - pos1.X);
            int k2_son = (pos4.Y - pos3.Y);
            int k2_mom = (pos4.X - pos3.X);
            if (k1_son == 0 && k1_mom == 0) return false;
            if (k2_son == 0 && k2_mom == 0) return false;
            return k1_son * k2_mom == k2_son * k1_mom;
        }

        static bool IsP2PWalkable(byte[,] map, Int2 startPos, Int2 endPos, bool isMiddle)
        {
            if (isMiddle && !IsWalkable(map, endPos))
            {
                return false;
            }
            if (startPos.ValueEquals(endPos)) return true;

            // 保证顺序
            if (startPos.X > endPos.X)
            {
                Int2 tempPos = startPos;
                startPos = endPos;
                endPos = tempPos;
            }
            if (startPos.X == endPos.X && startPos.Y > endPos.Y)
            {
                Int2 tempPos = startPos;
                startPos = endPos;
                endPos = tempPos;
            }

            int x1, y1, x2, y2, A, B;
            x1 = startPos.X;
            y1 = startPos.Y;
            x2 = endPos.X;
            y2 = endPos.Y;

            Int2 currentPos = startPos;
            bool isXSame = x1 == x2;
            bool isYSame = y1 == y2;
            if (isXSame)
            {
                currentPos += new Int2(0, 1);
                while (!currentPos.ValueEquals(endPos))
                {
                    if (!IsCanReach(map, currentPos)) return false;
                    currentPos += new Int2(0, 1);
                }
            }
            else if (isYSame)
            {
                currentPos += new Int2(1, 0);
                while (!currentPos.ValueEquals(endPos))
                {
                    if (!IsCanReach(map, currentPos)) return false;
                    currentPos += new Int2(1, 0);
                }
            }
            else
            {
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

                //斜率为正，则不移动，斜率为负，则向下移动一个位置
                Int2 offset = new Int2(0, isSlopeBiggerThanZero ? 0 : -1);
                currentPos += offset;
                endPos += offset;
                if (!IsCanReach(map, currentPos))
                {
                    return false;
                }

                // 计算d d1 d2
                if (isSlopeBiggerThanZero)
                {
                    d = A + B;
                    d2 = A + B;
                }
                else
                {
                    d = A - B;
                    d2 = A - B;
                }
                d1 = A;
                if (isSlopeABSBiggerThanOne) d1 = isSlopeBiggerThanZero ? B : -B;
                d = isSlopeBiggerThanZero ? (A + B) : A - B;
                d2 = d;

                // 计算 posAdd1 posAdd2
                if (!isSlopeABSBiggerThanOne)
                {
                    posAdd1 = new Int2(1, 0);
                    posAdd2 = new Int2(1, isSlopeBiggerThanZero ? 1 : -1);
                }
                else
                {
                    //斜率大于1
                    posAdd1 = new Int2(0, isSlopeBiggerThanZero ? 1 : -1);
                    posAdd2 = new Int2(1, isSlopeBiggerThanZero ? 1 : -1);
                }

                // 遍历路经过的点
                while (!currentPos.ValueEquals(endPos))
                {
                    bool isXYBothChange;
                    if (!isSlopeABSBiggerThanOne)
                    {
                        isXYBothChange = (isSlopeBiggerThanZero && d <= 0) || (!isSlopeBiggerThanZero && d >= 0);
                    }
                    else
                    {
                        //斜率大于1
                        isXYBothChange = (isSlopeBiggerThanZero && d >= 0) || (!isSlopeBiggerThanZero && d <= 0);
                    }

                    if (isXYBothChange)
                    {
                        currentPos += posAdd2;
                        d += d2;
                    }
                    else
                    {
                        currentPos += posAdd1;
                        d += d1;
                    }

                    if (currentPos.ValueEquals(endPos))
                    {
                        return true;
                    }

                    if (!IsCanReach(map, currentPos))
                    {
                        return false;
                    }

                }

                if (currentPos.Y != endPos.Y) return false;
            }

            return true;
        }

        #endregion

    }

}

