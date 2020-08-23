using System;

namespace PlotWizard
{
    public class SortingOrder
    {
        public int Xsign { get; private set; } = 1;
        public int Ysign { get; private set; } = 1;
        public bool SwapAxes { get; private set; } = false;
        public SortingOrder(int xsign, int ysign, bool swapAxes)
        {
            if (!ValidateSign(xsign) || !ValidateSign(ysign))
            {
                throw new ArgumentException("Sorting order: Signs fail validating");
            }
            Xsign = xsign;
            Ysign = ysign;
            SwapAxes = swapAxes;
        }
        private bool ValidateSign(int sign)
        {
            return (sign == 1 || sign == -1);
        }
        public static SortingOrder ToSortingOrder(int listViewIndex)
        {
            switch (listViewIndex)
            {
                case 0:
                    return new SortingOrder(1, 1, false);
                case 1:
                    return new SortingOrder(1, -1, false);
                case 2:
                    return new SortingOrder(1, 1, true);
                case 3:
                    return new SortingOrder(-1, 1, true);
                case 4:
                    return new SortingOrder(-1, 1, false);
                case 5:
                    return new SortingOrder(-1, -1, false);
                case 6:
                    return new SortingOrder(1, -1, true);
                case 7:
                    return new SortingOrder(-1, -1, true);
                default:
                    return new SortingOrder(1, 1, false);
            }
        }
        public static int ToListViewIndex(SortingOrder order)
        {
            throw new NotImplementedException();
        }
    }
}
