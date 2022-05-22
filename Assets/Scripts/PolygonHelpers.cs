// Code from: https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/

#pragma warning disable IDE0090 // "Simplify new expression" - implicit object creation is not supported in the .NET version used by Unity 2020.3

using System.Collections.Generic;
using UnityEngine;

namespace TT
{
    public static class PolygonHelpers
    {

        #region Private fields

        // Define Infinite (Using INT_MAX
        // caused overflow problems)
        private static int INF = 1000;

        #endregion


        // Given three collinear points p, q, r,
        // the function checks if point q lies
        // on line segment 'pr'
        private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.x <= Mathf.Max(p.x, r.x) &&
                q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) &&
                q.y >= Mathf.Min(p.y, r.y))
            {
                return true;
            }
            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            int val = (int)((q.y - p.y) * (r.x - q.x) -
                    (q.x - p.x) * (r.y - q.y));

            if (val == 0)
            {
                return 0; // collinear
            }
            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The function that returns true if
        // line segment 'p1q1' and 'p2q2' intersect.
        private static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            // Find the four orientations needed for
            // general and special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // Special Cases
            // p1, q1 and p2 are collinear and
            // p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1))
            {
                return true;
            }

            // p1, q1 and p2 are collinear and
            // q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1))
            {
                return true;
            }

            // p2, q2 and p1 are collinear and
            // p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2))
            {
                return true;
            }

            // p2, q2 and q1 are collinear and
            // q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2))
            {
                return true;
            }

            // Doesn't fall in any of the above cases
            return false;
        }

        // Returns true if the point p lies
        // inside the polygon[] with n vertices
        public static bool IsPointInside(Vector2[] polygon, int n, Vector2 p)
        {
            // There must be at least 3 vertices in polygon[]
            if (n < 3)
            {
                return false;
            }

            // Create a point for line segment from p to infinite
            Vector2 extreme = new Vector2(INF, p.y);

            // Count intersections of the above line
            // with sides of polygon
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to
                // 'extreme' intersects with the line
                // segment from 'polygon[i]' to 'polygon[next]'
                if (DoIntersect(polygon[i],
                                polygon[next], p, extreme))
                {
                    // If the point 'p' is collinear with line
                    // segment 'i-next', then check if it lies
                    // on segment. If it lies, return true, otherwise false
                    if (Orientation(polygon[i], p, polygon[next]) == 0)
                    {
                        return OnSegment(polygon[i], p,
                                        polygon[next]);
                    }
                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise
            return (count % 2 == 1); // Same as (count%2 == 1)
        }


        /// <summary>
        /// This script can be used to split a 2D polygon into triangles.
        /// The algorithm supports concave polygons, but not polygons with holes, 
        /// or multiple polygons at once.
        /// Taken from http://wiki.unity3d.com/index.php?title=Triangulator
        /// Found at: https://medium.com/@hyperparticle/draw-2d-physics-shapes-in-unity3d-2e0ec634381c
        /// </summary>

        public static int[] Triangulate(List<Vector2> points)
        {
            var indices = new List<int>();

            var n = points.Count;
            if (n < 3)
                return indices.ToArray();

            var vArray = new int[n];
            if (Area(points) > 0)
            {
                for (var v = 0; v < n; v++)
                    vArray[v] = v;
            }
            else
            {
                for (var v = 0; v < n; v++)
                    vArray[v] = n - 1 - v;
            }

            var nv = n;
            var count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if (count-- <= 0)
                    return indices.ToArray();

                var u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                var w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(points, u, v, w, nv, vArray))
                {
                    int a, b, c, s, t;
                    a = vArray[u];
                    b = vArray[v];
                    c = vArray[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        vArray[s] = vArray[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float Area(List<Vector2> points)
        {
            var n = points.Count;
            var a = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                var pval = points[p];
                var qval = points[q];
                a += pval.x * qval.y - qval.x * pval.y;
            }
            return a * 0.5f;
        }

        private static bool Snip(List<Vector2> points, int u, int v, int w, int n, int[] vArray)
        {
            int p;
            var a = points[vArray[u]];
            var b = points[vArray[v]];
            var c = points[vArray[w]];
            if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
                return false;
            for (p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                    continue;
                var pArray = points[vArray[p]];
                if (InsideTriangle(a, b, c, pArray))
                    return false;
            }
            return true;
        }

        private static bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = c.x - b.x;
            ay = c.y - b.y;
            bx = a.x - c.x;
            by = a.y - c.y;
            cx = b.x - a.x;
            cy = b.y - a.y;
            apx = p.x - a.x;
            apy = p.y - a.y;
            bpx = p.x - b.x;
            bpy = p.y - b.y;
            cpx = p.x - c.x;
            cpy = p.y - c.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return aCROSSbp >= 0.0f && bCROSScp >= 0.0f && cCROSSap >= 0.0f;
        }


        /// <summary>
        /// Returns the surface area of a polygon.
        /// </summary>
        /// <param name="points">A list of points that make up the polygon.</param>
        /// <returns>The surface area in game units.</returns>
        /// <remarks>Code from https://answers.unity.com/questions/684909/how-to-calculate-the-surface-area-of-a-irregular-p.html?utm_source=pocket_mylist</remarks>
        public static float PolygonArea(List<Vector3> points)
        {
            float temp = 0;

            for (int i = 0; i < points.Count; i++)
            {
                if (i != points.Count - 1)
                {
                    float mulA = points[i].x * points[i + 1].z;
                    float mulB = points[i + 1].x * points[i].z;
                    temp += mulA - mulB;
                }
                else
                {
                    float mulA = points[i].x * points[0].z;
                    float mulB = points[0].x * points[i].z;
                    temp += mulA - mulB;
                }
            }

            temp *= 0.5f;
            return Mathf.Abs(temp);
        }
    }
}