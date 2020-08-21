using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotWizard.Ribbon
{
    internal static class LayoutSettings
    {
        public static string defaultPlotterType { get; set; } = "DWG To PDF.pc3";
        // key = readable name, value = canonical name
        public static KeyValuePair<string, string> defaultPageSize { get; set; } = new KeyValuePair<string, string> ("ISO без полей A4(297.00 x 210.00 мм)", "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
        public static double defaultContentScaling { get; set; } = 1.003;
        public static double defaultViewportScaling { get; set; } = 1;
        public static string PlotterType { get; set; } = null;
        public static KeyValuePair<string, string> PageSize { get; set; } = new KeyValuePair <string, string> (null, null);
        public static double ContentScaling { get; set; } = -1;
        public static double ViewportScaling { get; set; } = -1;

        public static void SetDefaults()
        {
            PlotterType = defaultPlotterType;
            PageSize = defaultPageSize;
            ContentScaling = defaultContentScaling;
            ViewportScaling = defaultViewportScaling;
        }
    }
}
