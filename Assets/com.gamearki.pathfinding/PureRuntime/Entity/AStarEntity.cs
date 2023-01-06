using System;
using System.Collections.Generic;
using GameArki.PathFinding.Generic;

namespace GameArki.PathFinding.AStar
{

    public class AstarEntity
    {

        int width;
        public int Width => width;

        public int length;
        public int Length => length;

        int[,] heightMap;

        public AstarEntity(int width, int length)
        {
            this.width = width;
            this.length = length;
            heightMap = new int[width, length];
        }

        public List<Int2> FindPath(in Int2 startPos, in Int2 endPos, in Int2 walkableHeightDiffRange, bool allowDiagonalMove)
        {
            return AStarUtil.FindPath(heightMap, startPos, endPos, walkableHeightDiffRange, allowDiagonalMove);
        }

        public List<Int2> FindSmoothPath(in Int2 startPos, in Int2 endPos, in Int2 walkableHeightDiffRange, bool allowDiagonalMove)
        {
            var path = AStarUtil.FindPath(heightMap, startPos, endPos, walkableHeightDiffRange, allowDiagonalMove);
            if (path != null) path = AStarUtil.GetSmoothPath(heightMap, path, walkableHeightDiffRange);
            return path;
        }

        public void SetXYHeight(in Int2 pos, int height)
        {
            heightMap[pos.X, pos.Y] = height;
        }

        public int GetXYHeight(in Int2 pos)
        {
            return heightMap[pos.X, pos.Y];
        }

    }

}