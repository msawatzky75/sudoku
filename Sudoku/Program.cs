using System.Text;
using Sudoku;
using Sudoku.WaveFunction;


var gridSize = 3;
var weights = new List<ItemWeight<char>>
{
	new() { Value = '1' },
	new() { Value = '2' },
	new() { Value = '3' },
	new() { Value = '4' },
	new() { Value = '5' },
	new() { Value = '6' },
	new() { Value = '7' },
	new() { Value = '8' },
	new() { Value = '9' },
	// new() { Value = '0' },
	// new() { Value = 'A' },
	// new() { Value = 'B' },
	// new() { Value = 'C' },
	// new() { Value = 'D' },
	// new() { Value = 'E' },
	// new() { Value = 'F' },
};

var random = new Random(int.Parse(args[0] ?? "3"));
var model = new Model<char>((int)Math.Pow(gridSize, 4), weights, random, CreateGetAffectedBy(gridSize));

model.WaveFunction.ElementCollapsed += (sender, i) =>
{
	Console.Clear();
	var board = model.GetCollapsed(' ');
	var highlights = Helper.IndexesInSection(gridSize, i)
		.Concat(Helper.IndexesInRow(gridSize, i))
		.Concat(Helper.IndexesInColumn(gridSize, i));
	WriteBoard(board, gridSize, highlights.ToArray());
	if (!IsValidSudoku(board, gridSize, ' '))
	{
		Console.WriteLine("Invalid board");
		Console.ReadLine();
	}
	Thread.Sleep(30);
};
var output = model.Run();

Console.WriteLine(SerializeBoard(output, gridSize));

return;

Func<int, int[]> CreateGetAffectedBy(int gridSize)
{
	return index => Helper.GetAffected(index, gridSize)
		.Where(x => x != index)
		.ToArray();
}

bool IsValidSudoku<T>(T[] board, int gridSize, T nullValue)
{
	for (int i = 0; i < board.Length; i++)
	{
		var section = Helper.IndexesInSection(gridSize, i);
		var row = Helper.IndexesInRow(gridSize, i);
		var column = Helper.IndexesInColumn(gridSize, i);

		var all = section.Concat(row).Concat(column).Distinct().ToArray();

		var groups = all
			.Select(x => board[x])
			.GroupBy(x => x)
			.Where(g => !g.Key.Equals(nullValue))
			.ToList();

		if (groups.Any(g => g.Key.Equals(board[i]) && g.Count() > 1))
		{
			// Console.ReadLine();
			return false;
		}
		// if (groups.Any(group => group.Count() > 1 && !group.Key.Equals(nullValue) && !group.Key.Equals(board[i])))
		// {
		// 	return false;
		// }
	}

	return true;
}


void WriteBoard<T>(T?[] board, int gridSize, int[] highlight)
{
	var highlightColor = ConsoleColor.DarkCyan;

	int GetRow(int i) => i / (gridSize * gridSize);
	int GetColumn(int i) => i % (gridSize * gridSize);

	for (var index = 0; index < gridSize * gridSize * gridSize * gridSize; index++)
	{
		Console.ResetColor();
		var row = GetRow(index);
		var column = GetColumn(index);

		if ((index) % gridSize == 0)
			Console.Write('|');

		if (highlight.Contains(index))
		{
			Console.BackgroundColor = highlightColor;
			Console.Write('-');
		}
		else
			Console.Write(' ');

		Console.Write(board[index]);

		if (row != GetRow(index + 1))
		{
			Console.ResetColor();
			Console.WriteLine();
		}
	}
}

string SerializeBoard<T>(T?[] board, int gridSize, char verticalDelimiter = '|', char horizontalDelimiter = '=')
{
	var output = new StringBuilder();

	var horizontalDelimiterLine = Environment.NewLine
	                              + verticalDelimiter
	                              + horizontalDelimiter.Repeat((gridSize * gridSize) + gridSize - 1)
	                              + verticalDelimiter
	                              + Environment.NewLine;
	horizontalDelimiterLine = Environment.NewLine;


	for (var y = 0; y < gridSize * gridSize; y++)
	{
		Console.ResetColor();
		output.Append(horizontalDelimiterLine);
		output.Append(verticalDelimiter);
		for (var j = 0; j < gridSize * gridSize; j++)
		{
			var val = board[(y * gridSize * gridSize) + j];
			output.Append(val is null ? " " : val.ToString());

			if ((j + 1) % gridSize == 0)
				output.Append(verticalDelimiter);
		}
	}

	output.Append(horizontalDelimiterLine);

	return output.ToString();
}