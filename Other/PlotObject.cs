using Autodesk.AutoCAD.DatabaseServices;

namespace PlotWizard
{    internal class PlotObject
    {
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public Extents3d Extents { get; set; }
    }
}
