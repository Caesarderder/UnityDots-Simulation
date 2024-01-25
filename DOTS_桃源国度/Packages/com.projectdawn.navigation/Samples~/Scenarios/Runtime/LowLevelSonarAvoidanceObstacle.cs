using Unity.Collections;
using ProjectDawn.LocalAvoidance;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProjectDawn.Navigation.Sample.Scenarios
{
    public class LowLevelSonarAvoidanceObstacle : MonoBehaviour
    {
        public Shape shape;
        public float3 Velocity;
        public float Radius = 1;

        public enum Shape
        {
            Circle,
            Line,
            LineNoRadius,
        }
    }
}
