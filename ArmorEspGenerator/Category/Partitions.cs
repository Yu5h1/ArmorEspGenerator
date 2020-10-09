using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XeLib;
using Yu5h1Tools.WPFExtension;

namespace TESV_EspEquipmentGenerator
{
    public enum Partitions
    {
        Head = 30,
        Hair = 31,
        Body = 32,
        Hands = 33,
        Forearms = 34,
        Amulet = 35,
        Ring = 36,
        Feet = 37,
        Calves = 38,
        Shield = 39,
        Tail = 40,
        LongHair = 41,
        Circlet = 42,
        Ears = 43,
        FaceRomouth = 44,
        Neck = 45,  //(like a cape, scarf, or shawl, neck-tie etc),
        Chest = 46,  // primary or outergarment,
        Back = 47,  //(like a backpack/wings etc),
        MiscFX = 48,  //(use for anything that doesnt fit in the list),
        Pelvis = 49,  // primary or outergarment,
        DecapitatedHead = 50,
        Decapitate = 51,
        Pelvis_2nd = 52,  //secondary or undergarment
        Leg_Right = 53,  //primary or outergarment or right leg
        Leg_Left = 54,  //secondary or undergarment or left leg
        Face = 55,  //alternate or jewelry
        Chest_2nd = 56,  //secondary or undergarment
        Shoulder = 57,
        Arm_Left = 58,  //secondary or undergarment or left arm
        Arm_Right = 59,  //primary or outergarment or right arm
        Misc_or_FX = 60,  //(use for anything that doesnt fit in the list)
        FX01 = 61,
    }
    public enum BSDismemberBodyPartType
    {
        BP_TORSO = 0, //  Torso 
        BP_HEAD = 1, //  Head 
        BP_HEAD2 = 2, //  Head 2
        BP_LEFTARM = 3, //  Left Arm
        BP_LEFTARM2 = 4, //  Left Arm 2
        BP_RIGHTARM = 5, //  Right Arm
        BP_RIGHTARM2 = 6, //  Right Arm 2
        BP_LEFTLEG = 7, //  Left Leg
        BP_LEFTLEG2 = 8, //  Left Leg 2
        BP_LEFTLEG3 = 9, //  Left Leg 3
        BP_RIGHTLEG = 10, //  Right Leg
        BP_RIGHTLEG2 = 11, //  Right Leg 2
        BP_RIGHTLEG3 = 12, //  Right Leg 3
        BP_BRAIN = 13, //  Brain 
        SBP_30_HEAD = 30, //  Skyrim, Head(Human), Body(Atronachs, Beasts), Mask(Dragonpriest)
        SBP_31_HAIR = 31, //  Skyrim, Hair(human), Far(Dragon), Mask2(Dragonpriest), SkinnedFX(Spriggan)
        SBP_32_BODY = 32, //  Skyrim, Main body, extras(Spriggan)
        SBP_33_HANDS = 33, //  Skyrim, Hands L/R, BodyToo(Dragonpriest), Legs(Draugr), Arms(Giant)
        SBP_34_FOREARMS = 34, //  Skyrim, Forearms L/R, Beard(Draugr)
        SBP_35_AMULET = 35, //  Skyrim, Amulet
        SBP_36_RING = 36, //  Skyrim, Ring
        SBP_37_FEET = 37, //  Skyrim, Feet L/R
        SBP_38_CALVES = 38, //  Skyrim, Calves L/R
        SBP_39_SHIELD = 39, //  Skyrim, Shield
        SBP_40_TAIL = 40, //  Skyrim, Tail(Argonian/Khajiit), Skeleton01(Dragon), FX01(AtronachStorm), FXMist (Dragonpriest), Spit(Chaurus, Spider), SmokeFins(IceWraith)
        SBP_41_LONGHAIR = 41, //  Skyrim, Long Hair(Human), Skeleton02(Dragon), FXParticles(Dragonpriest)
        SBP_42_CIRCLET = 42, //  Skyrim, Circlet(Human, MouthFireEffect(Dragon)
        SBP_43_EARS = 43, //  Skyrim, Ears
        SBP_44_DRAGON_BLOODHEAD_OR_MOD_MOUTH = 44, //  Skyrim, Bloodied dragon head, or NPC face/mouth
        SBP_45_DRAGON_BLOODWINGL_OR_MOD_NECK = 45, //  Skyrim, Left Bloodied dragon wing, Saddle(Horse), or NPC cape, scarf, shawl, neck-tie, etc.
        SBP_46_DRAGON_BLOODWINGR_OR_MOD_CHEST_PRIMARY = 46, //  Skyrim, Right Bloodied dragon wing, or NPC chest primary or outergarment
        SBP_47_DRAGON_BLOODTAIL_OR_MOD_BACK = 47, //  kyrim, Bloodied dragon tail, or NPC backpack/wings/...
        SBP_48_MOD_MISC1 = 48, //  nything that does not fit in the list
        SBP_49_MOD_PELVIS_PRIMARY = 49, //  Pelvis primary or outergarment
        SBP_50_DECAPITATEDHEAD = 50, //  Skyrim, Decapitated Head
        SBP_51_DECAPITATE = 51, //  Skyrim, Decapitate, neck gore
        SBP_52_MOD_PELVIS_SECONDARY = 52, //  Pelvis secondary or undergarment
        SBP_53_MOD_LEG_RIGHT = 53, //  Leg primary or outergarment or right leg
        SBP_54_MOD_LEG_LEFT = 54, //  Leg secondary or undergarment or left leg
        SBP_55_MOD_FACE_JEWELRY = 55, //  Face alternate or jewelry
        SBP_56_MOD_CHEST_SECONDARY = 56, //  hest secondary or undergarment
        SBP_57_MOD_SHOULDER = 57, //  Shoulder 
        SBP_58_MOD_ARM_LEFT = 58, //  Arm secondary or undergarment or left arm
        SBP_59_MOD_ARM_RIGHT = 59, //  Arm primary or outergarment or right arm
        SBP_60_MOD_MISC2 = 60, //  Anything that does not fit in the list
        SBP_61_FX01 = 61, //  Skyrim, FX01(Humanoid)
        BP_SECTIONCAP_HEAD = 101, //  Section Cap | Head
        BP_SECTIONCAP_HEAD2 = 102, //  Section Cap | Head 2
        BP_SECTIONCAP_LEFTARM = 103, //  Section Cap | Left Arm
        BP_SECTIONCAP_LEFTARM2 = 104, //  Section Cap | Left Arm 2
        BP_SECTIONCAP_RIGHTARM = 105, //  Section Cap | Right Arm
        BP_SECTIONCAP_RIGHTARM2 = 106, //  Section Cap | Right Arm 2
        BP_SECTIONCAP_LEFTLEG = 107, //  Section Cap | Left Leg
        BP_SECTIONCAP_LEFTLEG2 = 108, //  Section Cap | Left Leg 2
        BP_SECTIONCAP_LEFTLEG3 = 109, //  Section Cap | Left Leg 3
        BP_SECTIONCAP_RIGHTLEG = 110, //  Section Cap | Right Leg
        BP_SECTIONCAP_RIGHTLEG2 = 111, //  Section Cap | Right Leg 2
        BP_SECTIONCAP_RIGHTLEG3 = 112, //  Section Cap | Right Leg 3
        BP_SECTIONCAP_BRAIN = 113, //  Section Cap | Brain
        SBP_130_HEAD = 130, //  Skyrim, Head slot, use on full-face helmets
        SBP_131_HAIR = 131, //  Skyrim, Hair slot 1, use on hoods
        SBP_141_LONGHAIR = 141, //  Skyrim, Hair slot 2, use for longer hair
        SBP_142_CIRCLET = 142, //  Skyrim, Circlet slot 1, use for circlets
        SBP_143_EARS = 143, //  Skyrim, Ear slot
        SBP_150_DECAPITATEDHEAD = 150, //  Skyrim, neck gore on head side
        BP_TORSOCAP_HEAD = 201, //  orso Cap | Head
        BP_TORSOCAP_HEAD2 = 202, //  Torso Cap | Head 2
        BP_TORSOCAP_LEFTARM = 203, //  Torso Cap | Left Arm
        BP_TORSOCAP_LEFTARM2 = 204, //  Torso Cap | Left Arm 2
        BP_TORSOCAP_RIGHTARM = 205, //  Torso Cap | Right Arm
        BP_TORSOCAP_RIGHTARM2 = 206, //  Torso Cap | Right Arm 2
        BP_TORSOCAP_LEFTLEG = 207, //  Torso Cap | Left Leg
        BP_TORSOCAP_LEFTLEG2 = 208, //  Torso Cap | Left Leg 2
        BP_TORSOCAP_LEFTLEG3 = 209, //  Torso Cap | Left Leg 3
        BP_TORSOCAP_RIGHTLEG = 210, //  Torso Cap | Right Leg
        BP_TORSOCAP_RIGHTLEG2 = 211, //  Torso Cap | Right Leg 2
        BP_TORSOCAP_RIGHTLEG3 = 212, //  Torso Cap | Right Leg 3
        BP_TORSOCAP_BRAIN = 213, //  Torso Cap | Brain
        SBP_230_HEAD = 230, //  Skyrim, Head slot, use for neck on character head
        BP_TORSOSECTION_HEAD = 1000, //  Torso Section | Head
        BP_TORSOSECTION_HEAD2 = 2000, //  Torso Section | Head 2
        BP_TORSOSECTION_LEFTARM = 3000, //  Torso Section | Left Arm
        BP_TORSOSECTION_LEFTARM2 = 4000, //  Torso Section | Left Arm 2
        BP_TORSOSECTION_RIGHTARM = 5000, //  Torso Section | Right Arm
        BP_TORSOSECTION_RIGHTARM2 = 6000, //  Torso Section | Right Arm 2
        BP_TORSOSECTION_LEFTLEG = 7000, //  Torso Section | Left Leg
        BP_TORSOSECTION_LEFTLEG2 = 8000, //  Torso Section | Left Leg 2
        BP_TORSOSECTION_LEFTLEG3 = 9000, //  Torso Section | Left Leg 3
        BP_TORSOSECTION_RIGHTLEG = 10000, //  Torso Section | Right Leg
        BP_TORSOSECTION_RIGHTLEG2 = 11000, //  Torso Section | Right Leg 2
        BP_TORSOSECTION_RIGHTLEG3 = 12000, //  Torso Section | Right Leg 3
        BP_TORSOSECTION_BRAIN = 13000    //  Torso Section | Brain     
    }


