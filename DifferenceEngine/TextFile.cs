using System;
using System.IO;
using System.Collections;

namespace DifferenceEngine
{
	public class TextLine : IComparable
	{
		public string Line;
		public int _hash;

		public TextLine(string str)
		{
			Line = str.Replace("\t","    ");
			_hash = str.GetHashCode();
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return _hash.CompareTo(((TextLine)obj)._hash);
		}

		#endregion
	}


	public class DiffListTextFile : IDiffList
	{
		private const int MaxLineLength = 1024;
		private readonly ArrayList _lines;

		public DiffListTextFile(string fileName)
		{
			_lines = new ArrayList();
			using (var sr = new StreamReader(fileName)) 
			{
				string line;
				// Read and display lines from the file until the end of 
				// the file is reached.
				while ((line = sr.ReadLine()) != null) 
				{
					if (line.Length > MaxLineLength)
					{
						throw new InvalidOperationException(
                            $"File contains a line greater than {MaxLineLength} characters.");
					}
					_lines.Add(new TextLine(line));
				}
			}
		}
		#region IDiffList Members

		public int Count()
		{
			return _lines.Count;
		}

		public IComparable GetByIndex(int index)
		{
			return (TextLine)_lines[index];
		}

		#endregion
	
	}
}