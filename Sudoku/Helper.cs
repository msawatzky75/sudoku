namespace Sudoku;

public class Helper
{
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
}