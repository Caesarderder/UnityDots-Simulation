using System;
using UnityEngine;
using ProjectDawn.Navigation.Hybrid;

namespace ProjectDawn.Navigation.Sample.Scenarios
{
    [Obsolete("AgentDestinationAuthoring is for sample purpose, please create your own component that will handle agent high level logic.")]
    [RequireComponent(typeof(AgentAuthoring))]
    [DisallowMultipleComponent]
    internal class AgentDestinationAuthoring : MonoBehaviour
    {
        public Transform Target;
        public float Radius;
        public bool EveryFrame;

        private void Start()
        {
            var agent = transform.GetComponent<AgentAuthoring>();
            var body = agent.EntityBody;
            body.Destination = Target.position;
            body.IsStopped = false;
            agent.EntityBody = body;
        }

        void Update()
        {
            if (!EveryFrame)
                return;
            Start();
        }
    }
}
