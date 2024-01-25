// @ 2022 Lukas Chodosevicius

using System;

namespace ProjectDawn.LocalAvoidance
{
    /// <summary>
    /// One dimension line segment.
    /// </summary>
    [Serializable]
    public struct Line
    {
        /// <summary>
        /// Position where segment starts. Must be lower than <see cref="Line.To"/>.
        /// </summary>
        public float From;
        /// <summary>
        /// Position where segment ends. Must be greated than <see cref="Line.From"/>.
        /// </summary>
        public float To;

        /// <summary>
        /// Length of segment from start to end.
        /// </summary>
        public float Length => To - From;

        public static bool operator==(Line lhs, Line rhs)
        {
            return lhs.From == rhs.From && lhs.To == rhs.To;
        }

        public static bool operator!=(Line lhs, Line rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(Line other)
        {
            return this == other;
        }

        public override bool Equals(object compare)
        {
            return compare is Line compareNode && Equals(compareNode);
        }

        public override int GetHashCode()
        {
            return (int)(From + To);
        }

        public Line(float from, float to)
        {
            if (from > to)
                throw new ArgumentException("Expected from < to, actual from > to.", "from and to");

            this.From = from;
            this.To = to;
        }

        public struct CutResult
        {
            public int SegmentCount;
            public Line Segment0;
            public Line Segment1;
        }

        public static bool CutLine(Line lhs, Line rhs, out CutResult result)
        {
            // lhs: ------------
            // rhs:   --------
            if (lhs.From <= rhs.From && rhs.From <= lhs.To &&
                lhs.From <= rhs.To && rhs.To <= lhs.To)
            {
                result = new CutResult
                {
                    SegmentCount = 2,
                    Segment0 = new Line(lhs.From, rhs.From),
                    Segment1 = new Line(rhs.To, lhs.To),
                };
                return true;
            }

            // lhs: --------
            // rhs:   --------
            if (lhs.From <= rhs.From && rhs.From <= lhs.To &&
                lhs.To <= rhs.To)
            {
                result = new CutResult
                {
                    SegmentCount = 1,
                    Segment0 = new Line(lhs.From, rhs.From),
                };
                return true;
            }

            // lhs:      -------
            // rhs:   --------
            if (rhs.From <= lhs.From &&
                lhs.From <= rhs.To && rhs.To <= lhs.To)
            {
                result = new CutResult
                {
                    SegmentCount = 1,
                    Segment0 = new Line(rhs.To, lhs.To),
                };
                return true;
            }

            // lhs:   --------
            // rhs: ------------
            if (rhs.From <= lhs.From &&
                lhs.To <= rhs.To)
            {
                result = new CutResult();
                return true;
            }

            result = new CutResult();
            return false;
        }
    }
}
