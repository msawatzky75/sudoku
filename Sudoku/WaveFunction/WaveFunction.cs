using System.Collections;

namespace Sudoku.WaveFunction;

public class WaveFunction<T>(int size, ItemWeight<T>[] itemWeights, Random rand)
{
	public ItemWeight<T>[] Items { get; } = itemWeights;
	private Random Random { get; } = rand;
	public List<ItemWeight<T>>[] CoefficientMatrix { get; set; } = CreateCoefficientMatrix(size, itemWeights);

	public EventHandler<int> ElementCollapsed = delegate { };

	/// <summary>
	/// Calculates the entropy (complexity) of a tile's weights.
	/// </summary>
	/// <returns>Numerical representation of the entropy of the tile's options.</returns>
	public static double CalculateEntropy(List<ItemWeight<T>> items) => CalculateEntropy(items.Sum(x => x.Weight));

	/// <summary>
	/// Calculates the entropy (complexity) of a tile's weights.
	/// </summary>
	/// <param name="sumWeight">A sum of all weights for a given tile's options.</param>
	/// <returns>Numerical representation of the entropy of the tile's options.</returns>
	public static double CalculateEntropy(double sumWeight) =>
		(Math.Log(sumWeight) - (sumWeight * Math.Log(sumWeight))) / -sumWeight;

	private static List<ItemWeight<T>>[] CreateCoefficientMatrix(int size, ItemWeight<T>[] items) =>
		Enumerable.Range(0, size)
			.Select(_ => items.Select(x => x).ToList())
			.ToArray();

	public List<ItemWeight<T>> GetWeights(int index) => CoefficientMatrix[index];

	public List<ItemWeight<T>> SetWeights(int index, List<ItemWeight<T>> items) =>
		CoefficientMatrix[index] = items.Clone();

	/// <summary>
	/// TODO: validate that the ItemWeight Param is actually in the matrix first?
	/// </summary>
	/// <param name="index"></param>
	/// <param name="weight"></param>
	public void SetOption(int index, ItemWeight<T> weight)
	{
		CoefficientMatrix[index] = [weight];
		ElementCollapsed(this, index);
	}

	public bool ConstrainOption(int index, ItemWeight<T> weight)
	{
		var item = CoefficientMatrix[index];
		var status = item.Remove(weight);
		if (item.Count == 1) ElementCollapsed(this, index);
		return status;
	}

	public T GetCollapsed(int index)
	{
		var options = GetWeights(index);
		if (options.Count == 1) return options[0].Value;
		throw new Exception($"Cell at {index} is not collapsed.");
	}

	public List<ItemWeight<T>>[] GetMatrix() => CoefficientMatrix;

	public T[] GetAllCollapsed(T nullValue)
	{
		return CoefficientMatrix.Select(x =>
		{
			if (x.Count == 1) return x[0].Value;
			return nullValue;
		}).ToArray();
	}

	public bool IsFullyCollapsed() => CoefficientMatrix.Select(x => x.Count).All(x => x == 1);

	/// <summary>
	/// Collapses the wave function at the given index to a single option.
	/// </summary>
	/// <param name="index"></param>
	/// <exception cref="ArgumentException"></exception>
	public ItemWeight<T> Collapse(int index, IEnumerable<ItemWeight<T>> blacklist)
	{
		if (index >= CoefficientMatrix.Length)
			throw new ArgumentException("value too large for matrix size", nameof(index));
		var possible = GetWeights(index).Where(x => !blacklist.Contains(x)).ToArray();

		if (possible.Length == 1) return possible[0];

		var totalWeight = possible.Sum(x => x.Weight);
		var bias = Random.NextDouble() * totalWeight;
		foreach (var item in possible)
		{
			bias -= item.Weight;
			if (bias > 0) continue;
			return item;
		}

		throw new Exception("Something went wrong while collapsing a cell.");
	}
}