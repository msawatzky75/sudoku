using System.Collections;
using System.Diagnostics;

namespace Sudoku.WaveFunction;

/// <summary>
/// 
/// </summary>
/// <param name="outputSize"></param>
/// <param name="items"></param>
/// <param name="rand"></param>
/// <param name="affectedBy">Given an affected index, and a value placed there, return a list of indices that can no longer have that value.</param>
/// <param name="nullValue"></param>
/// <typeparam name="T"></typeparam>
public class Model<T>(
	int outputSize,
	ItemWeight<T>[] items,
	Random rand,
	Func<int, T, Func<int, T[]>, int[]> affectedBy,
	T nullValue)
{
	public WaveFunction<T> WaveFunction { get; set; } = new(outputSize, items, rand);
	public ItemWeight<T>[] Items { get; set; } = items;

	public EventHandler<(int, ItemWeight<T>, int)> OnIteration { get; set; } = delegate { };
	public EventHandler<(int, StackTrace)> OnBacktrack { get; set; } = delegate { };

	public T[] Run()
	{
		var index = GetMinimumEntropyIndex();
		var option = WaveFunction.Collapse(index, []);
		try
		{
			Iterate(index, option);
		}
		catch (PropagationException e)
		{
			Console.WriteLine("Yeah that's not gonna work champ.");
			Console.WriteLine(e);
		}

		return WaveFunction.GetAllCollapsed(nullValue);
	}

	public void Iterate(int index, ItemWeight<T> option)
	{
		System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
		OnIteration(this, (index, option, stackTrace.FrameCount));

		var blacklist = new List<ItemWeight<T>>();
		while (!WaveFunction.IsFullyCollapsed())
		{
			var previous = WaveFunction.GetMatrix()[index].Clone();
			Stack<(int index, ItemWeight<T> constrained)> propagations = new();
			try
			{
				WaveFunction.SetOption(index, option);
				if (WaveFunction.IsFullyCollapsed()) return;
				if (Propagate(index, out propagations))
				{
					var next = GetMinimumEntropyIndex();
					Iterate(next, WaveFunction.Collapse(next, []));
					return;
				}
			}
			catch (IterationException)
			{
				OnBacktrack(this, (index, stackTrace));
				// just to pop the call stack, see throw below
			}

			// Undo the propagations from option selection
			UnPropagate(propagations);
			// remove the option from further selection
			blacklist.Add(option);
			// restore all previous options from before the selection
			WaveFunction.SetWeights(index, previous);

			// if we have blacklisted all options for this cell, we need to undo the previous iteration.
			if (blacklist.Count == previous.Count)
			{
				// iteration failure logic is the same a propagation failure logic,
				// but we need to escape 1 call from the call stack first.
				throw new IterationException($"Failed to iterate index {index}");
			}

			// select the next option that hasn't been blacklisted
			option = WaveFunction.Collapse(index, blacklist);
		}
	}

	/// <summary>
	/// Propagates the consequences of a collapsed cell. (removes that cell's value from affected indexes via
	/// <c>affectedBy</c> delegate.
	/// </summary>
	/// <param name="index">Index of collapsed cell</param>
	/// <param name="propagations">a stack of all constrained indexes, in the order they happened.</param>
	/// <returns>True if propagation was successful, false if there was a failure</returns>
	public bool Propagate(int index,
		out Stack<(int index, ItemWeight<T> constrained)> propagations)
	{
		// Create a stack of element indexes that were affected, and need those consequences spread.
		var queue = new Queue<int>();
		queue.Enqueue(index);
		propagations = new();

		// Spread consequences
		while (queue.Count > 0)
		{
			var workingIndex = queue.Dequeue();
			var possible = WaveFunction.GetWeights(workingIndex);
			if (possible.Count != 1) continue;

			// constrain affected from this value
			var item = possible[0];

			var affected = affectedBy(workingIndex, item.Value,
				i => WaveFunction.GetWeights(i).Select(x => x.Value).ToArray());

			foreach (var otherIndex in affected)
			{
				if (WaveFunction.GetWeights(otherIndex).Count == 1) continue;
				if (WaveFunction.GetWeights(otherIndex).Contains(item))
				{
					propagations.Push((otherIndex, item));
					WaveFunction.ConstrainOption(otherIndex, item);
				}

				var other = WaveFunction.GetWeights(otherIndex);
				if (other.Count == 1)
				{
					queue.Enqueue(otherIndex);
				}

				// if the available options are less than the number of other indices to propagate, we have an issue
				if (other.Count < queue.Count)
				{
					return false;
				}
			}
		}

		return true;
	}

	public void UnPropagate(Stack<(int index, ItemWeight<T> constrained)> propagations)
	{
		foreach (var (i, weight) in propagations)
		{
			WaveFunction.GetWeights(i).Add(weight);
		}
	}

	public int GetMinimumEntropyIndex()
	{
		var (minEntropy, minEntropyIndex) = WaveFunction
			.CoefficientMatrix
			.Select((x, i) => (x, i))
			.Where(x => x.x.Count > 1)
			.Select(x => (entropy: WaveFunction<T>.CalculateEntropy(x.x), i: x.i))
			// .Select(x => (entropy: rand.NextDouble() / 1000 * x.entropy, x.i))
			.MinBy(x => x.entropy);
		return minEntropyIndex;
	}

	public T[] GetCollapsed(T nullValue) => WaveFunction.GetAllCollapsed(nullValue);
}