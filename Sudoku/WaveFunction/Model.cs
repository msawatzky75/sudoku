using System.Diagnostics;

namespace Sudoku.WaveFunction;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Model<T> where T : IEquatable<T>
{
	/// <summary>
	/// Given an index and a value to put there, return a list of indexes that cannot have that same value.
	/// </summary>
	private readonly Func<int, T, Func<int, T[]>, int[]> _affectedBy;

	private readonly T _nullValue;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="outputSize"></param>
	/// <param name="items"></param>
	/// <param name="rand"></param>
	/// <param name="affectedBy">Given an affected index, and a value placed there, return a list of indices that can no longer have that value.</param>
	/// <param name="nullValue"></param>
	/// <typeparam name="T"></typeparam>
	public Model(int outputSize,
		ItemWeight<T>[] items,
		Random rand,
		Func<int, T, Func<int, T[]>, int[]> affectedBy,
		T nullValue)
	{
		_affectedBy = affectedBy;
		_nullValue = nullValue;
		Items = items;
		WaveFunction = new WaveFunction<T>(outputSize, items, rand);
	}

	public WaveFunction<T> WaveFunction { get; set; }
	public ItemWeight<T>[] Items { get; set; }

	public EventHandler<(int index, ItemWeight<T> selected)> OnIteration { get; set; } = delegate { };

	public EventHandler<int> OnPropagate { get; set; } = delegate { };

	public EventHandler<int> OnBacktrack { get; set; } = delegate { };

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

		return WaveFunction.GetAllCollapsed(_nullValue);
	}

	public void Iterate(int index, ItemWeight<T> option)
	{
		OnIteration(this, (index, option));

		var blacklist = new List<ItemWeight<T>>();
		while (!WaveFunction.IsFullyCollapsed())
		{
			var previous = WaveFunction.GetMatrix()[index].Clone();
			Stack<(int index, ItemWeight<T> constrained)> propagations = new();
			try
			{
				WaveFunction.SetOption(index, option);
				var propagated = Propagate(index, out propagations);
				if (WaveFunction.IsFullyCollapsed()) return;
				if (propagated)
				{
					var next = GetMinimumEntropyIndex();
					Iterate(next, WaveFunction.Collapse(next, []));
					return;
				}
			}
			catch (IterationException)
			{
				OnBacktrack(this, (index));
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
	/// <param name="propagations">A stack of all constrained indexes, in the order they happened.</param>
	/// <returns>True if propagation was successful, false if there was a failure</returns>
	public bool Propagate(int index,
		out Stack<(int index, ItemWeight<T> constrained)> propagations)
	{
		// Create a stack of element indexes that were affected, and need those consequences spread.
		var queue = new Queue<int>();
		propagations = new();

		OnPropagate(this, index);
		var possible = WaveFunction.GetWeights(index);

		// cannot propagate a collapsed cell, if it is not collapsed.
		if (possible.Count != 1)
			return false;

		// constrain affected from this value
		var item = possible[0];

		var affected = _affectedBy(index, item.Value,
			i => WaveFunction.GetWeights(i).Select(x => x.Value).ToArray());

		foreach (var otherIndex in affected)
		{
			// if an affected cell is already collapsed, skip it
			if (WaveFunction.GetWeights(otherIndex).Count == 1) continue;

			// if the affected cell doesn't have this cell's selected option, skip it.
			if (!WaveFunction.GetWeights(otherIndex).Contains(item)) continue;

			// remove this cell's selected option from affected cell
			propagations.Push((otherIndex, item));
			WaveFunction.ConstrainOption(otherIndex, item);

			// if the other cell is collapsed, propagate
			var other = WaveFunction.GetWeights(otherIndex);
			if (other.Count == 1)
				queue.Enqueue(otherIndex);

			// TODO: need someway to detect if a propagation failed, but this isn't it...
			// if the available options are less than the number of other indices to propagate, we have an issue 
			// else if (other.Count < queue.Count)
			// 	return false;
		}


		while (queue.Count > 0)
		{
			if (!Propagate(queue.Dequeue(), out var subPropagations))
				return false;

			foreach (var subPropagation in subPropagations)
			{
				propagations.Push(subPropagation);
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

	public bool SetInitial(T[] initialBoard)
	{
		for (int i = 0; i < initialBoard.Length; i++)
		{
			if (initialBoard[i].Equals(_nullValue)) continue;
			WaveFunction.SetOption(i, new ItemWeight<T> { Value = initialBoard[i], Weight = 1 });
			if (!Propagate(i, out var propagated))
				return false;
		}

		return true;
	}
}