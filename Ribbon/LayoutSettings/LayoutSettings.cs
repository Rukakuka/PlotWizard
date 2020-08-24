using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotWizard.Ribbon
{
    internal static class LayoutSettings
    {
        public  const string defaultPlotterType  = "DWG To PDF.pc3";
        // key = readable name, value = canonical name
        public static KeyValuePair<string, string> defaultPageSize { get; } = new KeyValuePair<string, string>("ISO без полей A4(297.00 x 210.00 мм)", "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
        public const double defaultContentScaling = 1.003;
        public const double defaultViewportScaling = 1;
        public const bool defaultAutoOpenFile = true;
        public static string PlotterType { get; set; } = null;
        // key = readable name, value = canonical name
        public static KeyValuePair<string, string> PageSize { get; set; } = new KeyValuePair <string, string> (null, null);
        public static double ContentScaling { get; set; } = -1;
        public static double ViewportScaling { get; set; } = -1;
        public static bool? AutoOpenFile { get; set; } = null;
        public static void SetDefaults()
        {
            PlotterType = defaultPlotterType;
            PageSize = defaultPageSize;
            ContentScaling = defaultContentScaling;
            ViewportScaling = defaultViewportScaling;
            AutoOpenFile = defaultAutoOpenFile;
        }
    }
}
