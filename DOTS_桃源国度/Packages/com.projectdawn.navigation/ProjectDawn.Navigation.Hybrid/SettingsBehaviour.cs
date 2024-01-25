using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Navigation.Hybrid
{
    [Obsolete("This class is obsolete, please use new settings workflow https://lukaschod.github.io/agents-navigation-docs/manual/settings.html.")]
    public abstract class SettingsBehaviour : MonoBehaviour
    {
        public static readonly List<Type> Types = FindTypesInAssemblies();
        public static List<Type> FindTypesInAssemblies()
        {
            Type baseType = typeof(SettingsBehaviour);
            List<Type> foundTypes = new List<Type>();

            // Get all loaded assemblies in the current app domain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                // Get all types in the current assembly
                Type[] types = assembly.GetTypes();

                // Check if any of the types inherit from the base type
                foreach (Type type in types)
                {
                    if (baseType.IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        foundTypes.Add(type);
                    }
                }
            }

            return foundTypes;
        }
        public abstract Entity GetOrCreateEntity();
    }
}
