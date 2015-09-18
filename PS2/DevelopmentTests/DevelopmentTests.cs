using SpreadsheetUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PS2GradingTests
{
    /// <summary>
    ///  This is a test class for DependencyGraphTest
    /// 
    ///  These tests should help guide you on your implementation.  Warning: you can not "test" yourself
    ///  into correctness.  Tests only show incorrectness.  That being said, a large test suite will go a long
    ///  way toward ensuring correctness.
    /// 
    ///  You are strongly encouraged to write additional tests as you think about the required
    ///  functionality of yoru library.
    /// 
    ///</summary>
    [TestClass]
    public class DependencyGraphTest
    {
        // ************************** TESTS ON EMPTY DGs ************************* //

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest1()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest2()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependees("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest3()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependents("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest4()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependees("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest5()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependents("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod]
        public void EmptyTest6()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t["a"]);
        }

        /// <summary>
        ///Removing from an empty DG shouldn't fail
        ///</summary>
        [TestMethod]
        public void EmptyTest7()
        {
            DependencyGraph t = new DependencyGraph();
            t.RemoveDependency("a", "b");
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Adding an empty DG shouldn't fail
        ///</summary>
        [TestMethod]
        public void EmptyTest8()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod]
        public void EmptyTest9()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependents("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void EmptyTest10()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependees("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }


        /**************************** SIMPLE NON-EMPTY TESTS ****************************/

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest1()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        ///Slight variant
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "b");
            Assert.AreEqual(1, t.Size);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest3()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.IsFalse(t.HasDependees("a"));
            Assert.IsTrue(t.HasDependees("b"));
            Assert.IsTrue(t.HasDependents("a"));
            Assert.IsTrue(t.HasDependees("c"));
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest4()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            HashSet<String> aDents = new HashSet<String>(t.GetDependents("a"));
            HashSet<String> bDents = new HashSet<String>(t.GetDependents("b"));
            HashSet<String> cDents = new HashSet<String>(t.GetDependents("c"));
            HashSet<String> dDents = new HashSet<String>(t.GetDependents("d"));
            HashSet<String> eDents = new HashSet<String>(t.GetDependents("e"));
            HashSet<String> aDees = new HashSet<String>(t.GetDependees("a"));
            HashSet<String> bDees = new HashSet<String>(t.GetDependees("b"));
            HashSet<String> cDees = new HashSet<String>(t.GetDependees("c"));
            HashSet<String> dDees = new HashSet<String>(t.GetDependees("d"));
            HashSet<String> eDees = new HashSet<String>(t.GetDependees("e"));
            Assert.IsTrue(aDents.Count == 2 && aDents.Contains("b") && aDents.Contains("c"));
            Assert.IsTrue(bDents.Count == 0);
            Assert.IsTrue(cDents.Count == 0);
            Assert.IsTrue(dDents.Count == 1 && dDents.Contains("c"));
            Assert.IsTrue(eDents.Count == 0);
            Assert.IsTrue(aDees.Count == 0);
            Assert.IsTrue(bDees.Count == 1 && bDees.Contains("a"));
            Assert.IsTrue(cDees.Count == 2 && cDees.Contains("a") && cDees.Contains("d"));
            Assert.IsTrue(dDees.Count == 0);
            Assert.IsTrue(dDees.Count == 0);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest5()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.AreEqual(0, t["a"]);
            Assert.AreEqual(1, t["b"]);
            Assert.AreEqual(2, t["c"]);
            Assert.AreEqual(0, t["d"]);
            Assert.AreEqual(0, t["e"]);
        }

        /// <summary>
        ///Removing from a DG 
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest6()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.RemoveDependency("a", "b");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest7()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependents("a", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> aPends = new HashSet<string>(t.GetDependents("a"));
            Assert.IsTrue(aPends.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest8()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependees("c", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> cDees = new HashSet<string>(t.GetDependees("c"));
            Assert.IsTrue(cDees.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        // ************************** STRESS TESTS ******************************** //
        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest1()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        /// <summary>
        /// Just do a bunch of things
        ///</summary>
        [TestMethod()]
        public void StressTest2()
        {
            const string LONG_ASS_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int SIZE = 1000;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random(9834095);

            for (int i = 0; i < SIZE; i++)
            {
                string[] arrayOfScreamingChildren = Enumerable.Repeat(
                    new string(Enumerable.Repeat(LONG_ASS_STRING, SIZE)
                              .Select(pick => pick[random.Next(LONG_ASS_STRING.Length)])
                              .ToArray())
                , SIZE).Distinct().ToArray();

                var parent = random.Next('a', 'z').ToString();

                t.AddDependency(parent, "To Be Replaced");

                Assert.IsTrue(t.GetDependents(parent).First() == "To Be Replaced");

                t.ReplaceDependents(parent, arrayOfScreamingChildren);
                Assert.AreEqual(arrayOfScreamingChildren.Length, t.GetDependents(parent).Count());

                for (int k = 0; k < arrayOfScreamingChildren.Length; k++)
                {
                    t.ReplaceDependees(parent, arrayOfScreamingChildren.Skip(k));
                    Assert.AreEqual(arrayOfScreamingChildren.Length - k, t.GetDependees(parent).Count());
                }
            }
        }

        /// <summary>
        /// Just do a bunch of things
        ///</summary>
        [TestMethod()]
        public void StressTest3()
        {
            const string LONG_ASS_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int SIZE = 1000;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random(9834095);
            
            for (int i = 0; i < SIZE; i++)
            {
                string[] arrayOfScreamingChildren = Enumerable.Repeat(
                    new string(Enumerable.Repeat(LONG_ASS_STRING, SIZE)
                              .Select(pick => pick[random.Next(LONG_ASS_STRING.Length)])
                              .ToArray())
                , SIZE).Distinct().ToArray();

                var parent = random.Next('a', 'z').ToString();
                
                t.ReplaceDependents(parent, arrayOfScreamingChildren);
                for(int k = 0; k < arrayOfScreamingChildren.Length; k++)
                {
                    t.RemoveDependency(parent, arrayOfScreamingChildren[k]);
                    Assert.AreEqual(arrayOfScreamingChildren.Length - k - 1, t.GetDependents(parent).Count());
                }
            }
        }

        /// <summary>
        /// Same things should yield same result 
        ///</summary>
        [TestMethod()]
        public void StressTest4()
        {
            const int SIZE = 100;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random(4444);

            for (int i = 0; i < SIZE; i++)
            {
                var parent = (i + 'a').ToString();
                var child = random.Next().ToString();

                t.AddDependency("a", child);
                t.AddDependency("b", child);
                t.AddDependency("c", child);

                t.AddDependency(child, "a");
                t.AddDependency(child, "b");
                t.AddDependency(child, "c");


                Assert.IsTrue(IsSameArray(t.GetDependents("a").ToArray(), t.GetDependents("b").ToArray()));
                Assert.IsTrue(IsSameArray(t.GetDependents("b").ToArray(), t.GetDependents("c").ToArray()));
                Assert.IsTrue(IsSameArray(t.GetDependents("a").ToArray(), t.GetDependents("c").ToArray()));
            }
        }

        /// <summary>
        /// Different things should yield different result 
        ///</summary>
        [TestMethod()]
        public void StressTest5()
        {
            const int SIZE = 100;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random(4444);

            for (int i = 0; i < SIZE; i++)
            {
                var parent = (i + 'a').ToString();
                var child = random.Next().ToString();

                t.AddDependency("a", child);
                t.AddDependency("b", child);
                t.AddDependency("c", child);

                t.AddDependency(child, "a");
                t.AddDependency(child, "b");
                t.AddDependency(child, "c");

                t.AddDependency("a", child);
                t.AddDependency("b", child);
                t.AddDependency("c", child + "oink");

                t.AddDependency(child, "a");
                t.AddDependency(child, "b");
                t.AddDependency(child + "oink", "c");


                Assert.IsTrue(IsSameArray(t.GetDependents("a").ToArray(), t.GetDependents("b").ToArray()));
                Assert.IsFalse(IsSameArray(t.GetDependents("b").ToArray(), t.GetDependents("c").ToArray()));
                Assert.IsFalse(IsSameArray(t.GetDependents("a").ToArray(), t.GetDependents("c").ToArray()));
            }
        }

        private static bool IsSameArray(string[] a, string[] b)
        {
            if (a.Length != b.Length) return false;
            for(int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Constant growth
        ///</summary>
        [TestMethod()]
        public void StressTest6()
        {
            const int SIZE = 100;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            for (int i = 0; i < SIZE; i++)
            {
                var parent = (i + 'a').ToString();
                var child = (i - (char)201).ToString();

                t.AddDependency(parent, child);
                t.AddDependency(child, parent);
                Assert.AreEqual((i + 1) * 2, t.Size);
            }
        }

        /// <summary>
        /// Just insert a bunch of stuff
        ///</summary>
        [TestMethod()]
        public void StressTest7()
        {
            const int SIZE = 2000000;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random();
            
            for (int i = 0; i < SIZE; i++)
            {
                var parent = random.Next('a', 'z').ToString();
                var child = random.Next('a', 'z').ToString();

                t.AddDependency(parent, child);
                t.RemoveDependency(parent, child);
            }

            Assert.AreEqual(0, t.Size);
        }



        // ********************************** ANOTHER STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest8()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependents
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDents = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDents.Add(letters[j]);
                }
                t.ReplaceDependents(letters[i], newDents);

                foreach (string s in dents[i])
                {
                    dees[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDents)
                {
                    dees[s[0] - 'a'].Add(letters[i]);
                }

                dents[i] = newDents;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        /// <summary>
        /// Big string tests for time
        ///</summary>
        [TestMethod()]
        public void StressTest9()
        {
            const string LONG_ASS_STRING = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int SIZE = 10000;

            // Dependency graph
            DependencyGraph t = new DependencyGraph();
            var random = new Random(9834095);

            DateTime nowish = DateTime.Now;
            // Take a sample of how long it takes to normally compute //
            byte[] trash = new byte[SIZE];
            random.NextBytes(trash); // Fill random buffer of sorts //
            var timeSpan = DateTime.Now - nowish;

            if (timeSpan.Milliseconds > 2) // Find inconclusive if it took over 5 ms to fill buffer //
                Assert.Inconclusive("We cannot run this test, your computer is too slow.");

            
            for (int i = 0; i < SIZE; i++)
            {
                // Make a random string that's 1000 characters long. //
                var parent = new string(
                    Enumerable.Repeat(LONG_ASS_STRING, 1000)
                              .Select(pick => pick[random.Next(LONG_ASS_STRING.Length)])
                              .ToArray()
                );
                var child = random.Next('a', 'z').ToString();

                t.AddDependency(parent, child);
            }

            timeSpan = DateTime.Now - nowish;
            
            if (timeSpan.Milliseconds > SIZE * 2) // If it takes over 2 ms per item, we fail.  //
                Assert.Fail("Time exceeded");
        }

        /// <summary>
        /// Check internals
        ///</summary>
        [TestMethod()]
        public void StressTest10()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            var random = new Random(34353);

            const int SIZE = 10000;
            for (int i = 0; i < SIZE; i++)
            {
                var parent = ('a' + i).ToString();
                var child = random.Next('a', 'z').ToString();

                t.AddDependency(parent, child);
            }

            PrivateObject insideT = new PrivateObject(t);

            var collection = insideT.GetFieldOrProperty("collectionDependees") as Dictionary<string, List<string>>;

            Assert.IsNotNull(collection);
            Assert.AreEqual(25, collection.Count);
        }

        /// <summary>
        /// Check internals
        ///</summary>
        [TestMethod()]
        public void StressTest11()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            var random = new Random();

            const int SIZE = 10000;
            for (int i = 0; i < SIZE; i++)
            {
                var parent = ('a' + i).ToString();
                var child = random.Next('a', 'z').ToString();

                t.AddDependency(parent, child);
            }

            PrivateObject insideT = new PrivateObject(t);

            var collection = insideT.GetFieldOrProperty("collectionDependencies") as Dictionary<string, List<string>>;

            Assert.IsNotNull(collection);
            Assert.AreEqual(SIZE, collection.Count);
        }

        /// <summary>
        /// Test for checking order
        ///</summary>
        [TestMethod()]
        public void StressTest12()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            t.ReplaceDependees("b", new string[] { "a" });
            t.ReplaceDependees("c", new string[] { "a" });
            t.ReplaceDependees("d", new string[] { "a" });

            Assert.AreEqual(3, t.GetDependents("a").Count());
            Assert.AreEqual("b", t.GetDependents("a").ElementAt(0));
            Assert.AreEqual("c", t.GetDependents("a").ElementAt(1));
            Assert.AreEqual("d", t.GetDependents("a").ElementAt(2));
        }

        /// <summary>
        /// Make sure we aren't lost in translation
        ///</summary>
        [TestMethod()]
        public void StressTest13()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            t.AddDependency("a", "a");
            t.AddDependency("a", "a");

            t.AddDependency("a", "c");
            t.RemoveDependency("a", "c");

            t.ReplaceDependees("b", new string[] { "c" });

            Assert.AreEqual(1, t.GetDependents("a").Count());
            Assert.AreEqual("a", t.GetDependents("a").First());
        }

        // ********************************** A THIRD STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest14()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 1000;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependees
            for (int i = 0; i < SIZE; i += 3)
            {
                HashSet<string> newDees = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 4)
                {
                    newDees.Add(letters[j]);
                }
                t.ReplaceDependees(letters[i], newDees);

                foreach (string s in dees[i])
                {
                    dents[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDees)
                {
                    dents[s[0] - 'a'].Add(letters[i]);
                }

                dees[i] = newDees;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        // ********************************** A THIRD STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest15()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependees
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDees = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDees.Add(letters[j]);
                }
                t.ReplaceDependees(letters[i], newDees);

                foreach (string s in dees[i])
                {
                    dents[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDees)
                {
                    dents[s[0] - 'a'].Add(letters[i]);
                }

                dees[i] = newDees;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }
    }
}
