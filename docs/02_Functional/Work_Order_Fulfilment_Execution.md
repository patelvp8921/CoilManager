# Work Order Fulfilment Execution

A Released Work Order has an approved fulfilment plan. Execution begins when inventory is reserved or a production job is linked. Work Order status is derived from physical execution: Released, In Fulfilment, Partially Ready, or Ready.

The existing-inventory portion is reserved directly by the Work Order. Reservations are transactional, specification-matched, limited by physical availability and the planned inventory quantity, and reduce the inventory record's available weight.

Lamination input remains owned by Lamination Job material allocation. A Work Order must never create a duplicate Slit Coil reservation for that input. Planned or merely released production does not count as Ready; only valid reserved fulfilment inventory or completed production output may contribute.