﻿namespace hfa.WebApi.Models.TvgMedias
{
    using Hfa.WebApi.Models;
    using Nest;
    using PlaylistManager.Entities;
    public class TvgMediaModel : TvgMedia, IModel<TvgMedia, TvgMediaModel>
    {
        public TvgMediaModel ToModel(IHit<TvgMedia> hit)
        {
            Id = hit.Id;
            DisplayName = hit.Source.DisplayName;
            Enabled = hit.Source.Enabled;
            MediaGroup =  hit.Source.MediaGroup;
            IsValid = hit.Source.IsValid;
            Lang = hit.Source.Lang;
            MediaType = hit.Source.MediaType;
            Name = hit.Source.Name;
            Position = hit.Source.Position;
            StartLineHeader = hit.Source.StartLineHeader;
            Tags = hit.Source.Tags;
            Url = hit.Source.Url;
            return this;
        }
    }
}
