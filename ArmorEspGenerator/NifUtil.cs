using System.Collections.Generic;
using System.Runtime.InteropServices;
using IEnumerable = System.Collections.IEnumerable;
using Yu5h1Tools.WPFExtension;
using System;
using System.IO;

namespace TESV_EspEquipmentGenerator
{
    public static class NifUtil
    {
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapesName([MarshalAs(UnmanagedType.LPStr)]string filename);
        public static string[] GetShapeNames(string filename) => GetShapesName(filename).GetLines();
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapeTexturesByIndex([MarshalAs(UnmanagedType.LPStr)]string fileName,Int32 index);
        public static string[] GetShapeTexturesArrayByIndex(string fileName, int index)
                                                => GetShapeTexturesByIndex(fileName, index).GetLines();
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetShapeTextures([MarshalAs(UnmanagedType.LPStr)]string filename, [MarshalAs(UnmanagedType.LPStr)]string shapename);
        public static string[] GetShapeTexturesArray(string filename, string shapename)
                                                        => GetShapeTextures(filename, shapename).GetLines();

        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetTexturesFromAllShapes([MarshalAs(UnmanagedType.LPStr)]string filename);

        public static Dictionary<string, string[]> GetShapesTextureInfos(string filename) {
            Dictionary<string, string[]> results = new Dictionary<string, string[]>();
            if (File.Exists(filename)) {
                var items = GetTexturesFromAllShapes(filename).GetLines();
                int count = items.Length / 10;
                for (int i = 0; i < count; i++)
                {
                    var begin = i * 10;
                    var shapeName = items[begin];
                    var textures = new string[9];
                    for (int o = 0; o < 9; o++) textures[o] = items[begin + 1 + o];
                    results.Add(shapeName, textures);
                }
            }
            return results;
        }
        [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetBSDismemberBodyParts([MarshalAs(UnmanagedType.LPStr)]string filename);

        public static Dictionary<string, string[]> TransferToTextureSetOrder(this Dictionary<string, string[]> infos)
        {
            foreach (var item in infos)
            {
                item.Value.ShapeTextureOrderToTextureSetOrder();
            }
            return infos;
        }
        public static string[] ShapeTextureOrderToTextureSetOrder(this string[] shapeTextures)
        {
            if (shapeTextures.Length > 5)
            {
                shapeTextures.Switch(2, 5);
                shapeTextures.Switch(4, 5);
                shapeTextures.Switch(3, 4);
            }
            return shapeTextures;
        }

        public static int[] GetShareTexturesShapesIndices(string fileName,int shapeIndex) {
            List<int> results = new List<int>();
            var infos = GetShapesTextureInfos(fileName);
            var tryGetTargetInfo = infos.IndexOf(shapeIndex);            
            if (tryGetTargetInfo != null) {
                var targetInfo = (KeyValuePair<string, string[]>)tryGetTargetInfo;
                infos.For((index,item) =>
                {
                    if (item.Value[0].Equals(targetInfo.Value[0], StringComparison.OrdinalIgnoreCase))
                        results.Add(index);
                });
            }
            return results.ToArray();
        }
    }
}
