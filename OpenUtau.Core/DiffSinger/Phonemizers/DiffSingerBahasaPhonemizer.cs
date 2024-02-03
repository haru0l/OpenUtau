using OpenUtau.Api;
using OpenUtau.Core.G2p;

namespace OpenUtau.Core.DiffSinger {
    [Phonemizer("DiffSinger Bahasa Phonemizer", "DIFFS BAHASA", language: "MY-ID")]
    public class DiffSingerBahasaPhonemizer : DiffSingerG2pPhonemizer {
        protected override string GetDictionaryName() => "dsdict-my.yaml";
        protected override IG2p LoadBaseG2p() => new BahasaG2p();
        protected override string[] GetBaseG2pVowels() => new string[] {
            "a", "e", "eu", "i", "o", "u"
        };

        protected override string[] GetBaseG2pConsonants() => new string[] {
            "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n",
            "ng", "ny", "p", "r", "s", "sy", "q", "t", "v", "w", "y", "z"
        };
    }
}
