[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class keyword
{
    private string langField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string lang
    {
        get
        {
            return this.langField;
        }
        set
        {
            this.langField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}