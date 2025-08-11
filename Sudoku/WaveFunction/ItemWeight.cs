using System.ComponentModel.DataAnnotations;

namespace Sudoku.WaveFunction;

public struct ItemWeight<T>() : IEquatable<ItemWeight<T>>, ICloneable
{
	public required T Value { get; set; }
	[Range(0.0, 1.0)] public double Weight { get; set; } = 0.5;

	public override string ToString() => Value?.ToString() ?? string.Empty;

	public object Clone() => new ItemWeight<T> { Value = Value, Weight = Weight };

	public bool Equals(ItemWeight<T> other)
	{
		return EqualityComparer<T>.Default.Equals(Value, other.Value);
	}

	public override bool Equals(object? obj)
	{
		return obj is ItemWeight<T> other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Value!.GetHashCode();
	}
}