    public static class PartitionsUtil {
        public static void SetPartitionFlags(Handle handle, Partitions[] values) {
            handle.SetValue(ConvertPartitionsToFlagsValue(GetPartitionFlags(values)));
        }
        public static PartitionFlag[] GetPartitionFlags()
        {
            var partitions = (Partitions[])System.Enum.GetValues(typeof(Partitions));
            var results = new PartitionFlag[partitions.Length];
            for (int i = 0; i < partitions.Length; i++) results[i] = new PartitionFlag(partitions[i],false);
            return results;
        }
        public static PartitionFlag[] GetPartitionFlags(Handle handle)
        {
            var results = GetPartitionFlags();
            if (handle != null) {
                var FlagsValue = handle.GetValue();
                for (int i = 0; i < results.Length; i++)
                {
                    results[i].IsEnable = false;
                    if (i < FlagsValue.Length) results[i].IsEnable = FlagsValue[i] == '1';
                }
            }
            return results;
        }
        public static PartitionFlag[] GetPartitionFlags(Partitions[] values)
        {
            var results = GetPartitionFlags();
            for (int i = 0; i < values.Length; i++)
            {
                for (int o = 0; o < results.Length; o++)
                {
                    if (results[o].Partition.Equals(values[i]))
                    {
                        results[o].IsEnable = true;
                        break;
                    }
                }
            }
            return results;
        }
        public static string ConvertPartitionsToFlagsValue(PartitionFlag[] value)
        {
            var result = "";
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (result == string.Empty)
                {
                    if (value[i].IsEnable) result = "1";
                }
                else result = (value[i].IsEnable ? "1" : "0") + result;
            }
            return result;
        }
        public static BSDismemberBodyPartType[] ConvertIndicesToBodyParts(this IEnumerable<int> indices)
        {
            BSDismemberBodyPartType[] results = new BSDismemberBodyPartType[indices.Count()];
            for (int i = 0; i < results.Length; i++)
                results[i] = (BSDismemberBodyPartType)indices.ElementAt(i);
            return results;
        }
        public static bool Contain(this BSDismemberBodyPartType bodypart, params BSDismemberBodyPartType[] flags)
        {
            foreach (var flag in flags) if (bodypart.Equals(flag)) return true;
            return false;
        }
        public static Partitions[] BSDismemberBodyPartsToPartitions(this IEnumerable<BSDismemberBodyPartType> bodyparts)
        {
            List<Partitions> results = new List<Partitions>();
            bodyparts.ToList().ForEach(d => {
                var curpartition = d.BSDismemberBodyPartTypeToPartitionType();
                if (!results.Contains(curpartition)) results.Add(curpartition);
            });
            return results.ToArray();
        }
        public static Partitions BSDismemberBodyPartTypeToPartitionType(this BSDismemberBodyPartType bodypart)
        {
            if (bodypart.Contain(BSDismemberBodyPartType.BP_HEAD, BSDismemberBodyPartType.BP_HEAD2,
                           BSDismemberBodyPartType.BP_SECTIONCAP_HEAD, BSDismemberBodyPartType.BP_SECTIONCAP_HEAD2,
                           BSDismemberBodyPartType.BP_TORSOCAP_HEAD, BSDismemberBodyPartType.BP_TORSOCAP_HEAD2,
                           BSDismemberBodyPartType.BP_TORSOSECTION_HEAD, BSDismemberBodyPartType.BP_TORSOSECTION_HEAD2,
                           BSDismemberBodyPartType.SBP_30_HEAD, BSDismemberBodyPartType.SBP_130_HEAD,
                           BSDismemberBodyPartType.SBP_230_HEAD))
                return Partitions.Head;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_31_HAIR, BSDismemberBodyPartType.SBP_131_HAIR))
                return Partitions.Hair;
            else if (bodypart.Contain(BSDismemberBodyPartType.BP_TORSO, BSDismemberBodyPartType.SBP_32_BODY))
                return Partitions.Body;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_33_HANDS))
                return Partitions.Hands;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_34_FOREARMS))
                return Partitions.Forearms;
            if (bodypart.Contain(BSDismemberBodyPartType.SBP_35_AMULET, BSDismemberBodyPartType.BP_BRAIN))
                return Partitions.Amulet;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_36_RING))
                return Partitions.Ring;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_37_FEET))
                return Partitions.Feet;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_38_CALVES))
                return Partitions.Calves;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_39_SHIELD))
                return Partitions.Shield;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_40_TAIL))
                return Partitions.Tail;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_41_LONGHAIR, BSDismemberBodyPartType.SBP_141_LONGHAIR))
                return Partitions.LongHair;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_42_CIRCLET, BSDismemberBodyPartType.SBP_142_CIRCLET))
                return Partitions.Circlet;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_43_EARS, BSDismemberBodyPartType.SBP_143_EARS))
                return Partitions.Ears;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_44_DRAGON_BLOODHEAD_OR_MOD_MOUTH))
                return Partitions.FaceRomouth;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_45_DRAGON_BLOODWINGL_OR_MOD_NECK))
                return Partitions.Neck;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_46_DRAGON_BLOODWINGR_OR_MOD_CHEST_PRIMARY))
                return Partitions.Chest;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_47_DRAGON_BLOODTAIL_OR_MOD_BACK))
                return Partitions.Back;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_48_MOD_MISC1))
                return Partitions.MiscFX;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_49_MOD_PELVIS_PRIMARY))
                return Partitions.Pelvis;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_50_DECAPITATEDHEAD, BSDismemberBodyPartType.SBP_150_DECAPITATEDHEAD))
                return Partitions.DecapitatedHead;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_51_DECAPITATE))
                return Partitions.Decapitate;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_52_MOD_PELVIS_SECONDARY))
                return Partitions.Pelvis_2nd;
            else if (bodypart.Contain(BSDismemberBodyPartType.BP_RIGHTLEG, BSDismemberBodyPartType.BP_RIGHTLEG2, BSDismemberBodyPartType.BP_RIGHTLEG3,
                           BSDismemberBodyPartType.BP_SECTIONCAP_RIGHTLEG, BSDismemberBodyPartType.BP_SECTIONCAP_RIGHTLEG2, BSDismemberBodyPartType.BP_SECTIONCAP_RIGHTLEG3,
                           BSDismemberBodyPartType.BP_TORSOCAP_RIGHTLEG, BSDismemberBodyPartType.BP_TORSOCAP_RIGHTLEG2, BSDismemberBodyPartType.BP_TORSOCAP_RIGHTLEG3,
                           BSDismemberBodyPartType.BP_TORSOSECTION_RIGHTLEG, BSDismemberBodyPartType.BP_TORSOSECTION_RIGHTLEG2, BSDismemberBodyPartType.BP_TORSOSECTION_RIGHTLEG3,
                           BSDismemberBodyPartType.SBP_53_MOD_LEG_RIGHT))
                return Partitions.Leg_Right;
            else if (bodypart.Contain(BSDismemberBodyPartType.BP_LEFTLEG, BSDismemberBodyPartType.BP_LEFTLEG2,
                                       BSDismemberBodyPartType.BP_LEFTLEG3, BSDismemberBodyPartType.BP_SECTIONCAP_LEFTLEG,
                                       BSDismemberBodyPartType.BP_SECTIONCAP_LEFTLEG2, BSDismemberBodyPartType.BP_SECTIONCAP_LEFTLEG3,
                                       BSDismemberBodyPartType.BP_TORSOCAP_LEFTLEG, BSDismemberBodyPartType.BP_TORSOCAP_LEFTLEG2,
                                       BSDismemberBodyPartType.BP_TORSOCAP_LEFTLEG3, BSDismemberBodyPartType.BP_TORSOSECTION_LEFTLEG,
                                       BSDismemberBodyPartType.BP_TORSOSECTION_LEFTLEG2, BSDismemberBodyPartType.BP_TORSOSECTION_LEFTLEG3,
                                       BSDismemberBodyPartType.SBP_54_MOD_LEG_LEFT))
                return Partitions.Leg_Left;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_55_MOD_FACE_JEWELRY))
                return Partitions.Face;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_56_MOD_CHEST_SECONDARY))
                return Partitions.Chest_2nd;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_57_MOD_SHOULDER))
                return Partitions.Shoulder;
            if (bodypart.Contain(BSDismemberBodyPartType.BP_LEFTARM, BSDismemberBodyPartType.BP_LEFTARM2,
                          BSDismemberBodyPartType.BP_SECTIONCAP_LEFTARM, BSDismemberBodyPartType.BP_SECTIONCAP_LEFTARM2,
                          BSDismemberBodyPartType.BP_TORSOCAP_LEFTARM, BSDismemberBodyPartType.BP_TORSOCAP_LEFTARM2,
                          BSDismemberBodyPartType.BP_TORSOSECTION_LEFTARM, BSDismemberBodyPartType.BP_TORSOSECTION_LEFTARM2,
                          BSDismemberBodyPartType.SBP_58_MOD_ARM_LEFT))
                return Partitions.Arm_Left;
            else if (bodypart.Contain(BSDismemberBodyPartType.BP_RIGHTARM, BSDismemberBodyPartType.BP_RIGHTARM2,
                          BSDismemberBodyPartType.BP_SECTIONCAP_RIGHTARM, BSDismemberBodyPartType.BP_SECTIONCAP_RIGHTARM2,
                          BSDismemberBodyPartType.BP_TORSOCAP_RIGHTARM, BSDismemberBodyPartType.BP_TORSOCAP_RIGHTARM2,
                          BSDismemberBodyPartType.BP_TORSOSECTION_RIGHTARM, BSDismemberBodyPartType.BP_TORSOSECTION_RIGHTARM2,
                          BSDismemberBodyPartType.SBP_59_MOD_ARM_RIGHT))
                return Partitions.Arm_Right;
            else if (bodypart.Contain(BSDismemberBodyPartType.SBP_60_MOD_MISC2))
                return Partitions.Misc_or_FX;
            if (bodypart.Contain(BSDismemberBodyPartType.SBP_61_FX01))
                return Partitions.FX01;
            return 0;
        }
    }
}
