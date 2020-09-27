using System.Runtime.InteropServices;
using IEnumerable = System.Collections.IEnumerable;

namespace TESV_EspEquipmentGenerator
{
    public static class NifUtil
    {
        [DllImport("ousniusNifLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapesName([MarshalAs(UnmanagedType.LPStr)]string filename);
        public static string[] GetShapesNames(string filename) => GetShapesName(filename).GetLines();
        [DllImport("ousniusNifLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapeTextures([MarshalAs(UnmanagedType.LPStr)]string filename, [MarshalAs(UnmanagedType.LPStr)]string shapename);
        public static string[] GetShapeTexturesArray(string filename, string shapename)
                                                        => GetShapeTextures(filename, shapename).GetLines();

        [DllImport("ousniusNifLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetBSDismemberBodyParts([MarshalAs(UnmanagedType.LPStr)]string filename);


    }
}
