using System;

namespace GameArki.PathFinding.Generic {

    public class AStarNode : IComparable<AStarNode> {

        public int f;
        public int g;
        public int h;
        public Int2 pos;
        public AStarNode parent;

        public int CompareTo(AStarNode x) {
            if (x.f > f) {
                return -1;
            } else if (x.f < f) {
                return 1;
            } else {
                return 0;
            }
        }

    }

}