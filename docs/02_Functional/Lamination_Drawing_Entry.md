# Lamination Drawing Entry

OEM drawings vary in layout and nomenclature, so MVP deliberately has no Drawing Master, OCR, or automatic import. The planner enters Drawing Number and/or OEM Job Number and may attach one PDF, PNG, JPG or JPEG source drawing (maximum 15 MB).

Each plate supports an ordered set of flexible dimension codes such as L1, X, B, W or any OEM-specific code without schema changes. Duplicate codes on one plate are rejected. The stored document reference is internal; API responses never expose a local filesystem path.
