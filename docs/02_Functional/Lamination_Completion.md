# Lamination Completion

## MVP workflow

Completion is available only for an **Allocated** Lamination Job. The operator records actual consumption once for every active allocation and good/rejected output for every planned step and plate type. Completion runs in one transaction.

For each Slit Coil allocation, CoilManager deducts only `ActualConsumedWeight` from physical weight, releases the entire reservation, records whether the allocation was Consumed, PartiallyConsumed, or Released, and keeps the same Slit Coil Number. Any unused allocated weight immediately returns to availability. A Slit Coil is marked Consumed only when its physical weight reaches zero.

The Lamination Job stores the final good pieces, rejected pieces, consumed weight, scrap weight, completion user/time, and remarks. It is the completed production lot; CoilManager does not generate a Lamination Batch, individual lamination inventory, or individual plate IDs.

Invalid transitions, negative quantities, consumption exceeding allocation, missing/duplicate allocation entries, stale RowVersion values, repeat completion, and cancellation after completion are rejected.