using System;
using System.Runtime.InteropServices;

namespace PlotWizard.Core
{
    internal static class UnmanagedApi
    {
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
        internal static extern int AcedTrans(double[] point, IntPtr fromRb, IntPtr toRb, int disp, double[] result);
    }
}
