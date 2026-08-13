using CoilManager.Domain.Enums;

namespace CoilManager.Application.DTOs.Sales;

public sealed record CustomerDto(Guid Id,string CustomerCode,string CustomerName,string? ShortName,string BillingAddress,string? ShippingAddress,
 string City,string State,string Country,string PostalCode,string ContactPerson,string Phone,string Email,string? GSTNumber,string? PAN,
 string? PaymentTerms,int? CreditDays,bool IsActive,string? Remarks,string? CreatedBy,DateTimeOffset CreatedOn,string? ModifiedBy,DateTimeOffset? ModifiedOn,string RowVersion);
public sealed record SaveCustomerRequest(string CustomerName,string? ShortName,string BillingAddress,string? ShippingAddress,string City,string State,
 string Country,string PostalCode,string ContactPerson,string Phone,string Email,string? GSTNumber,string? PAN,string? PaymentTerms,int? CreditDays,
 bool IsActive,string? Remarks,string? RowVersion);
public sealed record SalesOrderLineRequest(int LineNumber,SalesOrderProductType ProductType,string Description,Guid? GradeId,decimal? Width,decimal? Length,
 string? DrawingNumber,string? DrawingRevision,string? OEMJobNumber,string? TransformerRating,decimal OrderedQuantity,QuantityUnit QuantityUnit,
 decimal? UnitPrice,DateOnly? RequiredDeliveryDate,string? CustomerItemReference,string? Remarks);
public sealed record SaveSalesOrderRequest(Guid CustomerId,string CustomerPONumber,DateOnly? CustomerPODate,DateOnly OrderDate,DateOnly RequiredDeliveryDate,
 string Currency,string? PaymentTerms,string BillingAddress,string ShippingAddress,string? ContactPerson,string? ContactPhone,string? ContactEmail,
 SalesOrderPriority Priority,decimal? TaxAmount,string? CustomerRemarks,string? InternalRemarks,IReadOnlyList<SalesOrderLineRequest> Lines,string? RowVersion);
public sealed record SalesOrderLineDto(Guid Id,int LineNumber,SalesOrderProductType ProductType,string Description,Guid? GradeId,string? GradeCode,
 decimal? Thickness,string? Category,decimal? CoreLossPerKg,decimal? Width,decimal? Length,string? DrawingNumber,string? DrawingRevision,string? OEMJobNumber,
 string? TransformerRating,decimal OrderedQuantity,QuantityUnit QuantityUnit,decimal? UnitPrice,decimal? LineAmount,DateOnly? RequiredDeliveryDate,
 string? CustomerItemReference,string? Remarks,string? DrawingAttachmentName,decimal FulfilledQuantity,decimal ReadyQuantity,decimal DispatchedQuantity,string? WorkOrderStatus);
public sealed record SalesOrderDto(Guid Id,string SalesOrderNumber,Guid CustomerId,string CustomerCode,string CustomerName,string CustomerPONumber,
 DateOnly? CustomerPODate,DateOnly OrderDate,DateOnly RequiredDeliveryDate,string Currency,string? PaymentTerms,string BillingAddress,string ShippingAddress,
 string? ContactPerson,string? ContactPhone,string? ContactEmail,SalesOrderStatus Status,SalesOrderPriority Priority,int TotalLines,decimal TotalWeightKg,
 decimal TotalPieces,decimal TotalSets,decimal? Subtotal,decimal? TaxAmount,decimal? TotalAmount,string? CustomerRemarks,string? InternalRemarks,
 string? ConfirmedBy,DateTimeOffset? ConfirmedOn,string? CancelledBy,DateTimeOffset? CancelledOn,string? CancellationReason,string? CreatedBy,
 DateTimeOffset CreatedOn,string? ModifiedBy,DateTimeOffset? ModifiedOn,string RowVersion,IReadOnlyList<SalesOrderLineDto> Lines);
public sealed record CancelSalesOrderRequest(string Reason,string? RowVersion);
