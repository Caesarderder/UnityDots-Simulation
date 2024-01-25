#if GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Griffin
{
    public enum GShadingSystem
    {
        Polaris,
#if __MICROSPLAT_POLARIS__
        MicroSplat
#endif
    }
}
#endif
