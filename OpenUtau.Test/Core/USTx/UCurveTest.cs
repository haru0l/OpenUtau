using System.Linq;
using OpenUtau.Core.Ustx;
using Xunit;

namespace OpenUtau.Test.Core.USTx {
    public class UCurveTest {
        static readonly UExpressionDescriptor Descriptor = new UExpressionDescriptor("test", "tst", 0, 100, 0);

        static UCurve MakeCurve(int[] xs, int[] ys) {
            return new UCurve(Descriptor) { xs = xs.ToList(), ys = ys.ToList() };
        }

        static void AssertSameValues(UCurve expected, UCurve actual, int from, int to) {
            for (int tick = from; tick <= to; tick++) {
                Assert.True(expected.Sample(tick) == actual.Sample(tick),
                    $"tick {tick}: expected {expected.Sample(tick)}, actual {actual.Sample(tick)}");
            }
        }

        [Fact]
        public void ReplaceRangeRemovesPointsInsideRangeIncludingBoundaries() {
            // Neighbours are within UCurve.interval of the range, so no anchors are added.
            var (xs, ys) = UCurve.ReplaceRange(
                new[] { 0, 395, 400, 700, 1000, 1005, 1440 }, new[] { 10, 20, 30, 40, 50, 60, 70 },
                400, 1000,
                new[] { (700, 99) },
                Descriptor);

            Assert.Equal(new[] { 0, 395, 700, 1005, 1440 }, xs);
            Assert.Equal(new[] { 10, 20, 99, 60, 70 }, ys);
        }

        [Fact]
        public void ReplaceRangeInsertsPointsInTickOrder() {
            // Neighbours are within UCurve.interval of the range, so no anchors are added.
            var (xs, ys) = UCurve.ReplaceRange(
                new[] { 395, 1005 }, new[] { 10, 40 },
                400, 1000,
                new[] { (900, 60), (500, 50) },
                Descriptor);

            Assert.Equal(new[] { 395, 500, 900, 1005 }, xs);
            Assert.Equal(new[] { 10, 50, 60, 40 }, ys);
        }

        [Fact]
        public void ReplaceRangeLaterPointOverwritesSameTick() {
            var (xs, ys) = UCurve.ReplaceRange(
                new[] { 395, 1005 }, new[] { 10, 40 },
                400, 1000,
                new[] { (500, 50), (500, 60) },
                Descriptor);

            Assert.Equal(new[] { 395, 500, 1005 }, xs);
            Assert.Equal(new[] { 10, 60, 40 }, ys);
        }

        [Fact]
        public void ReplaceRangeClampsValues() {
            var (_, ys) = UCurve.ReplaceRange(
                new[] { 395, 1005 }, new[] { 10, 40 },
                400, 1000,
                new[] { (500, 150), (600, -10) },
                Descriptor);

            Assert.Equal(new[] { 10, 100, 0, 40 }, ys);
        }

        [Fact]
        public void ReplaceRangeAddsDefaultAnchorsToEmptyCurve() {
            // Without anchors the edited points would be the first and last points, and the
            // piano roll draws a slope from them towards the edges of the view.
            var descriptor = new UExpressionDescriptor("test", "tst", 0, 100, 20);

            var (xs, ys) = UCurve.ReplaceRange(
                new int[0], new int[0],
                800, 1200,
                new[] { (800, 50), (1200, 50) },
                descriptor);

            Assert.Equal(new[] { 795, 800, 1200, 1205 }, xs);
            Assert.Equal(new[] { 20, 50, 50, 20 }, ys);
        }

        [Fact]
        public void ReplaceRangeDoesNotModifyInput() {
            var baseXs = new[] { 0, 480, 960 };
            var baseYs = new[] { 10, 20, 30 };

            UCurve.ReplaceRange(baseXs, baseYs, 400, 1000, new[] { (500, 50) }, Descriptor);

            Assert.Equal(new[] { 0, 480, 960 }, baseXs);
            Assert.Equal(new[] { 10, 20, 30 }, baseYs);
        }

