// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }
}
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
    // Required by the compiler for 'init' setters and positional record types on pre-.NET 5 targets.
    internal static class IsExternalInit { }
}

namespace System.Collections.Generic
{
    /// <summary>Polyfill for IReadOnlySet{T}, which was added in .NET 5.</summary>
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        bool Contains(T value);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
    }
}

namespace Nullean.ScopedFileSystem
{
    /// <summary>
    /// Wraps a <see cref="HashSet{T}"/> to implement the <see cref="System.Collections.Generic.IReadOnlySet{T}"/>
    /// polyfill on netstandard2.0 and netstandard2.1 targets where <c>HashSet{T}</c> does not yet declare that interface.
    /// </summary>
    internal sealed class ReadOnlySetWrapper<T> : System.Collections.Generic.IReadOnlySet<T>
    {
        private readonly HashSet<T> _inner;

        internal ReadOnlySetWrapper(IEqualityComparer<T>? comparer = null) =>
            _inner = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);

        internal ReadOnlySetWrapper(IEnumerable<T> items, IEqualityComparer<T>? comparer = null) =>
            _inner = new HashSet<T>(items, comparer ?? EqualityComparer<T>.Default);

        public int Count => _inner.Count;
        public bool Contains(T value) => _inner.Contains(value);
        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        public bool IsProperSubsetOf(IEnumerable<T> other) => _inner.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _inner.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _inner.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _inner.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _inner.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => _inner.SetEquals(other);
    }
}
#endif
