namespace PlaylistManager.Entities
{
    using hfa.PlaylistBaseLibrary.Entities;
    using PlaylistBaseLibrary.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Text.RegularExpressions;
    [Serializable]
    public class Media : IComparable<Media>, IComparable, IValidatableObject, IEquatable<Media>, IEqualityComparer<Media>
    {

        public Media()
        {
            Tags = new List<string>();
            Enabled = true;
            MediaType = MediaType.LiveTv;
            Lang = "fr";
            IsValid = true;
            MediaGroup = new MediaGroup();
        }

        public Media(string name, string url) : this()
        {
            DisplayName = Name = name;
            Url = url;
        }

        #region Properties 
        private string _id;
        private string _dislayName;

        public string Id
        {
            get
            {
                if (_id == null && !string.IsNullOrEmpty(Url))
                    _id = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{new Uri(Url).IdnHost}{new Uri(Url).PathAndQuery}{Name}"), Base64FormattingOptions.None);
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Is valid media
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Header line separator
        /// </summary>
        public string StartLineHeader { get; set; }

        /// <summary>
        /// Culture
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Original Name
        /// </summary>
        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_dislayName))
                    _dislayName = Name;
                return _dislayName;
            }
            set { _dislayName = value; }
        }

        /// <summary>
        /// Position 
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Default Url
        /// </summary>
        [Required]
        public string Url { get; set; }

        public List<string> Tags { get; set; }

        public bool Enabled { get; set; }

        public MediaType MediaType { get; set; }

        public MediaGroup MediaGroup { get; set; }

        [NotMapped]
        public string Group => MediaGroup?.Name;

        public string GetTrimedDisplayName() => Regex.Replace(DisplayName, @"\s+", "");

        public int? GetChannelNumber() => GetChannelNumber(Name);
        public static int? GetChannelNumber(string mediaName)
        {
            var match = Regex.Match(mediaName, @"\b(?:[^\+])(?<number>\d{1,2})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (match.Success && match.Groups["number"].Success)
            {
                return Convert.ToInt32(match.Groups["number"]?.Value);
            }
            return null;
        }

        public string CleanNameForSearch() => CleanMediaNameForSearch(DisplayName);

        public static string CleanMediaNameForSearch(string mediaName)
        {
            mediaName = Regex.Replace(mediaName, "\\s(((?:f?h|s?){1}d\\b|\\d{2,3}0p)|\\+\\d|\\(.*\\))", string.Empty, RegexOptions.IgnoreCase);
            var match = Regex.Match(mediaName, "(?<name>[a-z\\s]+)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
            if (match.Success && match.Groups["name"].Success)
            {
                return match.Groups["name"].Value;
            }
            return mediaName;
        }

        #endregion

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is Media))
                throw new ArgumentException("Must be a media type");

            return Position.CompareTo(((Media)obj).Position);
        }

        public int CompareTo(Media other)
        {
            if (other == null)
                return 1;
            return Url.CompareTo(other.Url);
        }

        public static bool operator ==(Media m1, Media m2) => m1?.Url == m2?.Url && m1?.Name == m1?.Name && m2?.Lang == m2?.Lang;
        public static bool operator !=(Media m1, Media m2) => m1.Url != m2.Url || m1.Name != m2.Name || m1?.Lang != m2?.Lang;
        public static bool operator >(Media m1, Media m2) => m1.Url.CompareTo(m2.Url) > 0;
        public static bool operator <(Media m1, Media m2) => m1.Url.CompareTo(m2.Url) < 0;
        public static bool operator >=(Media m1, Media m2) => m1.Url.CompareTo(m2.Url) >= 0;
        public static bool operator <=(Media m1, Media m2) => m1.Url.CompareTo(m2.Url) <= 0;

        public bool Equals(Media other) => Url.Equals(other.Url) && Name.Equals(other.Name) && Lang.Equals(other.Lang);

        public override bool Equals(object obj)
        {
            Media m = obj as Media;
            if (m == null)
                return false;
            return m.Url == Url && m.Name == Name && m.Lang == Lang;
        }

        public override int GetHashCode() => Url.GetHashCode();

        public override string ToString() => $"{Position} {Name} ({Lang})";

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Name))
                yield return new ValidationResult("Media name required");
            if (string.IsNullOrEmpty(Url))
                yield return new ValidationResult("Media url required");
        }

        public virtual string Format(IMediaFormatter mediaFormatter) => mediaFormatter.Format(this);

        public bool Equals(Media x, Media y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null | y == null)
                return false;
            else if (x.Url == y.Url && x.Name == y.Name && x.Lang == y.Lang)
                return true;
            return false;
        }

        public int GetHashCode(Media obj)
        {
            var code = obj.Url.GetHashCode() ^ obj.Name.GetHashCode() ^ obj.Lang.GetHashCode();
            return code.GetHashCode();
        }
    }

    public enum MediaType : Byte
    {
        LiveTv = 0,
        Radio,
        /// <summary>
        /// video file
        /// </summary>
        Video,
        /// <summary>
        /// audio file
        /// </summary>
        Audio,
        Other
    }
}
