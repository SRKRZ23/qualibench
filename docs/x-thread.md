1/ I built a 1.4 MB native binary that audits label quality in 26 ms. It found an outlier annotator in a dataset from 4 certified pathologists in 0.2 ms. Repo below.

2/ The problem: every enterprise AI team buys "data quality" from a labeling vendor. Nobody can verify the number. Scale AI sells you a promise on a slide.

3/ QualiBench reads a CSV (items x annotators). Ships two detectors:
- Consensus: majority vote, robust on K<=5 experts
- Confident Learning: per-annotator confusion matrix, wins on K>=50

You pick per workload.

4/ Validated on two public real datasets:

WhichDog (MTurk, 400x1028): F1 = 0.855 (CL)
CytoCrowd (4 pathologists + gold, 446x4): F1 = 0.817 (Consensus), alpha = 0.2474

5/ That alpha = 0.2474 is the punchline. Four board-certified pathologists. Five-class problem. One of them at 40% agreement with consensus, three at 89-95%.

QualiBench found him in 0.2 ms.

6/ The pitch isn't "we beat Scale AI." It's "the audit layer should be independent of the vendor, open-source, and run in 26 ms."

CI gate: qualibench cl data.csv truth.csv --fail-on-noise 0.85 -> exit 1 if F1 drops.

7/ NativeAOT. 1.4 MB. 3 ms startup. Depends only on macOS system libs. No runtime, no cloud, no per-seat pricing.

otool -L shows /usr/lib/* only.

8/ MIT license. Code + reproducible benchmark:
github.com/SRKRZ23/qualibench

Feedback welcome. Especially from anyone who's bought annotation services and wished they could audit the output.

#buildinpublic #DataQuality #MLOps
