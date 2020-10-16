using System.Collections.Generic;
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

        public static Dictionary<string, string[]> GetShapesTextureInfos(string filename) {
            Dictionary<string, string[]> results = new Dictionary<string, string[]>();
            var items =  GetTexturesFromAllShapes(filename).GetLines();
            int count = items.Length / 10;
            for (int i = 0; i < count; i++)
            {
                var begin = i * 10;
                var shapeName = items[begin];
                var textures = new string[9];
                for (int o = 0; o < 9; o++) textures[o] = items[begin+1 + o];
                results.Add(shapeName, textures);
            }
            return results;
        }

        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetBSDismemberBodyParts([MarshalAs(UnmanagedType.LPStr)]string filename);


    }
}
