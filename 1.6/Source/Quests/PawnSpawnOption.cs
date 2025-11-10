using Verse;
using RimWorld;
using System.Xml;

namespace VanillaQuestsExpandedAncients
{
    public class PawnSpawnOption
    {
        public PawnKindDef kind;
        public IntRange count;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string defName = xmlRoot.Name;
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", defName);
            
            if (xmlRoot.FirstChild != null)
            {
                count = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
            }
            else
            {
                count = new IntRange(1, 1);
            }
        }
    }
}