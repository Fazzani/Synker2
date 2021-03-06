﻿namespace hfa.WebApi.Models
{
    using hfa.Synker.Service.Entities.MediasRef;
    using Hfa.WebApi.Models;
    using Nest;
    public class MediaRefModel : MediaRef, IModel<MediaRef, MediaRefModel>
    {
        public new string Id { get; set; }
        public MediaRefModel ToModel(IHit<MediaRef> hit)
        {
            Id = hit.Id;
            Cultures = hit.Source.Cultures;
            DisplayNames = hit.Source.DisplayNames;
            Groups = hit.Source.Groups;
            MediaType = hit.Source.MediaType;
            Tvg = hit.Source.Tvg;
            return this;
        }
    }
}
