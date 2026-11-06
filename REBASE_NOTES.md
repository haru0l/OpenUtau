# Fork rebase notes

`master` = `upstream/master` (stakira/OpenUtau @ `f920e242`) + 3 fork commits.

Rebased on 2025-11-06 from fork head `28b745c2`. Pre-rebase state is preserved in the
local branch `backup/pre-rebase-master`.

## Fork commits kept

| Commit | What it does |
| --- | --- |
| `f90805db` set to beta | `SerializablePreferences.Channel = "beta"` (upstream now stores this as a string channel; the old `bool Beta` field is gone, so the intent was re-expressed on the new field). |
| `6027fe5d` Curve performance enhancement | Amortised `Samples(xstart, count, interval)` bulk sampler, wired into `RenderPhrase`. |
| `b1ad36be` bahasa phonemizer | Indonesian/Malay G2P (`BahasaG2p`), DiffSinger Bahasa phonemizer, `g2p-bahasa.zip` resource, lyrics-helper + phonetic-assistant registration. |

## Fork commits dropped as redundant (10)

Upstream independently merged the same DiffSinger work, so these applied as empty:

- `8353110c` `DiffSingerConfig.cs`, `0d9fad42` + `e995d08b` `DiffSingerRenderer.cs`,
  `2baac9eb` + `e471e59c` `DiffSingerVariance.cs`, `24a2e356`, `417a0fda`
  — all the tension/energy/breathiness embed work (`useTensionEmbed`,
  `predict_tension`, `TENC`) is present in upstream `master`.
- `d3453354`, `9eabd5fe`, `94efb2fc` `dotnet-desktop.yml` — GitHub's stock WPF/MSIX
  template (.NET 6, WPF packaging project, `actions/*@v3`). It targets a WAP packaging
  project this repo does not have and would fail on every run. Upstream's real CI
  (`build.yml`, `pr-test.yml`, `worldline-build.yml`) is used instead.

## Adaptation made while rebasing `6027fe5d`

Upstream refactored `RenderPhrase` to sample `Pipeline.CurveSource` (an immutable
snapshot) rather than `UCurve`. The fork's `UCurve.Samples` was therefore kept (it is
the tested public API) **and** ported onto `CurveSource.Samples`
(`OpenUtau.Core/Pipeline/PhraseSource.cs`) so the call sites in `RenderPhrase.cs` keep
the amortised search. `SetRangeCurveCommand` was reparented from the removed
`CurveCommand` onto `ExpCommand` and given its own `Unexecute`/merge logic.

## Deliberate behaviour change (please confirm)

The fork's `BahasaG2pLyricsHelper` was written as `: base(new ArpabetG2p())` — an
English-ARPAbet G2P wrapped in a helper whose whole purpose is Bahasa (Indonesian/Malay)
G2P, and `BahasaG2p.cs` was added in the same commit. That reads as a copy-paste slip from
the `ArpabetG2pLyricsHelper` above it, so the rebase wires it to `base(new BahasaG2p())`.

This is the one semantic (not merely mechanical) change in the rebase. It compiles and the
suite passes either way, so if the fork really did intend the ARPAbet link, change line
143 of `OpenUtau.Core/Util/LyricsHelper.cs` back.

## Verification

- `dotnet build` (whole solution): 0 errors.
- `dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj`: 720 passed, 0 failed,
  3 skipped (live-voicebank tests needing external resources).
