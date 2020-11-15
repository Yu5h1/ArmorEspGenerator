using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using Yu5h1Tools.WPFExtension;
using System.Xml.Serialization;

namespace TESV_EspEquipmentGenerator
{
    [Serializable]
    public class DefaultEquipmentsValue
    {
        public static PathInfo location => new PathInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultEquipmentsValue.xml"));
        public static List<DefaultEquipmentsValue> current = null;
        public List<string> Tags;
        public string ArmorType;
        public int Value = 0;
        public double Weight = 0;
        public double Rating = 0;
        public List<string> Keywords;
        public DefaultEquipmentsValue()
        {
            Keywords = new List<string>();
            Tags = new List<string>();
        }
        public DefaultEquipmentsValue(string armorType,int value,double weight,double rating,string[] keywords,params string[] tags)
        {
            Tags = new List<string>(tags);
            ArmorType = armorType;
            Value = value;
            Weight = weight;
            Rating = rating;
            Keywords = new List<string>(keywords);
        }
        public DefaultEquipmentsValue(  Armor armor, params string[] tags) : this
                                     (  armor.bipedBodyTemplate.ArmorType,
                                        armor.Value,
                                        armor.Weight,
                                        armor.Rating,
                                        armor.keywords.Select(d=>d.GetValue().Split(' ')[0]).ToArray(),
                                        tags){ }

        public XElement ToXElement() => xmlUtil.ToXElement(this);
        public override string ToString()
            => string.Join("\n", "Tags : " + Tags.Join(","), "ArmorType : "+ ArmorType, "Value : "+ Value,
                "Weight : "+ Weight, "Rating : "+Rating, "Keywords : \n"+ Keywords.ToContext("     "));

        public static void CreateDefaultEquipmentsValue(List<DefaultEquipmentsValue> datas)
        {
            
            var xDoc = new XDocument();
            xDoc.SetRoot("Root");
            xDoc.Save(location);
        }
        public static List<DefaultEquipmentsValue> Load(XDocument xdoc) {
            current = new List<DefaultEquipmentsValue>();
            if (xdoc.Root != null)
            foreach (var item in xdoc.Root.Elements()) current.Add(item.ToObject<DefaultEquipmentsValue>());
            return current;
        }
        public static List<DefaultEquipmentsValue> Load() {
            if (!location.Exists) return new List<DefaultEquipmentsValue>();
            var xDoc = XDocument.Load(location);
            return Load(xDoc);
        }
        public static void Add(params DefaultEquipmentsValue[] datas) {
            var results = Load();
            results.AddRange(datas);
            Save(results);
        }
        public static void Remove(int index)
        {
            var results = Load();
            results.RemoveAt(index);
            Save(results);
        }
        public static void Save(List<DefaultEquipmentsValue> datas) {
            var xDoc = new XDocument();
            xDoc.Add(datas.ToXElement("root"));
            xDoc.Save(location);
        }
        public static void Clear() => Save(new List<DefaultEquipmentsValue>());
    }
}
