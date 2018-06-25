namespace TvheadendLibrary.Common
{
    using static TvheadendLibrary.QueryParams;
    public static class FactoryQueryParamsFilter
    {
        public static IQueryParamsFilter CreateStringQueryParamsFilter(string field, string value) =>
            new StringQueryParamsFilter(field, value);

        public static IQueryParamsFilter CreateNumericQueryParamsFilter(string field, string value, string comparaison) =>
            new NumericQueryParamsFilter(field, value, comparaison);
    }
}
