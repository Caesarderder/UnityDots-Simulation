using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ProjectDawn.Navigation
{
    /// <summary>
    /// Classes with this interface will be included into <see cref="AgentsNavigationSettings"/> as sub settings.
    /// </summary>
    public interface ISubSettings { }

    /// <summary>
    /// Settings asset of agents navigation package. Contains list of sub settings combined from classes that implement <see cref="ISubSettings"/> interface.
    /// </summary>
    [CreateAssetMenu(fileName = "Agents Navigation Settings", menuName = "AI/Agents Navigation Settings", order = 1000)]
    [HelpURL("https://lukaschod.github.io/agents-navigation-docs/manual/settings.html")]
    public class AgentsNavigationSettings : ScriptableObject
    {
        static AgentsNavigationSettings s_Instance;

        /// <summary>
        /// Currently used agents navigation settings asset.
        /// </summary>
        public static AgentsNavigationSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                // Settings asset is stored in preloaded assets
                // Here we attempt to find it
                foreach (var asset in UnityEditor.PlayerSettings.GetPreloadedAssets())
                {
                    if (asset is AgentsNavigationSettings settings)
                    {
                        return settings;
                    }
                }
                return null;
#else
                return s_Instance;
#endif
            }

            set
            {
                if (Application.isPlaying)
                    throw new InvalidOperationException("Can not change agents navigation settings at runtime!");

#if UNITY_EDITOR
                var assets = new List<UnityEngine.Object>(UnityEditor.PlayerSettings.GetPreloadedAssets());

                // Remove all AgentsNavigationSettings
                if (value == null)
                {
                    for (int i = 0; i < assets.Count; i++)
                    {
                        if (assets[i] is AgentsNavigationSettings)
                        {
                            assets.RemoveAt(i);
                            i--;
                        }
                    }
                    UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
                    return;
                }

                // Change existing AgentsNavigationSettings to new value
                for (int i = 0; i < assets.Count; i++)
                {
                    if (assets[i] is AgentsNavigationSettings)
                    {
                        assets[i] = value;

                        // Force to contain only single AgentsNavigationSettings
                        for (int j = i + 1; j <assets.Count; j++)
                        {
                            if (assets[j] is AgentsNavigationSettings)
                            {
                                assets.RemoveAt(j);
                                j--;
                            }
                        }

                        UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
                        return;
                    }
                }

                // Simply add at the end AgentsNavigationSettings
                assets.Add(value);
                UnityEditor.PlayerSettings.SetPreloadedAssets(assets.ToArray());
#endif
            }
        }

        [SerializeReference]
        List<ISubSettings> m_SubSettings = new();

        public List<ISubSettings> SubSettings => m_SubSettings;

        public AgentsNavigationSettings()
        {
            s_Instance = this;
        }

        /// <summary>
        /// Returns true, if contains settings of specified type.
        /// </summary>
        public bool Contains(Type type)
        {
            foreach (var setting in m_SubSettings)
            {
                if (setting == null)
                    continue;
                if (setting.GetType() == type)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns instance of sub setting. In case project does not have settings asset set, it will return instance with default values.
        /// </summary>
        public static T Get<T>() where T : ISubSettings
        {
            if (Instance == null)
                return Activator.CreateInstance<T>();

            foreach (var setting in Instance.m_SubSettings)
            {
                if (setting == null)
                    continue;
                if (setting.GetType() == typeof(T))
                {
                    return (T)setting;
                }
            }

            return Activator.CreateInstance<T>();
        }

        void Reset()
        {
            m_SubSettings.Clear();
            foreach (var type in FindTypesInAssemblies())
            {
                if (Contains(type))
                    continue;

                var subSettings = Activator.CreateInstance(type) as ISubSettings;
                m_SubSettings.Add(subSettings);
            }
        }

        /// <summary>
        /// Returns all types that implement <see cref="ISubSettings"/> interface.
        /// </summary>
        /// <returns></returns>
        public static List<Type> FindTypesInAssemblies()
        {
            Type baseType = typeof(ISubSettings);
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
    }
}
