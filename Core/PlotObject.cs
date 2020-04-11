using Autodesk.AutoCAD.DatabaseServices;

namespace PlotWizard
{    internal class PlotObject
    {
        public string Label { get; set; }
        public string Sheet { get; set; }
        public Extents3d Extents { get; set; }
    }
}
