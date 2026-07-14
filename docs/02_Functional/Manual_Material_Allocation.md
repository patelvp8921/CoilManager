# Manual Material Allocation

The allocation screen exposes current, reserved, and available weights separately. Available weight is current physical weight minus active Work Order reservations. Reserved, Issued, and Partially Consumed allocations are active; Consumed and Released allocations are not.

Rules:

- allocated weight must be greater than zero and no more than available weight;
- partial allocation and multiple coils per Work Order are supported;
- reservations are summed across all Work Orders to prevent reuse of the same available weight;
- releasing an allocation or cancelling its Work Order restores availability;
- partial reservations do not change the whole coil's physical status or number;
- only coils with positive available weight appear in lookup results.

Automatic allocation is explicitly deferred.
