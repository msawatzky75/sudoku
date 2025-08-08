using Sudoku;
using Sudoku.WaveFunction;


var seed = 4;
if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
	int.TryParse(args[0], out seed);
var random = new Random(seed);

ItemWeight<char>[] weights =
[
	new() { Value = '1' },
	new() { Value = '2' },
	new() { Value = '3' },
	new() { Value = '4' },
	new() { Value = '5' },
	new() { Value = '6' },
	new() { Value = '7' },
	new() { Value = '8' },
	new() { Value = '9' },
	new() { Value = '0' },
	new() { Value = 'A' },
	new() { Value = 'B' },
	new() { Value = 'C' },
	new() { Value = 'D' },
	new() { Value = 'E' },
	new() { Value = 'F' },
];

var gridSize = 3;

var sudoku = new Sudoku<char>(
	' ',
	random: random,
	gridSize: 3,
	cellWeights: weights[0..(int)Math.Pow(gridSize, 2)]
);
//
// sudoku.Model.WaveFunction.ElementCollapsed += (_, i) =>
// {
// 	Console.Clear();
// 	var board = sudoku.Model.GetCollapsed(' ');
// 	var highlights = Sudoku<char>.IndexesInSection(gridSize, i)
// 		.Concat(Sudoku<char>.IndexesInRow(gridSize, i))
// 		.Concat(Sudoku<char>.IndexesInColumn(gridSize, i));
// 	Sudoku<char>.WriteBoard(board, gridSize, highlights.ToArray());
//
// 	// Thread.Sleep(50);
// };

var output = sudoku.Run();
Console.Clear();
var valid = Sudoku<char>.IsValidSudoku(output, gridSize, ' ', out var errors);
if (!valid)
	Console.WriteLine("Invalid board");
else
	Console.WriteLine("Valid Board");

Sudoku<char>.WriteBoard(output, gridSize, errors.ToArray());

return !valid ? 1 : 0;

// var slc = new SeaLandCoast(9);
// var output = slc.Run();
//
// for (int x = 0; x < 9; x++)
// {
// 	for (int y = 0; y < 9; y++)
// 	{
// 		Console.Write(output[x + y * gridSize].ToString() + ' ');
// 	}
//
// 	Console.WriteLine();
// }