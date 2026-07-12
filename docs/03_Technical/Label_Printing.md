# Label Printing

`LabelSettings` configures dimensions, company identity, copies, version, barcode/QR visibility, orientation, error correction, and future DPI. The backend returns label data and records tracking plus one `SlitCoilLabelPrintHistory` row per coil per print event.

Single and batch pages reuse one preview, Code 128 component, and QR component. Print CSS uses `@page { size: 100mm 75mm; margin: 0; }`, hides application chrome, prevents overflow, and emits one label per page.

Browser printing is the MVP transport. Direct Zebra/TSC/Godex integration, ZPL, and printer discovery are deferred.
