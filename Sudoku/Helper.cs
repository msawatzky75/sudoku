using System.Collections;
using Sudoku.WaveFunction;

namespace Sudoku;

public static class Helper
{
	public static T[] ToBoard<T>(this IEnumerable<IEnumerable<ItemWeight<T>>> matrix, T nullValue)
	{
		return matrix
			.Select(x =>
			{
				var itemWeights = x as ItemWeight<T>[] ?? x.ToArray();
				return itemWeights.Length == 1 ? itemWeights.First().Value : nullValue;
			})
			.ToArray();
	}

	public static List<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
	{
		var list = new List<T>();
		foreach (var item in listToClone) list.Add((T)item.Clone());
		return list;
	}
}