# Sales Order Workflow

    Customer demand
          |
          v
    Draft Sales Order --cancel(reason)--> Cancelled
          |
       confirm
          v
    Confirmed <----release hold---- On Hold
          |                            ^
          +--------- put on hold ------+
          |
          +-- cancel(reason) ----------> Cancelled

Confirmation validates an active customer, Customer PO number, delivery date, at least one valid uniquely numbered line, positive quantities, product-specific requirements, authoritative Grade Master values, and optimistic concurrency.

Confirmation records the actor and timestamp only. It has no inventory, allocation, Work Order, production-job, or dispatch side effects. Sprint S2 will use confirmed demand to plan fulfilment through Work Orders.
