using System.IO;
using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;
using System.Linq;

namespace TESV_EspEquipmentGenerator
{
    public class EquipmentAssets
    {
        public EquipmentAsset male;
        public EquipmentAsset female;
        public EquipmentAssets() {
            male = new EquipmentAsset();
            female = new EquipmentAsset();
        }
    }
    public class EquipmentAsset
    {
        string ModelCache = "";
        public string Model {
            get => ModelCache;
            set {
                ModelCache = value;
                var ModelPathInfo = new PathInfo(value);
                if (ModelPathInfo.Name.Contains("_1")) {
                    EnableWeightSlider = File.Exists(ModelPathInfo.ChangeName(ModelPathInfo.Name.Replace("_1", "_0")));
                }
            }
        }
        public string _1stPerson = "";
        public string ItemModel = "";
        public bool EnableWeightSlider = false;

        public override string ToString()
                                => "Model:" + PathInfo.GetName(Model) + "\n1stPerson:" + PathInfo.GetName(_1stPerson) + "\nGnd:" + PathInfo.GetName(ItemModel);
    }
}