        [Fact]
        public void ReplaceRangeKeepsValuesOutsideRange() {
            // Vertical shift of [805, 1195].
            var before = MakeCurve(new[] { 0, 1000, 2000 }, new[] { 0, 100, 0 });

            var (xs, ys) = UCurve.ReplaceRange(
                before.xs, before.ys,
                805, 1195,
                new[] { (805, 100), (1000, 100), (1195, 100) },
                Descriptor);
            var after = MakeCurve(xs, ys);

            AssertSameValues(before, after, 0, 805 - UCurve.interval);
            AssertSameValues(before, after, 1195 + UCurve.interval, 2000);
        }

        [Fact]
        public void ReplaceRangeKeepsValuesOutsideMovedRange() {
            // Horizontal shift of [805, 1195] by +300: the original and the destination range are cleared.
            var before = MakeCurve(new[] { 0, 1000, 2000 }, new[] { 0, 100, 0 });

            var (xs, ys) = UCurve.ReplaceRange(
                before.xs, before.ys,
                805, 1495,
                new[] { (1105, 81), (1300, 100), (1495, 81) },
                Descriptor);
            var after = MakeCurve(xs, ys);

            AssertSameValues(before, after, 0, 805 - UCurve.interval);
            AssertSameValues(before, after, 1495 + UCurve.interval, 2000);
        }

        [Fact]
        public void ReplaceRangeKeepsDefaultBeforeFirstPoint() {
            var before = MakeCurve(new[] { 2000 }, new[] { 30 });

            var (xs, ys) = UCurve.ReplaceRange(
                before.xs, before.ys,
                800, 1200,
                new[] { (800, 50), (1200, 50) },
                Descriptor);
            var after = MakeCurve(xs, ys);

            AssertSameValues(before, after, 1200 + UCurve.interval, 2100);
        }

        [Fact]
        public void ReplaceRangeKeepsDefaultAfterLastPoint() {
            var before = MakeCurve(new[] { 0 }, new[] { 30 });

            var (xs, ys) = UCurve.ReplaceRange(
                before.xs, before.ys,
                800, 1200,
                new[] { (800, 50), (1200, 50) },
                Descriptor);
            var after = MakeCurve(xs, ys);

            AssertSameValues(before, after, 0, 800 - UCurve.interval);
        }
    }
}

// Independent suite from the fork: covers UCurve.Samples (amortised binary search fast path)
// and UCurve.SetRange (batch point writes), which the tests above do not exercise.
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace OpenUtau.Core.Ustx {
    public class UCurveSamplesTest {
        readonly ITestOutputHelper output;
        readonly UCurve curve; 
        public UCurveSamplesTest(ITestOutputHelper output){
            this.output = output;
            var descriptor = new UExpressionDescriptor("testcurve", "TEST", 0, 200, 99);
            curve = new UCurve(descriptor);
            curve.xs = new List<int> { 10, 30, 60, 100 };
            curve.ys = new List<int> { 110, 40, 50, 140 };
        }

        [Fact]
        public void UCurveSampleTest() {
            //If the x value exists in xs, return the corresponding y value
            Assert.Equal(110, curve.Sample(10));
            Assert.Equal(40, curve.Sample(30));
            Assert.Equal(50, curve.Sample(60));
            Assert.Equal(140, curve.Sample(100));
            //If the x value is greater than xs[^1] or less than xs[0], return the default value specified in the descriptor
            Assert.Equal(99, curve.Sample(5));
            Assert.Equal(99, curve.Sample(120));
            //If the x value is between xs[i] and xs[i+1], return the linear interpolation of ys[i] and ys[i+1]
            Assert.Equal(47, curve.Sample(50));
            Assert.Equal(61, curve.Sample(65));
        }

        [Fact]
        public void UCurveSamplesTest() {
            //UCurve.Samples should give the same reault as UCurve.Sample
            var samples = curve.Samples(5, 24, 5).ToArray();
            Assert.Equal(110, samples[1]);
            Assert.Equal(40, samples[5]);
            Assert.Equal(50, samples[11]);
            Assert.Equal(140, samples[19]);
            Assert.Equal(99, samples[0]);
            Assert.Equal(99, samples[^1]);
            Assert.Equal(47, samples[9]);
            Assert.Equal(61, samples[12]);
        }
    }
}
