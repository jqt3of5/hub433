using System.Linq;
using mqtt;
using NUnit.Framework;

namespace PipelineTests
{
    [TestFixture]
    public class RouteTrieTests
    {
        [Test]
        public void TestEmptyPath()
        {
            var trie = new TrieNode<string>();
            var path = "";
            var value = "test";
            trie.AddValue(path, value);

            Assert.That(trie.TryGetValue(path, out var values));
            Assert.That(values, Contains.Item(value));
        }

        [Test]
        public void TestRouteWithOneSubtopic()
        {
            var trie = new TrieNode<string>();
            var path = "a";

            var value = "test";
            trie.AddValue(path, value);

            Assert.That(trie.TryGetValue(path, out var values));
            Assert.That(values, Contains.Item(value));
        }

        [Test]
        public void TestRouteWithThreeSubtopics()
        {
            var trie = new TrieNode<string>();
            var path = "a/b/c";

            var value = "test";
            trie.AddValue(path, value);

            Assert.That(trie.TryGetValue(path, out var values));
            Assert.That(values, Contains.Item(value));
        }

        [Test]
        public void TestRouteWithWildcard()
        {
            var trie = new TrieNode<string>();
            var path = "a/+/c";

            var value = "test";
            trie.AddValue(path, value);

            var path2 = new[] {"a", "b", "c"};
            Assert.That(trie.TryGetValue(path2, out var values));
            Assert.That(values, Contains.Item(value));
        }

        [Test]
        public void TestWithMultipleRoute()
        {
            var trie = new TrieNode<string>();

            var path = "a/b/c";
            var value = "test";
            trie.AddValue(path, value);

            var path1 = "a/b/d";
            var value1 = "test1";
            trie.AddValue(path1, value1);

            var path2 = "b/c/d";
            var value2 = "test2";
            trie.AddValue(path2, value2);

            Assert.That(trie.TryGetValue(path2, out var values));
            Assert.That(values, Contains.Item(value2));
        }

        [Test]
        public void TestWithMultipleRoutesWithWildcardWithMatchingTopic()
        {
            var trie = new TrieNode<string>();

            var path = "a/b/c";
            var value = "test";
            trie.AddValue(path, value);

            var path1 = "a/+/c";
            var value1 = "test1";
            trie.AddValue(path1, value1);

            var path2 = "a/b/d";
            var value2 = "test2";
            trie.AddValue(path2, value2);

            Assert.That(trie.TryGetValue(path, out var values));
            Assert.That(values, Contains.Item(value));
        }

        [Test]
        public void TestWithMultipleRoutesWithWildcardWithNonMatchingTopic()
        {
            var trie = new TrieNode<string>();

            var path = "a/b/c";
            var value = "test";
            trie.AddValue(path, value);

            var path1 = "a/+/d";
            var value1 = "test1";
            trie.AddValue(path1, value1);

            var path2 = "a/b/d";
            Assert.That(trie.TryGetValue(path2, out var values));
            Assert.That(values, Contains.Item(value1));
        }


        [Test]
        public void TestEnumeratorWithMultipleRoutes()
        {
            var trie = new TrieNode<string>();

            var path = "a/b/c";
            trie.AddValue(path, path);

            path = "a/b/d";
            trie.AddValue(path, path);

            path = "a/c/d";
            trie.AddValue(path, path);

            path = "b/c/d";
            trie.AddValue(path, path);

            Assert.That(trie.Count(), Is.EqualTo(4));
        }
    }
}