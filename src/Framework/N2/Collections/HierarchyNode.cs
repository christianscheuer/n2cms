using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using N2.Engine;
using System.Diagnostics;

namespace N2.Collections
{
	/// <summary>
	/// A node used by the hierarchy navigator to wrap the data beeing 
	/// navigated.
	/// </summary>
	/// <typeparam name="T">The type of data to wrap.</typeparam>
	[DebuggerDisplay("{Current} Count={Children}")]
	public class HierarchyNode<T>
	{
		/// <summary>Creates a new instance of the hierarchy node.</summary>
		/// <param name="current">The current node.</param>
		public HierarchyNode(T current)
		{
			Current = current;
			Children = new List<HierarchyNode<T>>();
		}

		/// <summary>Gets or sets the current node.</summary>
		public T Current { get; set; }

		/// <summary>Gets or sets the parent node.</summary>
		public HierarchyNode<T> Parent { get; set; }

		/// <summary>Gets a list of child nodes.</summary>
		public IList<HierarchyNode<T>> Children { get; set; }

		public string ToString(Func<T, string> begin, Func<T, string> indent, Func<T, string> outdent, Func<T, string> end)
		{
			using (var sw = new StringWriter())
			{
				Write(sw, begin, indent, outdent, end);
				return sw.ToString();
			}
		}

		public void Write(TextWriter writer, Func<T, string> begin, Func<T, string> indent, Func<T, string> outdent, Func<T, string> end)
		{
			writer.Write(begin(Current));
			if (Children.Count > 0)
			{
				writer.Write(indent(Current));
				foreach (var child in Children)
				{
					child.Write(writer, begin, indent, outdent, end);
				}
				writer.Write(outdent(Current));
			}
			writer.Write(end(Current));
		}

		public void Add(HierarchyNode<T> node)
		{
			Children.Add(node);
			node.Parent = this;
		}

		#region Equals & GetHashCode
		public override bool Equals(object obj)
		{
			if (Current == null)
				return false;
			var other = obj as HierarchyNode<T>;
			if (other == null)
				return false;

			return Current.Equals(other.Current);
		}
		public override int GetHashCode()
		{
			if (Current == null)
				return 0.GetHashCode();
			return Current.GetHashCode();
		}
		#endregion
	}
}
