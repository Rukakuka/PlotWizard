using Autodesk.AutoCAD.Geometry;

namespace PlotWizard.Ribbon
{
    class BlockSelectorSettings
    {
        public const string defaultPrefix = "";
        public const string defaultPostfix = "";
        public static SortingOrder defaultSortingOrder { get; } = new SortingOrder(1, 1, false);
        public static string Prefix { get; set; } = null;
        public static string Postfix { get; set; } = null;
        public static SortingOrder SortingOrder { get; set; } = defaultSortingOrder;
        public static Point3d FirstCornerPoint { get; set; } = new Point3d(0, 0, 0);
        public static Point3d SecondCornerPoint { get; set; } = new Point3d(0, 0, 0);
        public static string TargetBlockName { get; set; } = null;
        public static void SetDefaults()
        {
            SortingOrder = defaultSortingOrder;
            Prefix = defaultPrefix;
            Postfix = defaultPostfix;
        }
    }
}
