using Sudoku.WaveFunction;

namespace Sudoku;

public class Sudoku<T> where T : IConvertible
{
	/* 9x9 (gridsize 3) indexes
	  0, 1, 2, 3, 4, 5, 6, 7, 8,
	  9,10,11,12,13,14,15,16,17,
	 18,19,20,21,22,23,24,25,26,

	 27,28,29,30,31,32,33,34,35,
	 36,37,38,39,40,41,42,43,44,
	 45,46,47,48,49,50,51,52,53,

	 54,55,56,57,58,59,60,61,62,
	 63,64,65,66,67,68,69,70,71,
	 72,73,74,75,76,77,78,79,80,
	*/
	private int GridSize { get; set; }
	private ItemWeight<T>[] CellWeights { get; set; }
	private readonly Random Random;
	private readonly T NullValue;

	public readonly Model<T> Model;

	public Sudoku(T nullValue, int gridSize = 3, ItemWeight<T>[]? cellWeights = null, Random? random = null)
	{
		var finalSize = (int)Math.Pow(gridSize, 4);
		NullValue = nullValue;
		GridSize = gridSize;
		Random = random ?? new Random();
		CellWeights = cellWeights ?? Enumerable.Range(0, finalSize)
			.Select(x => new ItemWeight<T> { Value = (T)Convert.ChangeType(x, typeof(T)) })
			.ToArray();

		Model = new Model<T>(
			finalSize,
			CellWeights,
			Random,
			CreateGetAffectedBy(GridSize),
			NullValue
		);
	}

	public T[] Run()
	{
		ulong iterations = 0;
		Model.OnIteration += (sender, tuple) =>
		{
			iterations++;
			var (index, option, stackFrames) = tuple;
			Console.Clear();
			Console.WriteLine($"Iterations: {iterations}");
			WriteBoard(Model.WaveFunction.GetMatrix().ToBoard(NullValue), GridSize, GetAffected(index, GridSize));
		};

		ulong totalBacktracks = 0;
		List<int> backtracks = new List<int>();
		Model.OnBacktrack += (sender, tuple) =>
		{
			totalBacktracks++;
			if (totalBacktracks < 1_000) return;
			var (index, stackFrames) = tuple;
			backtracks.Add(index);
			var rankedBacktracks = backtracks.GroupBy(i => i).OrderBy(g => g.Count()).Select(g => (g.Key, g.Count())).ToList();
			if (rankedBacktracks.Count() <= 1) return;
			Console.Clear();
			Console.WriteLine(
				$"least common backtrack : {rankedBacktracks.Skip(1).First()}");
			// Console.WriteLine(stackFrames);
		};
		return Model.Run();
	}


	private static Func<int, T, Func<int, T[]>, int[]> CreateGetAffectedBy(int gridSize)
	{
		return (index, value, getValueAt) => GetAffected(index, gridSize)
			.Where(x => x != index)
			.ToArray();
	}

	public static int GetRow(int i, int gridSize) => i / (gridSize * gridSize);
	public static int GetColumn(int i, int gridSize) => i % (gridSize * gridSize);

	public static int[] IndexesInRow(int gridSize, int index)
	{
		var row = GetRow(index, gridSize);
		var items = new int[gridSize * gridSize];
		for (var i = 0; i < gridSize * gridSize; i++) items[i] = row * (gridSize * gridSize) + i;
		return items;
	}

	public static int[] IndexesInColumn(int gridSize, int index)
	{
		var column = GetColumn(index, gridSize);
		var items = new int[gridSize * gridSize];
		for (var i = 0; i < gridSize * gridSize; i++) items[i] = column + i * (gridSize * gridSize);
		return items;
	}

	public static int[] IndexesInSection(int gridSize, int index)
	{
		var sectionRow = GetRow(index, gridSize) / gridSize;
		var sectionColumn = GetColumn(index, gridSize) / gridSize;

		var xSectionStart = sectionColumn * gridSize;
		var ySectionStart = sectionRow * gridSize * gridSize * gridSize;

		var items = new int[gridSize * gridSize];
		for (var x = 0; x < gridSize; x++)
		for (var y = 0; y < gridSize; y++)
		{
			var xOffset = x;
			var yOffset = y * gridSize * gridSize;

			items[(y * gridSize) + x] = yOffset + ySectionStart + xOffset + xSectionStart;
		}

		return items;
	}

	public static int[] GetAffected(int index, int gridSize)
	{
		return IndexesInRow(gridSize, index)
			.Concat(IndexesInColumn(gridSize, index))
			.Concat(IndexesInSection(gridSize, index))
			.Distinct()
			.ToArray();
	}

	public static void WriteBoard(T?[] board, int gridSize, int[] highlight)
	{
		var highlightColor = ConsoleColor.DarkCyan;

		for (var index = 0; index < Math.Pow(gridSize, 4); index++)
		{
			Console.ResetColor();
			var row = GetRow(index, gridSize);
			var column = GetColumn(index, gridSize);

			// Horizontal separator
			if (row % gridSize == 0 && column == 0)
			{
				for (var y = 0; y < (gridSize * 2) * (gridSize + 1); y++)
					Console.Write("-");
				Console.WriteLine();
			}

			// Vertical separator
			if ((index) % gridSize == 0)
				Console.Write("| ");

			if (highlight.Contains(index))
			{
				Console.BackgroundColor = highlightColor;
				Console.Write('-');
			}
			else
				Console.Write(' ');

			Console.Write(board[index]);
			Console.ResetColor();


			if (row != GetRow(index + 1, gridSize))
				Console.WriteLine("| ");
		}
	}

	public static bool IsValidSudoku(T[] board, int gridSize, T nullValue, out List<int> invalid)
	{
		invalid = new List<int>();

		for (int i = 0; i < board.Length; i++)
		{
			var section = IndexesInSection(gridSize, i);
			var row = IndexesInRow(gridSize, i);
			var column = IndexesInColumn(gridSize, i);

			var all = section.Concat(row).Concat(column).Distinct().ToArray();

			var groups = all
				.Select(x => board[x])
				.GroupBy(x => x)
				.Where(g => !g.Key!.Equals(nullValue))
				.ToList();

			if (groups.Any(g => g.Key!.Equals(board[i]) && g.Count() > 1))
			{
				invalid.Add(i);
			}
		}

		return invalid.Count == 0;
	}
}