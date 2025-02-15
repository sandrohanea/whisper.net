// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Whisper.net.Internals;
internal class StringPool : IStringPool
{
    // Pool is organized by allocated capacity.
    // The keys (capacities) are kept in sorted order.
    private readonly SortedList<int, Stack<string>> pool = [];

    // We track the allocated capacity for each pooled string.
    // we store the allocated capacity which is >= their .Length.
    private readonly Dictionary<string, int> stringCapacities =
        new(ReferenceEqualityComparer<string>.Instance);

    /// <summary>
    /// Converts a native UTF8 pointer (null-terminated) into a pooled string.
    /// </summary>
    public unsafe string? GetStringUtf8(IntPtr nativePointer)
    {
        if (nativePointer == IntPtr.Zero)
        {
            return null;
        }

        // Determine the length in bytes by scanning for the null terminator.
        var byteLen = 0;
        while (Marshal.ReadByte(nativePointer, byteLen) != 0)
        {
            byteLen++;
        }

        // We'll use the byte count as an upper bound for the required char count.
        // (For multibyte characters the buffer might be a bit larger than needed,
        // which is acceptable for pooling.)
        var requiredCharCount = byteLen;

        // Try to get a modifiable candidate from the pool.
        var candidate = TryGetCandidateFromPool(requiredCharCount, out var candidateCapacity);
        if (candidate is null)
        {
#if NETSTANDARD
            candidate = new string('\0', requiredCharCount);
            candidateCapacity = requiredCharCount;
            stringCapacities[candidate] = candidateCapacity;
#else
            // On netcore, if no candidate is available, decode using the efficient helper.
            candidate = Marshal.PtrToStringUTF8(nativePointer);
            ArgumentNullException.ThrowIfNull(candidate, "Unable to decode native UTF8 string");
            candidateCapacity = candidate.Length;
            stringCapacities[candidate] = candidateCapacity;
            // The candidate is already decoded correctly—return it.
            return candidate;
#endif
        }

        // We have a candidate from the pool.
        // Decode the native UTF8 data into the candidate's buffer.
        var bytePtr = (byte*)nativePointer.ToPointer();
        fixed (char* dest = candidate)
        {
            var decodedChars = Encoding.UTF8.GetChars(bytePtr, byteLen, dest, candidateCapacity);
            // Only adjust the internal length if the decoded length is different.
            if (decodedChars != candidateCapacity)
            {
                UnsafeSetStringLength(candidate, decodedChars);
            }
        }

        return candidate;
    }

    /// <summary>
    /// Returns a string to the pool for later reuse.
    /// All strings created by the pool are re‑pooled for future use.
    /// </summary>
    public void ReturnString(string? returnedString)
    {
        if (returnedString is null)
        {
            return;
        }

        // Only pool strings that we have tracked.
        if (!stringCapacities.TryGetValue(returnedString, out var capacity))
        {
            return;
        }

        if (!pool.TryGetValue(capacity, out var stack))
        {
            stack = new Stack<string>();
            pool[capacity] = stack;
        }
        stack.Push(returnedString);
    }

    /// <summary>
    /// Attempts to locate a candidate string from the pool.
    /// Uses binary search on the sorted keys for efficiency.
    /// </summary>
    private string? TryGetCandidateFromPool(int requiredCharCount, out int candidateCapacity)
    {
        candidateCapacity = 0;
        string? candidate = null;

        // Binary search to find the first key that is >= requiredCharCount.
        var low = 0;
        var high = pool.Count - 1;
        var index = -1;
        while (low <= high)
        {
            var mid = (low + high) / 2;
            var key = pool.Keys[mid];
            if (key < requiredCharCount)
            {
                low = mid + 1;
            }
            else
            {
                index = mid;
                high = mid - 1;
            }
        }

        if (index != -1)
        {
            // First, try to find a candidate with capacity in the "ideal" range (<= requiredCharCount * 2).
            for (var i = index; i < pool.Count; i++)
            {
                var cap = pool.Keys[i];
                if (cap > requiredCharCount * 2)
                {
                    break;
                }

                if (pool.Values[i].Count > 0)
                {
                    candidate = pool.Values[i].Pop();
                    candidateCapacity = cap;
                    if (pool.Values[i].Count == 0)
                    {
                        pool.RemoveAt(i);
                    }
                    break;
                }
            }

            // If no candidate in the ideal range, choose the first available candidate with capacity >= required.
            if (candidate is null)
            {
                for (var i = index; i < pool.Count; i++)
                {
                    var cap = pool.Keys[i];
                    if (pool.Values[i].Count > 0)
                    {
                        candidate = pool.Values[i].Pop();
                        candidateCapacity = cap;
                        if (pool.Values[i].Count == 0)
                        {
                            pool.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }

        return candidate;
    }

    /// <summary>
    /// Unsafe method to modify the internal length of a string.
    /// CAUTION: This violates string immutability.
    /// </summary>
    private static unsafe void UnsafeSetStringLength(string str, int newLength)
    {
        fixed (char* pStr = str)
        {
            var pLength = (int*)pStr - 1;
            *pLength = newLength;
        }
    }

    /// <summary>
    /// A helper comparer that compares objects by reference.
    /// </summary>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static ReferenceEqualityComparer<T> Instance { get; } = new ReferenceEqualityComparer<T>();

        public bool Equals(T? x, T? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
