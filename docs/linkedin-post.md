I spent one session building a tool that audits data quality — the thing every "AI data vendor" sells you and no one lets you verify.

Result: QualiBench. 1.4 MB native binary. Audits 100,000 labeled examples in 26 milliseconds. Runs in your CI.

The interesting part isn't the speed. It's what happened when I validated it on real data.

I took CytoCrowd, a published cervical cytology dataset annotated by four board-certified pathologists with a gold standard. Ran the audit.

Krippendorff's alpha between the four experts: 0.2474.

That's not a typo. Four certified pathologists, five-class problem, and inter-rater agreement is roughly what you'd get from random noise on half the classes. One of the four had 40% agreement with consensus while the other three were 89-95%.

QualiBench found that outlier annotator in 0.2 milliseconds.

Here's the uncomfortable question this raises: if four certified pathologists can't agree with each other, what exactly are you buying when you pay a labeling vendor for "expert annotation"? And more importantly — how would you ever know?

The pitch is simple: the incumbent sells you a promise on a slide. QualiBench lets you check it yourself, on your own data, on your own laptop.

Second dataset, WhichDog (MTurk crowdsourcing, 1028 annotators, 400 items): F1 = 0.855 with Confident Learning. That's a different regime — dense annotator pool, high noise, model-based detector wins by +26 F1 points over naive consensus.

So the tool ships two detectors. You pick per workload:
- K >= 50 annotators -> Confident Learning (learns per-annotator error matrix)
- K <= 5 experts -> Consensus (majority vote, robust on small K)

I'm not claiming this replaces Scale AI or Surge. I'm claiming the audit layer should be independent of the vendor, open-source, and run in 26 ms.

MIT license. Code and reproducible validation: github.com/SRKRZ23/qualibench

#DataQuality #MachineLearning #ConfidentLearning #DataAnnotation #MLOps
