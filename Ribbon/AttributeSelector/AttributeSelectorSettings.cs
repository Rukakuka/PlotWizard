
namespace PlotWizard.Ribbon
{
    class AttributeSelectorSettings
    {
        public static string defaultPrefix { get; set; } = "";
        public static string defaultPostfix { get; set; } = "";
        public static SortingOrder defaultSortingOrder { get; set; } = new SortingOrder(1, 1, false);
        public static string Prefix { get; set; } = null;
        public static string Postfix { get; set; } = null;
        public static SortingOrder SortingOrder { get; set; } = null;
        public static void SetDefaults()
        {
            SortingOrder = defaultSortingOrder;
            Prefix = defaultPrefix;
            Postfix = defaultPostfix;
        }
    }
}
