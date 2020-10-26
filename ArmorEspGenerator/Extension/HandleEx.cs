using XeLib.API;
using XeLib;
using System.Linq;
using System.Text.RegularExpressions;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public static class HandleEx
    {
        public static string GetFormID(this Handle handle) => Records.GetFormId(handle, true, false).ToString($"X{8}");
        public static string GetEditorID(this Handle handle) => RecordValues.GetEditorId(handle);
        public static bool SetEditorID(this Handle handle, string value)
        {
            value = value.MakeValidEditorID();
            if (value != "" && value != handle.GetEditorID())
            {
                handle.SetValue("EDID", value);
                return true;
            }
            return false;
        }
        public static bool HasValue(this Handle handle) => handle != null && handle.HasElement(handle.GetDisplayName());
        public static bool HasElement(this Handle handle, string path) => handle != null ? Elements.HasElement(handle, path) : false;
        public static Handle GetElement(this Handle handle, string path) => path == "" || !handle.HasElement(path) ? null : Elements.GetElement(handle, path);
        public static Handle[] GetElements(this Handle handle,string path = "",bool sort = false, bool filter = false) => Elements.GetElements(handle.Value, path, sort,filter);
        public static Handle[] GetElementsByGetSignature(this Handle handle, string Signature = "") => Elements.GetElements(handle).Where(d=>d.GetSignature() == Signature).ToArray();
        public static Handle[] GetArrayItems(this Handle handle, string itemName) 
            => handle != null && handle.HasElement(itemName) ? Elements.GetElements(handle.Value) : new Handle[0] ;
        public static Handle AddArrayItem(this Handle handle, string path = "", string subpath = "", string value = "") 
                                                    => Elements.AddArrayItem(handle, path, subpath, value);
        public static string GetValue(this Handle handle,string path = "") => handle == null ? "" : ElementValues.GetValue(handle, path);
        public static Elements.ValueTypes GetValueType(this Handle handle, string path = "") => Elements.ValueType(handle);
        public static int GetInteger(this Handle handle, string path = "") => ElementValues.GetIntValue(handle, path);
        public static void SetInteger(this Handle handle, string path = "",int value = 0) => ElementValues.SetIntValue(handle, path,value);
        public static void SetValue(this Handle handle,string value) => ElementValues.SetValue(handle, "", value);
        public static void SetValue(this Handle handle, string path, string value)
        {
            if (Elements.HasElement(handle, path)) ElementValues.SetValue(handle, path, value);
            else Elements.AddElementValue(handle, path, value);
        }
        public static bool GetFlag(this Handle handle, string path,string name) => ElementValues.GetFlag(handle, path,name);
        public static void SetFlag(this Handle handle, string path, string name, bool value) => ElementValues.SetFlag(handle, path,name,value);

        public static string GetSignature(this Handle handle) => ElementValues.Signature(handle);

        public static string[] GetSignatures(this Handle handle) => Elements.GetSignaturesAllowed(handle);

        public static string GetSignatureByDisplayName(this Handle handle,string displayName)
        {
            if (handle != null)
                foreach (var item in handle.GetDefineNames())
                {
                    if (item.ToLower().Contains(displayName.ToLower())) return item;
                }
            return "";
        }

        public static Handle Owner(this Handle handle) => Elements.GetContainer(handle);

        public static Handle AddElement(this Handle handle, string path = "")
            => Elements.AddElement(handle,path);

        public static Handle CopyAsNew(this Handle handle, string newEditorID = "", Handle container = null)
        {
           var result = Elements.CopyElement(handle, container == null ? handle.Owner() : container, true);
            if (newEditorID != "") result.SetEditorID(newEditorID);
            return result;
        }
        public static Handle CopyAsOverride(this Handle handle, string newEditorID, Handle container = null)
        {
            var result = Elements.CopyElement(handle, container == null ? handle.Owner() : container, false);
            result.SetEditorID(newEditorID);
            return result;
        }
        public static void Delete(this Handle handle, string path = "") => Elements.RemoveElement(handle, path);
        public static bool CompareSignature<T>(this Handle handle) => handle.GetSignature() == SignatureUtil.GetSignature<T>();
        public static int Count(this Handle handle) => Elements.ElementCount(handle);
        public static string GetDisplayName(this Handle handle) => handle == null ? "Null" : ElementValues.DisplayName(handle);
        public static string[] GetDefineNames(this Handle handle) => handle == null ? null : Elements.GetDefNames(handle);
        public static Elements.ElementTypes GetElementType(this Handle handle) => Elements.ElementType(handle);

        public static Handle[] GetRecords(this Handle handle,string search = "",bool includeOverrides = false) => Records.GetRecords(handle, search, includeOverrides);

        public static string GetRecordHeaderFormID(this Handle handle) {
            var RecordHeader = handle.GetElement("Record Header");
            if (RecordHeader != null) {
                return RecordHeader.GetValue("FormID");
            }
            return "";
        }
        public static string MakeValidEditorID(this string txt)
        {
            if (txt.IsNullOrEmpty()) return "";
            txt = txt.Split('_').Select(d => d.FirstCharToUpper()).Join();
            txt = txt.Split(' ').Select(d => d.FirstCharToUpper()).Join();
            return new Regex("[^a-zA-Z0-9]").Replace(txt, "");
        }
    }
}
