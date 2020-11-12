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
                if (ModelPathInfo.Exists) {
                    if (ModelPathInfo.Name.Contains("_1"))
                    {
                        EnableWeightSlider = File.Exists(ModelPathInfo.ChangeName(ModelPathInfo.Name.Replace("_1", "_0")));
                    }
                    var InventoryItemModelFolder = new PathInfo(new PathInfo(ModelPathInfo.directory).FindAny("GND", "GO"));
                    if (InventoryItemModelFolder.Exists)
                    {
                        var IIModel = InventoryItemModelFolder.FindAny(ModelPathInfo.Name.RemoveSuffixFromLast("_1")+".nif");
                        if (!IIModel.IsNullOrEmpty()) ItemModel = IIModel;
                    }
                }
            }
        }
        public string _1stPerson = "";
        public string ItemModel = "";
        public bool EnableWeightSlider = false;

        public override string ToString()
                                => "Model:" + PathInfo.GetName(Model) + "\n1stPerson:" + PathInfo.GetName(_1stPerson) + "\nInventory Item Model:" + PathInfo.GetName(ItemModel);
    }

    
}
