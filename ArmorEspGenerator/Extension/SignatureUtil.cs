﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESV_EspEquipmentGenerator;
using XeLib;

public static class SignatureUtil
{
    public static string GetSignature<T>()
    {
        var type = typeof(T);
        if (type == typeof(Armor)) return Armor.Signature;
        if (type == typeof(ArmorAddon)) return ArmorAddon.Signature;
        if (type == typeof(TextureSet)) return TextureSet.Signature;
        return null;
    }
    public static bool CompareSignature<T>(this Handle handle) => handle.GetSignature() == SignatureUtil.GetSignature<T>();
}