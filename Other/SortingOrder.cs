
namespace PlotWizard
{
    public class SortingOrder
    {
        public int Xsign { get; private set; } = 1;
        public int Ysign { get; private set; } = 1;
        public bool SwitchAxes { get; private set; } = false;
        public SortingOrder(int xsign, int ysign, bool switchAxes)
        {
            Xsign = xsign;
            Ysign = ysign;
            SwitchAxes = switchAxes;
        }
        public static SortingOrder ToSortingOrder(int listViewIndex)
        {
            return new SortingOrder(1, 1, false);
        }
        public static int ToListViewIndex(SortingOrder order)
        {
            return 0;
        }
    }
}
