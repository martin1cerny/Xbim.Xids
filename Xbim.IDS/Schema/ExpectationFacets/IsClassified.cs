namespace Xbim.IDS
{
    public partial class IsClassified : IFacet
    {
		public string ClassificationName { get; set; }

		public string ClassificationValue { get; set; }

		public IsClassifiedValueMode ValueMode { get; set; }
	}

    public enum IsClassifiedValueMode
    {
        Exact,
        Regex,
    }
}
