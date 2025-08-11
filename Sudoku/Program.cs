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

char[] blank =
[
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',

	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',

	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
	' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
];

char[] initialBoard;

initialBoard =
[
	'7', ' ', '5', '9', ' ', ' ', ' ', ' ', '8',
	' ', ' ', '9', ' ', ' ', '1', ' ', '3', ' ',
	' ', '8', ' ', '6', ' ', ' ', '2', ' ', '5',

	'8', ' ', ' ', '3', ' ', ' ', '4', '5', '9',
	' ', '1', ' ', '5', ' ', '2', ' ', '7', ' ',
	'5', '7', '3', ' ', ' ', '6', ' ', ' ', '2',

	'2', ' ', '7', ' ', ' ', '9', ' ', '8', ' ',
	' ', '9', ' ', '7', ' ', ' ', '6', ' ', ' ',
	'1', ' ', ' ', ' ', ' ', '8', '9', ' ', '7',
];

// initialBoard =
// [
// 	'6', ' ', ' ', ' ', '3', '1', ' ', ' ', ' ',
// 	'9', ' ', '5', ' ', ' ', ' ', '2', '8', '1',
// 	' ', ' ', ' ', ' ', ' ', ' ', ' ', '3', '6',
//
// 	'5', ' ', '1', ' ', '4', '2', ' ', ' ', '3',
// 	'2', ' ', ' ', ' ', '1', ' ', ' ', ' ', '8',
// 	'4', ' ', ' ', '7', '8', ' ', ' ', ' ', '2',
//
// 	'3', '7', ' ', ' ', ' ', ' ', ' ', ' ', ' ',
// 	'8', '4', '9', ' ', ' ', ' ', '1', ' ', '7',
// 	' ', ' ', ' ', '6', '7', ' ', ' ', ' ', '9',
// ];

// initialBoard = blank;


var sudoku = new Sudoku<char>(
	initialBoard,
	' ',
	random: random,
	gridSize: 3,
	cellWeights: weights[0..(int)Math.Pow(gridSize, 2)]
);

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