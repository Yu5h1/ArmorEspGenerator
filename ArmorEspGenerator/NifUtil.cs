using System.Runtime.InteropServices;
using IEnumerable = System.Collections.IEnumerable;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public static class NifUtil
    {
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapesName([MarshalAs(UnmanagedType.LPStr)]string filename);
        public static string[] GetShapeNames(string filename) => GetShapesName(filename).GetLines();
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapeTextures([MarshalAs(UnmanagedType.LPStr)]string filename, [MarshalAs(UnmanagedType.LPStr)]string shapename);
        public static string[] GetShapeTexturesArray(string filename, string shapename)
                                                        => GetShapeTextures(filename, shapename).GetLines();

        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetTexturesFromAllShapes([MarshalAs(UnmanagedType.LPStr)]string filename);
        
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetBSDismemberBodyParts([MarshalAs(UnmanagedType.LPStr)]string filename);


    }
}
