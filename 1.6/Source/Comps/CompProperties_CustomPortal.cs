using Verse;

namespace VanillaQuestsExpandedAncients
{
    public class CompProperties_CustomPortal : CompProperties
    {
        public string enterTexPath;
        public string viewDestinationTexPath;
        public string enterCommandKey;
        public string viewDestinationCommandKey;
        public string viewDestinationDescKey;

        public CompProperties_CustomPortal()
        {
            compClass = typeof(CompCustomPortal);
        }
    }
}