using UnityEngine;
using UnityEditor;
using ProjectDawn.Navigation.Hybrid;

namespace ProjectDawn.Navigation.Editor
{
    static class AgentsNavigationMenuItems
    {
        [MenuItem("GameObject/AI/Agent Cylinder", false, 10)]
        static void CreateAgentCylinder(MenuCommand menuCommand)
        {
            // Create a custom game object
            var agent = new GameObject("Agent Cylinder", 
                typeof(AgentAuthoring),
                typeof(AgentCylinderShapeAuthoring));

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(agent.transform);
            capsule.transform.localPosition = new Vector3(0, 1.0f, 0);
            Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(agent, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(agent, "Create " + agent.name);
            Selection.activeObject = agent;
        }

        [MenuItem("GameObject/AI/Agent Circle", false, 11)]
        static void CreateAgentCircle(MenuCommand menuCommand)
        {
            // Create a custom game object
            var agent = new GameObject("Agent Circle", 
                typeof(AgentAuthoring),
                typeof(AgentCircleShapeAuthoring));

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(agent, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(agent, "Create " + agent.name);
            Selection.activeObject = agent;
        }
    }
}
