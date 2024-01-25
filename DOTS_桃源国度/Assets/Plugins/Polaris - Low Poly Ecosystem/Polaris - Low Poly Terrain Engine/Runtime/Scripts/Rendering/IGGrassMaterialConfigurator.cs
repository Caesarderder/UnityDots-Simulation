#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin.Rendering
{
    public interface IGGrassMaterialConfigurator
    {
        void Configure(GStylizedTerrain terrain, int prototypeIndex, MaterialPropertyBlock propertyBlock);
    }
}
#endif
