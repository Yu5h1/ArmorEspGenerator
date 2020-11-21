using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IEnumerable = System.Collections.IEnumerable;

public static class NifUtil
{

    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    [return:MarshalAs(UnmanagedType.I1)]
    public static extern bool HasCollisionObject([MarshalAs(UnmanagedType.LPStr)]string filePath);
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    [return:MarshalAs(UnmanagedType.I1)]
    public static extern bool IsGroundItemObject([MarshalAs(UnmanagedType.LPStr)]string filePath);
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern string GetShapesName([MarshalAs(UnmanagedType.LPStr)]string filename);
    public static string[] GetShapeNames(string filename) => GetShapesName(filename).Split('\n');
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

    public static List<(string name, string[] textures)> GetShapesTextureInfos(string filename) {
        var results = new List<(string name, string[] textures)>();
        var pathinfo = new PathInfo(filename);
        if (pathinfo.Exists && pathinfo.extension.EndsWith("nif",StringComparison.OrdinalIgnoreCase)) {
            var items = GetTexturesFromAllShapes(filename).GetLines();
            if (items.Length > 1) {
                int count = items.Length / 10;
                for (int i = 0; i < count; i++)
                {
                    var begin = i * 10;
                    var shapeName = items[begin];
                    var textures = new string[9];
                    for (int o = 0; o < 9; o++) textures[o] = items[begin + 1 + o];
                    results.Add((shapeName, textures));
                }
            }
        }
        return results;
    }
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern string GetBSDismemberBodyParts([MarshalAs(UnmanagedType.LPStr)]string filename);

    public static List<(string shapeName, string[] textures)> TransferToTextureSetOrder(this List<(string shapeName, string[] textures)> infos)
    {
        foreach (var item in infos)
        {
            item.textures.ShapeTextureOrderToTextureSetOrder();
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
        var shapesInfo = GetShapesTextureInfos(fileName);
        if (shapeIndex < shapesInfo.Count) {
            var targetInfo = shapesInfo[shapeIndex];
            for (int index = 0; index < shapesInfo.Count; index++)
            {
                var item = shapesInfo[index];
                if (item.textures[0].Equals(targetInfo.textures[0], StringComparison.OrdinalIgnoreCase))
                    results.Add(index);
            }
        }
        return results.ToArray();
    }

    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern string GetEyesTexture([MarshalAs(UnmanagedType.LPStr)]string filename);
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern string GetBrowsTexture([MarshalAs(UnmanagedType.LPStr)]string filename);

    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEyesTexture([MarshalAs(UnmanagedType.LPStr)]string filePath, [MarshalAs(UnmanagedType.LPStr)]string texturePath);
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetBrowsTexture([MarshalAs(UnmanagedType.LPStr)]string filePath, [MarshalAs(UnmanagedType.LPStr)]string texturePath);

    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Test([MarshalAs(UnmanagedType.LPStr)]string filePath);
    [DllImport("NifUtility.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void TestAllMethods([MarshalAs(UnmanagedType.LPStr)]string filePath);
}

