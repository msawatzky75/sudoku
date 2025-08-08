using Sudoku.WaveFunction;

namespace Sudoku;

public enum Direction
{
	Up = 0,
	Down = 1,
	Left = 2,
	Right = 3
}

public class SeaLandCoast
{
	private const char Sea = 'S';
	private const char Coast = 'C';
	private const char Land = 'L';
	public int GridSize { get; }
	public ItemWeight<char>[] ItemWeights { get; }
	private const char _nullValue = ' ';
	private Random _random = new Random(0);
	public Model<char> Model { get; }

	public SeaLandCoast(int gridSize)
	{
		GridSize = gridSize;
		ItemWeights =
		[
			new() { Value = Sea, Weight = 0.6 },
			new() { Value = Coast, Weight = 0.1 },
			new() { Value = Land, Weight = 0.4 },
		];
		Model = new Model<char>(gridSize * gridSize, ItemWeights, _random, GetAffectedBy, _nullValue);
	}

	public char[] Run()
	{
		return Model.Run();
	}

	/// <summary>
	/// Given an affected index, and a value placed there, return a list of indices that can no longer have that value.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="placedItem"></param>
	/// <param name="getValuesAt"></param>
	/// <returns></returns>
	private int[] GetAffectedBy(int index, char placedItem, Func<int, char[]> getValuesAt)
	{
		char GetValue(int index)
		{
			var value = getValuesAt(index);
			return value.Length == 1 ? value[0] : _nullValue;
		}

		var affected = new List<int>();

		if (GetInDirection(index, Direction.Up, out var upIndex)
		    && CanBePlacedNextTo(placedItem, GetValue(upIndex)))
			affected.Add(upIndex);
		if (GetInDirection(index, Direction.Down, out var downIndex)
		    && CanBePlacedNextTo(placedItem, GetValue(downIndex)))
			affected.Add(downIndex);
		if (GetInDirection(index, Direction.Left, out var leftIndex)
		    && CanBePlacedNextTo(placedItem, GetValue(leftIndex)))
			affected.Add(leftIndex);
		if (GetInDirection(index, Direction.Right, out var rightIndex) &&
		    CanBePlacedNextTo(placedItem, GetValue(rightIndex)))
			affected.Add(rightIndex);


		return affected.ToArray();
	}

	public bool CanBePlacedNextTo(char placed, char placing) =>
		(placed, placing) switch
		{
			(Sea, Sea)
				or (Sea, Coast)
				or (Coast, Coast)
				or (Coast, Land)
				or (Coast, Sea)
				or (Land, Land)
				or (Land, Coast)
				=> true,
			_ => false
		};

	public static int GetRow(int i, int gridSize) => i / (gridSize);
	public static int GetColumn(int i, int gridSize) => i % (gridSize);

	public bool GetInDirection(int index, Direction direction, out int otherIndex)
	{
		var boardSize = (int)Math.Pow(GridSize, 2);
		otherIndex = -1;
		switch (direction)
		{
			case Direction.Up:
				if (index - GridSize >= 0)
				{
					otherIndex = index - GridSize;
					return true;
				}

				break;
			case Direction.Down:
				if (index + GridSize < boardSize)
				{
					otherIndex = index + GridSize;
					return true;
				}

				break;
			case Direction.Left:
				if (index - 1 >= 0)
				{
					otherIndex = index - 1;
					return true;
				}

				break;
			case Direction.Right:
				if (index + 1 < boardSize)
				{
					otherIndex = index + 1;
					return true;
				}

				break;
		}

		return false;
	}
}