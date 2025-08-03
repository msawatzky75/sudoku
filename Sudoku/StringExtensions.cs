namespace Sudoku;

public static class StringExtensions
{
	public static string Repeat(this char character, int count) => character.ToString().Repeat(count);

	public static string Repeat(this string input, int count)
	{
		var output = "";
		for (var i = 0; i < count; i++) output += input;
		return output;
	}
}