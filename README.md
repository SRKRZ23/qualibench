# QualiBench

**Independent label-quality auditing — runs on your laptop, not someone else's cloud.**

QualiBench is a 1.6 MB native binary that audits annotated datasets in milliseconds.
It tells you *which labels to trust, which annotators to retrain, and which samples to
re-label* — with a full precision/recall curve instead of a vendor SLA.

No cloud. No per-seat pricing. No sending your data to a labeling vendor.
One binary. Reads CSV. Runs in 3 ms. Audits 100,000 labels in 26 ms.

---

## Why

Every enterprise AI team buys "data quality" from a vendor — Scale AI, Surge, Appen —
and gets back a number on a slide. There is no way to *verify* that number without a
second vendor. QualiBench is the second opinion: a self-contained auditor you run
on your own machine, on your own data, in your own CI, before you ship a model
trained on labels you don't actually trust.

> **Scale AI sells you a promise. QualiBench lets you check it.**

---

## Key numbers

M3 Pro, .NET 10, synthetic 100k×10 dataset, 10% injected noise.

| Detector | Precision | Recall | F1 | Wall clock |
|---|---|---|---|---|
| Consensus (agree≥0.50, ≥2 annotators) | 99.98% | 94.46% | 0.971 | 26 ms |
| **Confident Learning** (default) | **99.86%** | **99.80%** | **0.998** | 26 ms |
| Full sweep (32 operating points) | — | — | — | 174 ms |

| Runtime | Startup | `evaluate` | `sweep` | Binary |
|---|---|---|---|---|
| JIT | 19.7 ms / 36 MiB | 129 ms / 72 MiB | 191 ms / 79 MiB | — |
| **NativeAOT** | **3.0 ms / 4.5 MiB** | 77 ms / 35 MiB | 176 ms / 40 MiB | **1.6 MB** |

Full reproduction: [`docs/AOT-BENCH.md`](docs/AOT-BENCH.md).

---

## CI gate

`cl` and `evaluate` accept `--fail-on-noise <F1>`. Exit 1 when F1 drops below.

Example GitHub Actions step:

```yaml
- run: ./qualibench cl labels.csv truth.csv --fail-on-noise 0.85
```

The step fails the pipeline when label quality regresses below 0.85 F1.
Drop the binary into your repo (`publish-aot/QualiBench.Cli`) and call it directly —
no setup step, no runtime dependency, starts in 3 ms.
