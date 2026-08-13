using System.ComponentModel.DataAnnotations.Schema;
using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class SalesOrder : AuditableEntity
{
    private readonly List<SalesOrderLine> _lines = [];
    private SalesOrder() { }
    public SalesOrder(string number, Customer customer, string customerPoNumber, DateOnly? customerPoDate,
        DateOnly orderDate, DateOnly requiredDeliveryDate, string currency, SalesOrderPriority priority)
    {
        SalesOrderNumber=number; Status=SalesOrderStatus.Draft;
        ApplyHeader(customer, customerPoNumber, customerPoDate, orderDate, requiredDeliveryDate, currency, priority,
            customer.PaymentTerms, customer.BillingAddress, customer.ShippingAddress ?? customer.BillingAddress,
            customer.ContactPerson, customer.Phone, customer.Email, null, null);
    }
    public string SalesOrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public string CustomerCode { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPONumber { get; private set; } = string.Empty;
    public DateOnly? CustomerPODate { get; private set; }
    public DateOnly OrderDate { get; private set; }
    public DateOnly RequiredDeliveryDate { get; private set; }
    public string Currency { get; private set; } = "INR";
    public string? PaymentTerms { get; private set; }
    public string BillingAddress { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ContactEmail { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public SalesOrderPriority Priority { get; private set; }
    public int TotalLines => _lines.Count;
    public decimal? TotalOrderWeight => _lines.Where(x=>x.QuantityUnit==QuantityUnit.Kg).Sum(x=>x.OrderedQuantity);
    public decimal? TotalOrderPieces => _lines.Where(x=>x.QuantityUnit is QuantityUnit.Pieces or QuantityUnit.Sets).Sum(x=>x.OrderedQuantity);
    public decimal? Subtotal => _lines.Any(x=>x.LineAmount.HasValue) ? _lines.Sum(x=>x.LineAmount ?? 0) : null;
    public decimal? TaxAmount { get; private set; }
    public decimal? TotalAmount => Subtotal.HasValue ? Subtotal + (TaxAmount ?? 0) : null;
    public string? CustomerRemarks { get; private set; }
    public string? InternalRemarks { get; private set; }
    public string? ConfirmedBy { get; private set; }
    public DateTimeOffset? ConfirmedOn { get; private set; }
    public string? CancelledBy { get; private set; }
    public DateTimeOffset? CancelledOn { get; private set; }
    public string? CancellationReason { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines;
    [NotMapped] public DateTimeOffset CreatedOn => CreatedAtUtc;
    [NotMapped] public DateTimeOffset? ModifiedOn => UpdatedAtUtc;
    [NotMapped] public string? ModifiedBy => UpdatedBy;
    public void Update(Customer customer,string po,DateOnly? poDate,DateOnly orderDate,DateOnly requiredDate,string currency,
        SalesOrderPriority priority,string? terms,string billing,string shipping,string? contact,string? phone,string? email,
        decimal? tax,string? customerRemarks,string? internalRemarks,IEnumerable<SalesOrderLine> lines)
    {
        if(Status!=SalesOrderStatus.Draft) throw new InvalidOperationException("Only draft Sales Orders can be edited.");
        ApplyHeader(customer,po,poDate,orderDate,requiredDate,currency,priority,terms,billing,shipping,contact,phone,email,customerRemarks,internalRemarks);
        TaxAmount=tax; _lines.Clear(); _lines.AddRange(lines);
    }
    public void Confirm(string actor,DateTimeOffset at)
    {
        if(Status!=SalesOrderStatus.Draft) throw new InvalidOperationException("Only a draft Sales Order can be confirmed.");
        if(!Customer.IsActive) throw new InvalidOperationException("Inactive customers cannot be used for Sales Orders.");
        if(_lines.Count==0) throw new InvalidOperationException("At least one valid line is required before confirmation.");
        Status=SalesOrderStatus.Confirmed; ConfirmedBy=actor; ConfirmedOn=at;
    }
    public void Hold(){ if(Status!=SalesOrderStatus.Confirmed) throw new InvalidOperationException("Only confirmed Sales Orders can be put on hold."); Status=SalesOrderStatus.OnHold; }
    public void ReleaseHold(){ if(Status!=SalesOrderStatus.OnHold) throw new InvalidOperationException("Only on-hold Sales Orders can be released."); Status=SalesOrderStatus.Confirmed; }
    public void Cancel(string reason,string actor,DateTimeOffset at)
    {
        if(Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Confirmed or SalesOrderStatus.OnHold)) throw new InvalidOperationException("This Sales Order cannot be cancelled.");
        if(string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancellation reason is required.");
        Status=SalesOrderStatus.Cancelled; CancellationReason=reason.Trim(); CancelledBy=actor; CancelledOn=at;
    }
    private void ApplyHeader(Customer c,string po,DateOnly? poDate,DateOnly date,DateOnly required,string currency,SalesOrderPriority priority,
        string? terms,string billing,string shipping,string? contact,string? phone,string? email,string? customerRemarks,string? internalRemarks)
    {
        if(string.IsNullOrWhiteSpace(po)) throw new ArgumentException("Customer PO Number is required.");
        Customer=c; CustomerId=c.Id; CustomerCode=c.CustomerCode; CustomerName=c.CustomerName; CustomerPONumber=po.Trim();
        CustomerPODate=poDate; OrderDate=date; RequiredDeliveryDate=required; Currency=string.IsNullOrWhiteSpace(currency)?"INR":currency.Trim().ToUpperInvariant();
        Priority=priority; PaymentTerms=Optional(terms); BillingAddress=billing.Trim(); ShippingAddress=shipping.Trim();
        ContactPerson=Optional(contact); ContactPhone=Optional(phone); ContactEmail=Optional(email);
        CustomerRemarks=Optional(customerRemarks); InternalRemarks=Optional(internalRemarks);
    }
    private static string? Optional(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}

public sealed class SalesOrderLine : BaseEntity
{
    private SalesOrderLine() { }
    public SalesOrderLine(int lineNumber,SalesOrderProductType type,string description,Guid? gradeId,string? gradeCode,
        decimal? thickness,string? category,decimal? coreLoss,decimal? width,decimal? length,string? drawing,string? revision,
        string? oem,string? rating,decimal quantity,QuantityUnit unit,decimal? unitPrice,DateOnly? requiredDate,string? reference,string? remarks)
    {
        if(lineNumber<=0) throw new ArgumentException("Line number must be positive.");
        if(quantity<=0) throw new ArgumentException("Ordered quantity must be greater than zero.");
        LineNumber=lineNumber; ProductType=type; Description=description.Trim(); GradeId=gradeId; GradeCode=gradeCode;
        Thickness=thickness; Category=category; CoreLossPerKg=coreLoss; Width=width; Length=length; DrawingNumber=Optional(drawing);
        DrawingRevision=Optional(revision); OEMJobNumber=Optional(oem); TransformerRating=Optional(rating); OrderedQuantity=quantity;
        QuantityUnit=unit; UnitPrice=unitPrice; LineAmount=unitPrice*quantity; RequiredDeliveryDate=requiredDate;
        CustomerItemReference=Optional(reference); Remarks=Optional(remarks);
    }
    public Guid SalesOrderId { get; private set; }
    public SalesOrder SalesOrder { get; private set; } = null!;
    public int LineNumber { get; private set; }
    public SalesOrderProductType ProductType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? GradeId { get; private set; }
    public Grade? Grade { get; private set; }
    public string? GradeCode { get; private set; }
    public decimal? Thickness { get; private set; }
    public string? Category { get; private set; }
    public decimal? CoreLossPerKg { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Length { get; private set; }
    public string? DrawingNumber { get; private set; }
    public string? DrawingRevision { get; private set; }
    public string? DrawingAttachmentName { get; private set; }
    public string? DrawingAttachmentPath { get; private set; }
    public string? OEMJobNumber { get; private set; }
    public string? TransformerRating { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? LineAmount { get; private set; }
    public DateOnly? RequiredDeliveryDate { get; private set; }
    public string? CustomerItemReference { get; private set; }
    public string? Remarks { get; private set; }
    public decimal FulfilledQuantity { get; private set; }
    public decimal ReadyQuantity { get; private set; }
    public decimal DispatchedQuantity { get; private set; }
    public string? WorkOrderStatus { get; private set; }
    public DateTimeOffset CreatedOn { get; private set; }=DateTimeOffset.UtcNow;
    public DateTimeOffset? ModifiedOn { get; private set; }
    public byte[] RowVersion { get; private set; }=[];
    public void SetDrawingAttachment(string name,string path){DrawingAttachmentName=Optional(name);DrawingAttachmentPath=Optional(path);}
    private static string? Optional(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
