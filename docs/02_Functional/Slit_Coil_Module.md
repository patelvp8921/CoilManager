# Slit Coil Module

Slitting Job completion creates the Slit Coil inventory records. The visible shop-floor identity is the Coil Number (for example `SC-2026-00001-01`); database IDs are used only for navigation.

Inventory supports paging, sorting, material and dimensional filters, and search across Coil Number, Mother Coil Number, Slitting Job Number, Barcode, QR Code, grade, and heat number. Coil Details is a read-only passport with identity, material, source, label payloads, traceability, and inventory history.

Slit Coil Labels support initial printing, reprinting, Batch Print, Print History, and explicit Label Version increments. Printing does not change inventory status. Camera scanning, re-slitting creation, editing, dispatch, and QA remain deferred.
