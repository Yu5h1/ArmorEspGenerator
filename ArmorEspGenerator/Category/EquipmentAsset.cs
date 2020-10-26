using System.Collections.Generic;
using Yu5h1Tools.WPFExtension;

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
        public EquipmentAssets(EquipmentAsset  Male, EquipmentAsset Female)
        {
            male = Male;
            female = Female;
            if (male == null) male = new EquipmentAsset();
            if (female == null) female = new EquipmentAsset();
        }
    }
    public class EquipmentAsset
    {
        public string Model = "";
        public string _1stPerson = "";
        public string ItemModel = "";
        public override string ToString()
                                => "Model:" + PathInfo.GetName(Model) + "\n1stPerson:" + PathInfo.GetName(_1stPerson) + "\nGnd:" + PathInfo.GetName(ItemModel);
    }
}
