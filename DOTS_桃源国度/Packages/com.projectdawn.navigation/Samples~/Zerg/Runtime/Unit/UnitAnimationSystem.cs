using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ProjectDawn.Navigation;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    [RequireMatchingQueriesForUpdate]
    public partial class UnitAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Animator animator, in UnitBrain brain, in UnitAnimator unit, in AgentBody body) =>
            {
                if (!animator)
                    return;

                animator.SetBool(unit.AttackId, brain.State == UnitBrainState.Attack);

                float speed = math.length(body.Velocity);

                animator.SetFloat(unit.MoveSpeedId, speed);
                animator.speed = speed > 0.3f ? speed * unit.MoveSpeed : 1f;
            }).WithoutBurst().Run();
        }
    }
}
