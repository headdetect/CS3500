// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)

// Edited and assignment done by Brayden Lopez

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
        private readonly Dictionary<string, List<string>> _collectionDependencies;
        private readonly Dictionary<string, List<string>> _collectionDependees;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            _collectionDependencies = new Dictionary<string, List<string>>();
            _collectionDependees = new Dictionary<string, List<string>>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get
            {
                return _collectionDependencies.Sum(entry => entry.Value.Count);
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s] => GetDependees(s).Count();


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return GetDependents(s).Any();
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return GetDependees(s).Any();
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            return from collection in _collectionDependees where collection.Value.Contains(s) select collection.Key;
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            return _collectionDependees.ContainsKey(s) ? _collectionDependees[s] : Enumerable.Empty<string>();
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
            if (!_collectionDependencies.ContainsKey(s))
                _collectionDependencies.Add(s, new List<string>());

            if (_collectionDependencies[s].Contains(t)) return;

            _collectionDependencies[s].Add(t);

            if (!_collectionDependees.ContainsKey(t))
                _collectionDependees.Add(t, new List<string>());
            
            _collectionDependees[t].Add(s);
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (!_collectionDependencies.ContainsKey(s)) return;

            _collectionDependencies[s].Remove(t);
            _collectionDependees[t].Remove(s);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (_collectionDependencies.ContainsKey(s))
            {
                foreach (var entry in _collectionDependencies[s])
                    _collectionDependees[entry].Remove(s);

                _collectionDependencies[s].Clear();

            }

            foreach (var dependent in newDependents)
                AddDependency(s, dependent);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            if (_collectionDependees.ContainsKey(s))
            {
                foreach (var entry in _collectionDependencies)
                    _collectionDependencies[entry.Key].Remove(s); // Remove self from dependencies. //

                _collectionDependees[s].Clear();
            }

            foreach (var dependee in newDependees) {
                if (!_collectionDependees.ContainsKey(s))
                    _collectionDependees.Add(s, new List<string>());

                _collectionDependees[s].Add(dependee);
            }
        }
        
    }
}


