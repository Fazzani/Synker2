﻿namespace Hfa.WebApi.Models
{
    using System.Collections.Generic;
    public class QueryListBaseModel
    {
        public Dictionary<string, string> SearchDict { get; set; }
        public Dictionary<string, SortDirectionEnum> SortDict { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int Skip => PageNumber * PageSize;

        public bool GetAll { get; set; } = false;

        public override string ToString() => $"{Skip}:{SearchDict}:{SortDict}";
        public override int GetHashCode() => Skip.GetHashCode() ^ SortDict?.GetHashCode() ?? 0 ^ SearchDict?.GetHashCode() ?? 0;

    }

    public enum SortDirectionEnum
    {
        Asc = 1,
        Desc = 2
    }
}
