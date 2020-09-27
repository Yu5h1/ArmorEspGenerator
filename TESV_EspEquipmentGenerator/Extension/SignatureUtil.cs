using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESV_EspEquipmentGenerator
{
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
        public static string GetTemplateEditorID<T>()
        {
            var type = typeof(T);
            if (type == typeof(Armor)) return Armor.TemplateEditorID;
            if (type == typeof(ArmorAddon)) return ArmorAddon.TemplateEditorID;
            if (type == typeof(TextureSet)) return TextureSet.TemplateEditorID;
            return null;
        }
    }
}
