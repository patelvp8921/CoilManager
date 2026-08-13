# Customer Master

The Customer Master is the commercial party source for Sales Orders. Codes are generated as CUS-00001, are unique, and cannot be edited after creation.

Each customer stores billing and optional shipping addresses, location, primary contact details, GST/PAN identifiers, payment terms, credit days, status, remarks, audit fields, and a SQL row version. Active status is independently managed. Inactive customers remain visible historically but cannot be selected for a new Sales Order.

The list supports search, active status, city, state, and country filters. APIs are under /api/customers and use the standard response envelopes.
