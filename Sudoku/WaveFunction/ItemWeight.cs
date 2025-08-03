using System.ComponentModel.DataAnnotations;

namespace Sudoku.WaveFunction;

public class ItemWeight<T> : IComparable<ItemWeight<T>>
{
	public required T Value { get; set; }
	[Range(0.0, 1.0)] public double Weight { get; set; } = 0.5;

	public int CompareTo(ItemWeight<T>? other)
	{
		if (other == null) return 1;
		return Weight.CompareTo(other.Weight);
	}
	
	public override string ToString() => Value?.ToString() ?? string.Empty;
}