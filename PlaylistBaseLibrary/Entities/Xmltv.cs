using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Xml.Serialization;

namespace PlaylistBaseLibrary.Entities
{

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class tv
    {
        public tv()
        {
            programme = new List<tvProgramme>();
            channel = new List<tvChannel>();
        }
        private List<tvChannel> channelField;

        private List<tvProgramme> programmeField;

        /// <remarks/>
        [XmlElementAttribute("channel")]
        public List<tvChannel> channel
        {
            get
            {
                return this.channelField;
            }
            set
            {
                this.channelField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("programme")]
        public List<tvProgramme> programme
        {
            get
            {
                return this.programmeField;
            }
            set
            {
                this.programmeField = value;
            }
        }

        public static void Validate(tv tvInstance)
        {
            if (tvInstance == null)
                tvInstance = new tv();
            if (tvInstance.programme == null)
                tvInstance.programme = new List<tvProgramme>();
            if (tvInstance.channel == null)
                tvInstance.channel = new List<tvChannel>();
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvChannel : IEqualityComparer<tvChannel>
    {

        public tvChannel()
        {
            displayname = new List<string>();
        }
        private List<string> displaynameField;

        private tvChannelIcon iconField;

        private string idField;

        /// <remarks/>
        [XmlElementAttribute("display-name")]
        public List<string> displayname
        {
            get
            {
                return this.displaynameField;
            }
            set
            {
                this.displaynameField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute()]
        public tvChannelIcon icon
        {
            get
            {
                return this.iconField;
            }
            set
            {
                this.iconField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }


        public override string ToString()
        {
            return $"{string.Join(", ", displayname)}";
        }

        public bool Equals(tvChannel x, tvChannel y) => x.id == y.id;

        public int GetHashCode(tvChannel obj) => obj.id.GetHashCode();
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvChannelIcon
    {

        private string srcField;

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string src
        {
            get
            {
                return this.srcField;
            }
            set
            {
                this.srcField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgramme : IEqualityComparer<tvProgramme>
    {

        public override string ToString()
        {
            return $"{channelField} : {DefaultTitle} => {start} : {stop} ";
        }

        public bool Equals(tvProgramme x, tvProgramme y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one object null the return false
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(tvProgramme obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return 0;
            }

            int referencehHash =
                obj.Id == null ? 0 : obj.Id.GetHashCode();

            return referencehHash;
        }

        public static bool operator ==(tvProgramme x, tvProgramme y)
        {
            // If reference same object including null then return true
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            // If one object null the return false
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
            {
                return false;
            }

            // Compare object property values
            return x.Id == y.Id;
        }

        public static bool operator !=(tvProgramme x, tvProgramme y) => !(x == y);

        public override bool Equals(object obj)
        {
            // If comparing to null return false
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            if (obj is tvProgramme prog)
                return Equals(this, prog);
            return false;
        }

        public string DefaultTitle { get { return titleField.FirstOrDefault().Value; } }

        private const string formatDateTime = "yyyyMMddHHmmss zzz";

        private List<tvProgrammeTitle> titleField;

        private List<tvProgrammeDesc> descField;

        private tvProgrammeCredits creditsField;

        private string dateField;

        private List<tvProgrammeCategory> categoryField;

        private tvProgrammeLength lengthField;

        private tvProgrammeIcon iconField;

        private tvProgrammeVideo videoField;

        private tvProgrammeAudio audioField;

        private object previouslyshownField;

        private tvProgrammeSubtitles subtitlesField;

        private tvProgrammeRating ratingField;

        private tvProgrammeStarrating starratingField;

        private tvProgrammeReview reviewField;

        private string startField;

        private string stopField;

        private string showviewField;

        private string channelField;

        [XmlIgnore]
        public string Id
        {
            get { return $"{channel}-{StartTime.ToString(formatDateTime)}"; }
        }

        /// <remarks/>
        [XmlElementAttribute("title")]
        public List<tvProgrammeTitle> title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("desc")]
        public List<tvProgrammeDesc> desc
        {
            get
            {
                return this.descField;
            }
            set
            {
                this.descField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeCredits credits
        {
            get
            {
                return this.creditsField;
            }
            set
            {
                this.creditsField = value;
            }
        }

        /// <remarks/>
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("category")]
        public List<tvProgrammeCategory> category
        {
            get
            {
                return this.categoryField;
            }
            set
            {
                this.categoryField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeLength length
        {
            get
            {
                return this.lengthField;
            }
            set
            {
                this.lengthField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeIcon icon
        {
            get
            {
                return this.iconField;
            }
            set
            {
                this.iconField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeVideo video
        {
            get
            {
                return this.videoField;
            }
            set
            {
                this.videoField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeAudio audio
        {
            get
            {
                return this.audioField;
            }
            set
            {
                this.audioField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("previously-shown")]
        public object previouslyshown
        {
            get
            {
                return this.previouslyshownField;
            }
            set
            {
                this.previouslyshownField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeSubtitles subtitles
        {
            get
            {
                return this.subtitlesField;
            }
            set
            {
                this.subtitlesField = value;
            }
        }

        [XmlElementAttribute("episode-num")]
        public List<string> tvProgrammeepisodeNum
        {
            get; set;
        }

        /// <remarks/>
        public tvProgrammeRating rating
        {
            get
            {
                return this.ratingField;
            }
            set
            {
                this.ratingField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("star-rating")]
        public tvProgrammeStarrating starrating
        {
            get
            {
                return this.starratingField;
            }
            set
            {
                this.starratingField = value;
            }
        }

        /// <remarks/>
        public tvProgrammeReview review
        {
            get
            {
                return this.reviewField;
            }
            set
            {
                this.reviewField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string start
        {
            get
            {
                return this.startField;
            }
            set
            {
                this.startField = value;
            }
        }

        public DateTime StartTime => DateTime.ParseExact(start, formatDateTime, CultureInfo.CurrentCulture);
        public DateTime EndTime => DateTime.ParseExact(stop, formatDateTime, CultureInfo.CurrentCulture);

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string stop
        {
            get
            {
                return this.stopField;
            }
            set
            {
                this.stopField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string showview
        {
            get
            {
                return this.showviewField;
            }
            set
            {
                this.showviewField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string channel
        {
            get
            {
                return this.channelField;
            }
            set
            {
                this.channelField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeTitle
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
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
        [XmlTextAttribute()]
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

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeDesc
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
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
        [XmlTextAttribute()]
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

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeCredits
    {
        private List<string> directorField;

        private List<string> actorField;

        private string composerField;

        public List<keyword> keyword { get; set; }
        public List<string> editor { get; set; }
        public List<string> guest { get; set; }
        public List<string> commentator { get; set; }
        public List<string> producer { get; set; }
        public List<string> adapter { get; set; }
        public List<string> writer { get; set; }
        /// <remarks/>
        public List<string> director
        {
            get
            {
                return this.directorField;
            }
            set
            {
                this.directorField = value;
            }
        }

        /// <remarks/>
        [XmlElementAttribute("actor")]
        public List<string> actor
        {
            get
            {
                return this.actorField;
            }
            set
            {
                this.actorField = value;
            }
        }

        /// <remarks/>
        public string composer
        {
            get
            {
                return this.composerField;
            }
            set
            {
                this.composerField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeCategory
    {

        private string langField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
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
        [XmlTextAttribute()]
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

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeLength
    {

        private string unitsField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        /// <remarks/>
        [XmlTextAttribute()]
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

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeIcon
    {

        private string srcField;

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string src
        {
            get
            {
                return this.srcField;
            }
            set
            {
                this.srcField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeVideo
    {

        private string aspectField;

        private string qualityField;

        /// <remarks/>
        public string aspect
        {
            get
            {
                return this.aspectField;
            }
            set
            {
                this.aspectField = value;
            }
        }

        /// <remarks/>
        public string quality
        {
            get
            {
                return this.qualityField;
            }
            set
            {
                this.qualityField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeAudio
    {

        private string stereoField;

        /// <remarks/>
        public string stereo
        {
            get
            {
                return this.stereoField;
            }
            set
            {
                this.stereoField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeSubtitles
    {
        private List<string> languageField;

        private string typeField;

        /// <remarks/>
        public List<string> origlanguage
        {
            get; set;
        }

        /// <remarks/>
        public List<string> language
        {
            get
            {
                return this.languageField;
            }
            set
            {
                this.languageField = value;
            }
        }

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeRating
    {

        private string valueField;

        private string systemField;

        /// <remarks/>
        public string value
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

        /// <remarks/>
        [XmlAttributeAttribute()]
        public string system
        {
            get
            {
                return this.systemField;
            }
            set
            {
                this.systemField = value;
            }
        }
    }

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeStarrating
    {

        private string valueField;

        /// <remarks/>
        public string value
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

    /// <remarks/>
    [XmlTypeAttribute(AnonymousType = true)]
    public partial class tvProgrammeReview
    {

        private string langField;

        private string typeField;

        private string valueField;

        /// <remarks/>
        [XmlAttributeAttribute()]
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
        [XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [XmlTextAttribute()]
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
}
