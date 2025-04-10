// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Tests;
public partial class ProcessAsyncFunctionalTests
{
    public class SegmentDataComparer : IEqualityComparer<SegmentData>
    {
        public bool Equals(SegmentData? x, SegmentData? y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.Text == y.Text && x.MinProbability == y.MinProbability && x.Probability == y.Probability && x.Start == y.Start && x.End == y.End; // Compare by relevant properties
        }

        public int GetHashCode(SegmentData obj)
        {
            return obj.Text.GetHashCode();
        }
    }
}
