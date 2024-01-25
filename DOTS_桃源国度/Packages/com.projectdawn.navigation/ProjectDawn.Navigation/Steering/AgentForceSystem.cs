using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Transforms;

namespace ProjectDawn.Navigation
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(AgentLocomotionSystemGroup))]
    [Obsolete("AgentForceSystem is deprecated, please use AgentLocomotionSystem!", false)]
    public partial struct AgentForceSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AgentForceJob
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct AgentForceJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref LocalTransform transform, ref AgentBody body, in AgentSteering steering, in AgentShape shape)
            {
                if (body.IsStopped)
                    return;

                // Check, if we reached the destination
                float remainingDistance = body.RemainingDistance;
                if (remainingDistance <= steering.StoppingDistance + 1e-3f)
                {
                    body.Velocity = 0;
                    body.IsStopped = true;
                    return;
                }

                float maxSpeed = steering.Speed;

                // Start breaking if close to destination
                if (steering.AutoBreaking)
                {
                    float breakDistance = shape.Radius * 2 + steering.StoppingDistance;
                    if (remainingDistance <= breakDistance)
                    {
                        maxSpeed = math.lerp(steering.Speed * 0.25f, steering.Speed, remainingDistance / breakDistance);
                    }
                }

                // Force force to be maximum of unit length, but can be less
                float forceLength = math.length(body.Force);
                if (forceLength > 1)
                    body.Force = body.Force / forceLength;

                // Interpolate velocity
                body.Velocity = math.lerp(body.Velocity, body.Force * maxSpeed, math.saturate(DeltaTime * steering.Acceleration));

                float speed = math.length(body.Velocity);

                // Early out if steps is going to be very small
                if (speed < 1e-3f)
                    return;

                // Avoid over-stepping the destination
                if (speed * DeltaTime > remainingDistance)
                {
                    transform.Position += (body.Velocity / speed) * remainingDistance;
                    return;
                }

                // Update position
                transform.Position += DeltaTime * body.Velocity;

                // Update rotation
                if (shape.Type == ShapeType.Circle)
                {
                    float angle = math.atan2(body.Velocity.x, body.Velocity.y);
                    transform.Rotation = math.slerp(transform.Rotation, quaternion.RotateZ(-angle), DeltaTime * steering.AngularSpeed);
                }
                else if (shape.Type == ShapeType.Cylinder)
                {
                    float angle = math.atan2(body.Velocity.x, body.Velocity.z);
                    transform.Rotation = math.slerp(transform.Rotation, quaternion.RotateY(angle), DeltaTime * steering.AngularSpeed);
                }
            }
        }
    }
}
