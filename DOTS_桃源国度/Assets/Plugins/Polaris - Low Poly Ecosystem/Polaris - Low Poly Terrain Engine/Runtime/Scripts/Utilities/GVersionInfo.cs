#if GRIFFIN
namespace Pinwheel.Griffin
{
    /// <summary>
    /// Utility class contains product info
    /// </summary>
    public static class GVersionInfo
    {
        public static float Number
        {
            get
            {
                return 2021.1f;
            }
        }

        public static string Code
        {
            get
            {
                return "2021.1.15";
            }
        }

        public static string ProductName
        {
            get
            {
                return "Polaris - Low Poly Terrain Engine";
            }
        }

        public static string ProductNameAndVersion
        {
            get
            {
                return string.Format("{0} {1}", ProductName, Code);
            }
        }

        public static string ProductNameShort
        {
            get
            {
                return "Polaris";
            }
        }

        public static string ProductNameAndVersionShort
        {
            get
            {
                return string.Format("{0} {1}", ProductNameShort, Code);
            }
        }
    }
}
#endif
