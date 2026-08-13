# Work Order Module

A Work Order is the fulfilment plan for a demand; it is not a production job. Sales Orders describe customer demand, Work Orders decide how that demand will be covered, and Slitting/Lamination Jobs remain shop-floor execution records.

## Sprint S2.1 boundaries

Work Orders may plan coverage from existing inventory, production, or a mixture of both. Planning quantities do not reserve physical coils. Material reservation is deferred to S2.2 and production-job generation to S2.3.

Lamination production supports either Lamination Only, when matching slit material exists, or Slitting and Lamination. It must never infer that slitting is always required.

Active Draft and Released Work Orders count against a Sales Order item's remaining unplanned quantity. Cancelled Work Orders do not. Release requires complete coverage and a valid route.