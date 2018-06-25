namespace hfa.Synker.Service
{
    using hfa.Synker.Service.Services.Xmltv;
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class site
    {

        private SitePackChannel[] channelsField;

        private string generatorinfonameField;

        private string site1Field;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("channel", IsNullable = false)]
        public SitePackChannel[] channels
        {
            get
            {
                return this.channelsField;
            }
            set
            {
                this.channelsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("generator-info-name")]
        public string Generatorinfoname
        {
            get
            {
                return this.generatorinfonameField;
            }
            set
            {
                this.generatorinfonameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("site")]
        public string Site1
        {
            get
            {
                return this.site1Field;
            }
            set
            {
                this.site1Field = value;
            }
        }
    }

}
