using System;

namespace GameArki.PathFinding.Generic {

    public class AStarNode : IComparable<AStarNode> {

        public int F;
        public int G;
        public int H;
        public Int2 pos;
        public AStarNode Parent;

        public int CompareTo(AStarNode x) {
            if (x.F > F) {
                return -1;
            } else if (x.F < F) {
                return 1;
            } else {
                return 0;
            }
        }

    }

}