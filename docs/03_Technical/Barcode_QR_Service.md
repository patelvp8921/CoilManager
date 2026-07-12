# Barcode and QR Service

The canonical payload for both Code 128 Barcode and QR Code is the Coil Number only. Search trims input and performs an exact, case-insensitive match against Mother Coil Number, Slit Coil Number, BarcodeValue, and QrCodeValue. URL-encoded values are accepted.

Keyboard-style barcode scanners are supported. The ten most recent successful searches are kept locally. Slit Coil Labels render Code 128 using JsBarcode and QR Code with a scanner-safe quiet zone; both encode only Coil Number. Camera scanning is deferred.
