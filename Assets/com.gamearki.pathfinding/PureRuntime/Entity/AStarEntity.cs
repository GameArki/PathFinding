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

        public List<Int2> FindPath(int startX, int startY, int endX, int endY, int walkableHeightDiff, bool allowDiagonalMove)
        {
            return AStarUtil.FindPath(heightMap, startX, startY, endX, endY, walkableHeightDiff, allowDiagonalMove);
        }

        public List<Int2> FindSmoothPath(int startX, int startY, int endX, int endY, int walkableHeightDiff, bool allowDiagonalMove)
        {
            var path = AStarUtil.FindPath(heightMap, startX, startY, endX, endY, walkableHeightDiff, allowDiagonalMove);
            if (path != null) path = AStarUtil.GetSmoothPath(heightMap, path, walkableHeightDiff);
            return path;
        }

        public void SetXYHeight(int x, int y, int height)
        {
            heightMap[x, y] = height;
        }

        public int GetXYHeight(int x, int y)
        {
            return heightMap[x, y];
        }

    }

}