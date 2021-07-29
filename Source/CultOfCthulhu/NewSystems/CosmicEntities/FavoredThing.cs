using System.Xml;
using Verse;

namespace CultOfCthulhu
{
    public class FavoredThing
    {
        public float favor;
        public string thingDef;

        public FavoredThing()
        {
        }

        public FavoredThing(string thingDef, float favor)
        {
            this.thingDef = thingDef;
            this.favor = favor;
        }

        public string Summary => favor.ToStringPercent() + " favor " + (thingDef ?? "null");

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            thingDef = (string) ParseHelper.FromString(xmlRoot.Name, typeof(string));
            favor = (float) ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "(",
                thingDef ?? "null",
                " (",
                favor.ToStringPercent(),
                "% Favor)",
                ")"
            });
        }
    }
}