using System.ComponentModel.DataAnnotations.Schema;
using CoilManager.Domain.Common;

namespace CoilManager.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    private Customer() { }
    public Customer(string code, string name, string billingAddress, string city, string state, string country,
        string postalCode, string contactPerson, string phone, string email, bool isActive = true)
    {
        CustomerCode = Required(code); CustomerName = Required(name); BillingAddress = Required(billingAddress);
        City = Required(city); State = Required(state); Country = Required(country); PostalCode = Required(postalCode);
        ContactPerson = Required(contactPerson); Phone = Required(phone); Email = Required(email); IsActive = isActive;
    }
    public string CustomerCode { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string? ShortName { get; private set; }
    public string BillingAddress { get; private set; } = string.Empty;
    public string? ShippingAddress { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? GSTNumber { get; private set; }
    public string? PAN { get; private set; }
    public string? PaymentTerms { get; private set; }
    public int? CreditDays { get; private set; }
    public bool IsActive { get; private set; }
    public string? Remarks { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    [NotMapped] public DateTimeOffset CreatedOn => CreatedAtUtc;
    [NotMapped] public DateTimeOffset? ModifiedOn => UpdatedAtUtc;
    [NotMapped] public string? ModifiedBy => UpdatedBy;
    public void Update(string name, string? shortName, string billingAddress, string? shippingAddress, string city,
        string state, string country, string postalCode, string contactPerson, string phone, string email,
        string? gst, string? pan, string? paymentTerms, int? creditDays, string? remarks)
    {
        CustomerName=Required(name); ShortName=Optional(shortName); BillingAddress=Required(billingAddress);
        ShippingAddress=Optional(shippingAddress); City=Required(city); State=Required(state); Country=Required(country);
        PostalCode=Required(postalCode); ContactPerson=Required(contactPerson); Phone=Required(phone); Email=Required(email);
        GSTNumber=Optional(gst); PAN=Optional(pan); PaymentTerms=Optional(paymentTerms); CreditDays=creditDays; Remarks=Optional(remarks);
    }
    public void SetActive(bool active) => IsActive = active;
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value is missing.") : value.Trim();
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
