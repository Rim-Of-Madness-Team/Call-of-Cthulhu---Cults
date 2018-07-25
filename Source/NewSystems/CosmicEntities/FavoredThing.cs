using System.Xml;
using Verse;

namespace CultOfCthulhu
{
    public class FavoredThing
    {
        public string thingDef;
        public float favor;
        public FavoredThing()
        {
        }
        
        public FavoredThing(string thingDef, float favor)
        {
            this.thingDef = thingDef;
            this.favor = favor;
        }

        public string Summary
        {
            get
            {
                return this.favor.ToStringPercent() + " favor " + ((this.thingDef == null) ? "null" : this.thingDef);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            this.thingDef = (string)ParseHelper.FromString(xmlRoot.Name, typeof(string));
            this.favor = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }
        
        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "(",
                (this.thingDef == null) ? "null" : this.thingDef,
                " (",
                this.favor.ToStringPercent(),
                "% Favor)",
                ")"
            });
        }
        
    }
}