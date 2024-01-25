using Unity.Collections;
using ProjectDawn.LocalAvoidance;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProjectDawn.Navigation.Sample.Scenarios
{
    public class LowLevelSonarAvoidance : MonoBehaviour
    {
        public float DesiredAngle = radians(0);
        public float3 Velocity;
        public float Radius = 1;
        public float MaxDistance = 3;
        public float Angle = radians(120);

        void OnDrawGizmos()
        {
            using (var sonar = new SonarAvoidance(Allocator.Temp))
            {
                sonar.Set(transform.position, Velocity, new float3(cos(DesiredAngle), sin(DesiredAngle), 0), new float3(0, 0, 1), Radius, MaxDistance, math.PI);

                // Add blocker behind the velocity
                // This will prevent situations where agent has on right and left equally good paths
                if (length(Velocity) > 1e-3f)
                    sonar.InsertObstacle(normalizesafe(-Velocity), Angle);

                var obstacles = FindObjectsOfType<LowLevelSonarAvoidanceObstacle>();
                foreach (var obstacle in obstacles)
                {
                    switch (obstacle.shape)
                    {
                        case LowLevelSonarAvoidanceObstacle.Shape.Circle:
                            sonar.InsertObstacleCircle(obstacle.transform.position, obstacle.Velocity, obstacle.Radius);
                            sonar.DrawObstacleCircle(obstacle.transform.position, obstacle.Velocity, obstacle.Radius);
                            break;
                        case LowLevelSonarAvoidanceObstacle.Shape.Line:
                            {
                                float3 from = obstacle.transform.position + obstacle.transform.rotation * new float3(obstacle.Radius, 0, 0);
                                float3 to = obstacle.transform.position + obstacle.transform.rotation * new float3(-obstacle.Radius, 0, 0);
                                sonar.InsertObstacleLine(from, to);
                                Debug.DrawLine(from, to, Color.red);
                                break;
                            }
                        case LowLevelSonarAvoidanceObstacle.Shape.LineNoRadius:
                            {
                                float3 from = obstacle.transform.position + obstacle.transform.rotation * new float3(obstacle.Radius, 0, 0);
                                float3 to = obstacle.transform.position + obstacle.transform.rotation * new float3(-obstacle.Radius, 0, 0);
                                sonar.InsertObstacleLineIgnoreRadius(from, to);
                                Debug.DrawLine(from, to, Color.red);
                                float3 center = (from + to) * 0.5f;
                                Debug.DrawLine(center, center + cross(new float3(0, 0, 1), to - from) * 0.2f, Color.red);
                                break;
                            }
                    }
                    
                }

                sonar.DrawSonar();
            }
        }
    }
}
