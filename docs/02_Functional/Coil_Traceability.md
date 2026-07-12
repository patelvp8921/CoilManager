# Coil Traceability

Each Slit Coil records its immediate parent, root Mother Coil, source Mother Coil, and generating Slitting Job. First-generation Slit Coils have the Mother Coil as parent and root. Deeper generations retain the same root and point to their immediate parent Slit Coil.

The traceability API returns the root, parent chain, children ordered by slit sequence, descendants, related jobs, and inventory transactions. Circular or malformed genealogy produces a safe validation response.
