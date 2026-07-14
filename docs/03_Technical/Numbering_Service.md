# Numbering Service

Work Order numbers use `WO-{UTC year}-{five-digit sequence}`. Example: `WO-2026-00001`.

`GET /api/work-orders/next-number` provides a display preview. On create, the service reads the maximum sequence for the current year, advances past any existing number, and persists against a unique database index. The preview is never accepted from the client and is not authoritative.

The numbering scheme resets its sequence each year. A future high-volume implementation may replace maximum-sequence lookup with a transactional number-series row while retaining the public format.
