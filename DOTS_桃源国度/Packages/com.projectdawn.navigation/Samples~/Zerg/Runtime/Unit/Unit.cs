using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Sample.Zerg
{
    public enum PlayerId
    {
        Red,
        Blue,
    }

    public struct Unit : IComponentData
    {
        public PlayerId Owner;
    }

    public enum UnitBrainState
    {
        Idle,
        Move,
        Follow,
        Attack,
    }

    public struct UnitBrain : IComponentData
    {
        public UnitBrainState State;
    }

    public struct UnitCombat : IComponentData
    {
        public Entity Target;
        public float Damage;
        public float AggressionRadius;
        public float Range;
        public float Speed;
        public float Cooldown;
        public double CooldownTime;
        public double AttackTime;
        public bool IsReady(double time) => time >= CooldownTime + Cooldown;
        public bool IsFinished(double time) => time >= AttackTime + Speed;
    }

    public struct UnitFollow : IComponentData
    {
        public Entity Target;
        public float MinDistance;
    }

    public struct UnitLife : IComponentData
    {
        public float Life;
        public float MaxLife;
    }

    public struct UnitDead : IComponentData { }

    public struct UnitAnimator : IComponentData
    {
        public float MoveSpeed;
        public int MoveSpeedId;
        public int AttackId;
    }

    public struct UnitSmartStop : IComponentData
    {
        public float Radius;
    }
}
