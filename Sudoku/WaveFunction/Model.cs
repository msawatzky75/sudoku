using System.Collections;

namespace Sudoku.WaveFunction;

public class Model<T>(int outputSize, List<ItemWeight<T>> items, Random rand, Func<int, int[]> affectedBy)
{
	public WaveFunction<T> WaveFunction { get; set; } = new(outputSize, items, rand);
	public List<ItemWeight<T>> Items { get; set; } = items;

	public T[] Run()
	{
		while (!WaveFunction.IsFullyCollapsed()) Iterate();

		return WaveFunction.Select(x =>
		{
			if (x.Count != 1) throw new Exception("wtf bro");
			return x[0].Value;
		}).ToArray();
	}

	public T[] GetCollapsed(T nullValue)
	{
		return WaveFunction.Select(x =>
		{
			if (x.Count == 1) return x[0].Value;
			return nullValue;
		}).ToArray();
	}

	public void Iterate()
	{
		var index = GetMinimumEntropyIndex();
		WaveFunction.Collapse(index);
		Propagate(index);
	}

	public void Propagate(int index)
	{
		// Create a stack of element indexes that were affected, and need those consequences spread.
		var stack = new Stack<int>();
		stack.Push(index);

		// Spread consequences
		while (stack.Count > 0)
		{
			var workingIndex = stack.Pop();
			var possible = WaveFunction.GetWeights(workingIndex);
			var affected = affectedBy(workingIndex);

			if (possible.Count != 1) continue;

			// constrain affected from this value
			var item = possible[0];
			foreach (var otherIndex in affected)
			{
				if (WaveFunction.GetWeights(otherIndex).Count == 1) continue;
				WaveFunction.ConstrainOption(otherIndex, item);
				var other = WaveFunction.GetWeights(otherIndex);
				if (other.Count == 1) stack.Push(otherIndex);
			}
		}
	}

	public int GetMinimumEntropyIndex()
	{
		var (minEntropy, minEntropyIndex) = WaveFunction
			.Select((x, i) => (x, i))
			.Where(x => x.x.Count > 1)
			.Select(x => (entropy: WaveFunction<T>.CalculateEntropy(x.x), i: x.i))
			// .Select(x => (entropy: rand.NextDouble() / 1000 * x.entropy, x.i))
			.MinBy(x => x.entropy);
		return minEntropyIndex;
	}
}