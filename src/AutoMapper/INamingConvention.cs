namespace AutoMapper
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines a naming convention strategy
    /// </summary>
	public interface INamingConvention
	{
        /// <summary>
        /// Regular expression on how to tokenize a member
        /// </summary>
		Regex SplittingExpression { get; }

        /// <summary>
        /// Character to separate on
        /// </summary>
		string SeparatorCharacter { get; }
	}

    public class PascalCaseNamingConvention : INamingConvention
	{
        private readonly Regex _splittingExpression = new Regex(@"(\p{Lu}+(?=$|\p{Lu}[\p{Ll}0-9])|\p{Lu}?[\p{Ll}0-9]+)");

		public Regex SplittingExpression
		{
			get { return _splittingExpression; }
		}

		public string SeparatorCharacter
		{
			get { return string.Empty; }
		}
	}

	public class LowerUnderscoreNamingConvention : INamingConvention
	{
		private readonly Regex _splittingExpression = new Regex(@"[\p{Ll}0-9]+(?=_?)");

		public Regex SplittingExpression
		{
			get { return _splittingExpression; }
		}

		public string SeparatorCharacter
		{
			get { return "_"; }
		}
	}
}