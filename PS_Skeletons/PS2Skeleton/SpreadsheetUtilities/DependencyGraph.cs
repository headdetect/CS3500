// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// s1 depends on t1 --> t1 must be evaluated before s1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// (Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.)
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, Node<string>> collection;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            collection = new Dictionary<string, Node<string>>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get {
                var count = 0;
                foreach (var node in collection)
                    count += node.Value.CountDependencies();
                return count;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get { return GetDependees(s).Count(); }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return GetDependents(s).Count() > 0;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return GetDependees(s).Count() > 0;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            var find = searchNode(s);
            if (find == null) return Enumerable.Empty<string>();

            return find.Dependencies.Select(node => node.Item);
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            var find = searchNode(s);
            if (find == null) return Enumerable.Empty<string>();

            return find.Dependees.Select(node => node.Item);
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   s depends on t
        ///
        /// </summary>
        /// <param name="s"> s cannot be evaluated until t is</param>
        /// <param name="t"> t must be evaluated first.  S depends on T</param>
        public void AddDependency(string s, string t)
        {
            var node = searchNode(s);

            if (node == null)
            {
                // Node doesn't exist, let's add it //
                node = new Node<string>(s);
                node.AddDependency(t);
                collection.Add(s, node);
            }
            else
            {
                // Node exists, just add a dependency //
                node.AddDependency(s);
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            var node = searchNode(s);
            if (node == null) return;

            node.RemoveDependency(t);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            var node = searchNode(s);
            if (node == null) return;

            node.Dependencies.Clear();
            foreach (string t in newDependents)
                node.AddDependency(t);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            var node = searchNode(s);
            if (node == null) return;

            node.Dependees.Clear();
            foreach (string t in newDependees)
                node.Dependees.Add(new Node<string>(t));
        }

        private Node<string> searchNode(string s)
        {
            return collection.ContainsKey(s) ? collection[s] : null;
        }
    }

    internal class Node<T> where T : IComparable
    {
        /// <summary>
        /// The item in this node.
        /// </summary>
        public T Item { get; set; }
        
        /// <summary>
        /// A value that represents its priority in the graph. Will most likely represent the index (reversed), but might not.
        /// Will not be changed in this class, to be changed in the main class DependencyGraph
        /// </summary>
        internal int Priority { get; set; }

        /// <summary>
        /// The list of nodes that this node depends on.
        /// </summary>
        public List<Node<T>> Dependencies { get; set; }

        /// <summary>
        /// The list of nodes that depends on this one.
        /// </summary>
        public List<Node<T>> Dependees { get; private set; }

        private Node()
        {
            Dependencies = new List<Node<T>>();
            Dependees = new List<Node<T>>();
        }

        /// <summary>
        /// Creates a dependency-less/dependee-less node
        /// </summary>
        /// <param name="item">The item in this node</param>
        public Node(T item)
            : this()
        {
            Item = item;
        }

        /// <summary>
        /// Create a node with dependees
        /// </summary>
        /// <param name="item">The item of this node</param>
        /// <param name="AddDependency">The nodes that this node depends on</param>
        public Node(T item, params T[] dependencies)
            : this(item)
        {
            foreach (T dependency in dependencies)
            {
                AddDependency(dependency);
            }
        }

        public void AddDependency(T dependency)
        {
            var node = new Node<T>(dependency);
            node.Dependees.Add(this);
            Dependencies.Add(node);
        }


        public void RemoveDependency(T dependency)
        {
            Dependencies.RemoveAll(node => node.Item.Equals(dependency));
        }

        public int CountDependencies()
        {
            var count = Dependencies.Count;
            foreach (var node in Dependencies) {
                count += node.CountDependencies();
            }
            return count;
        }
    }
}


