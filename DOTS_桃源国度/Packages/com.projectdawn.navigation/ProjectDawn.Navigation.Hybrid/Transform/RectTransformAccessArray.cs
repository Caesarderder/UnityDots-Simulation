using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectDawn.Navigation.Hybrid
{
    /// <summary>
    /// Similar to <see cref="TransformAccessArray"/> just for <see cref="RectTransform"/>.
    /// This structure has similar usage like <see cref="EntityQueryExtensionsForTransformAccessArray.GetTransformAccessArray"/> with caching functionality.
    /// </summary>
    internal struct RectTransformAccessArray : IDisposable
    {
        TransformAccessArray m_Array;
        int m_Version;

        public void Update(in EntityQuery entityQuery)
        {
            int version = entityQuery.GetCombinedComponentOrderVersion();
            if (version == m_Version && m_Array.isCreated)
                return;

            m_Version = version;
            if (m_Array.isCreated)
                m_Array.SetTransforms(entityQuery.ToComponentArray<RectTransform>());
            else
                m_Array = new TransformAccessArray(entityQuery.ToComponentArray<RectTransform>());
        }

        public static implicit operator TransformAccessArray(RectTransformAccessArray v) => v.m_Array;

        public void Dispose()
        {
            if (m_Array.isCreated)
                m_Array.Dispose();
        }
    }
}
