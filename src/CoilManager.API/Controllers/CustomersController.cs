using CoilManager.Application.DTOs.Sales;
using CoilManager.Domain.Entities;
using CoilManager.Persistence;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoilManager.API.Controllers;

[Route("api/customers"),Authorize]
public sealed class CustomersController(ApplicationDbContext db):BaseApiController
{
 [HttpGet] public async Task<ActionResult<ApiPagedResponse<CustomerDto>>> List(string? search,bool? isActive,string? city,string? state,string? country,int page=1,int pageSize=25,CancellationToken ct=default)
 {var q=db.Customers.AsNoTracking();if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.CustomerCode.Contains(search)||x.CustomerName.Contains(search));if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);if(!string.IsNullOrWhiteSpace(city))q=q.Where(x=>x.City==city);if(!string.IsNullOrWhiteSpace(state))q=q.Where(x=>x.State==state);if(!string.IsNullOrWhiteSpace(country))q=q.Where(x=>x.Country==country);int total=await q.CountAsync(ct);page=Math.Max(1,page);pageSize=Math.Clamp(pageSize,1,100);var data=(await q.OrderBy(x=>x.CustomerCode).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct)).Select(Map).ToList();return Paged(data,new(page,pageSize,total));}
 [HttpGet("{id:guid}")] public async Task<ActionResult<ApiResponse<CustomerDto>>> Get(Guid id,CancellationToken ct){var x=await db.Customers.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);return x is null?Failure<CustomerDto>(404,"Customer was not found."):Success(Map(x));}
 [HttpGet("next-code")] public async Task<ActionResult<ApiResponse<string>>> Next(CancellationToken ct)=>Success(await NextCode(ct));
 [HttpPost,Authorize(Policy="Permission:Customers.Create")] public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(SaveCustomerRequest r,CancellationToken ct)
 {var error=Validate(r);if(error!=null)return Failure<CustomerDto>(400,error,[error]);var x=new Customer(await NextCode(ct),r.CustomerName,r.BillingAddress,r.City,r.State,r.Country,r.PostalCode,r.ContactPerson,r.Phone,r.Email,r.IsActive);Apply(x,r);db.Customers.Add(x);await db.SaveChangesAsync(ct);return CreatedAtAction(nameof(Get),new{id=x.Id},ApiResponse<CustomerDto>.Ok(Map(x),"Customer created."));}
 [HttpPut("{id:guid}"),Authorize(Policy="Permission:Customers.Edit")] public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(Guid id,SaveCustomerRequest r,CancellationToken ct)
 {var x=await db.Customers.FirstOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return Failure<CustomerDto>(404,"Customer was not found.");SetVersion(x,r.RowVersion);var error=Validate(r);if(error!=null)return Failure<CustomerDto>(400,error,[error]);Apply(x,r);try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){return Failure<CustomerDto>(409,"Customer was changed by another user.");}return Success(Map(x),"Customer updated.");}
 [HttpPost("{id:guid}/activate"),Authorize(Policy="Permission:Customers.Activate")] public Task<ActionResult<ApiResponse<CustomerDto>>> Activate(Guid id,CancellationToken ct)=>Status(id,true,ct);
 [HttpPost("{id:guid}/deactivate"),Authorize(Policy="Permission:Customers.Activate")] public Task<ActionResult<ApiResponse<CustomerDto>>> Deactivate(Guid id,CancellationToken ct)=>Status(id,false,ct);
 private async Task<ActionResult<ApiResponse<CustomerDto>>> Status(Guid id,bool active,CancellationToken ct){var x=await db.Customers.FirstOrDefaultAsync(x=>x.Id==id,ct);if(x is null)return Failure<CustomerDto>(404,"Customer was not found.");x.SetActive(active);await db.SaveChangesAsync(ct);return Success(Map(x),active?"Customer activated.":"Customer deactivated.");}
 private async Task<string> NextCode(CancellationToken ct){var last=await db.Customers.OrderByDescending(x=>x.CustomerCode).Select(x=>x.CustomerCode).FirstOrDefaultAsync(ct);int n=int.TryParse(last?.Split('-').Last(),out var value)?value+1:1;return $"CUS-{n:00000}";}
 private static string? Validate(SaveCustomerRequest r){if(string.IsNullOrWhiteSpace(r.CustomerName))return "Customer name is required.";if(string.IsNullOrWhiteSpace(r.BillingAddress)||string.IsNullOrWhiteSpace(r.City)||string.IsNullOrWhiteSpace(r.State)||string.IsNullOrWhiteSpace(r.Country))return "Billing address, city, state and country are required.";if(string.IsNullOrWhiteSpace(r.ContactPerson)||string.IsNullOrWhiteSpace(r.Phone)||string.IsNullOrWhiteSpace(r.Email))return "Contact person, phone and email are required.";if(r.CreditDays<0)return "Credit days cannot be negative.";return null;}
 private static void Apply(Customer x,SaveCustomerRequest r)=>x.Update(r.CustomerName,r.ShortName,r.BillingAddress,r.ShippingAddress,r.City,r.State,r.Country,r.PostalCode,r.ContactPerson,r.Phone,r.Email,r.GSTNumber,r.PAN,r.PaymentTerms,r.CreditDays,r.Remarks);
 private void SetVersion(Customer x,string? v){if(!string.IsNullOrWhiteSpace(v))db.Entry(x).Property(y=>y.RowVersion).OriginalValue=Convert.FromBase64String(v);}
 private static CustomerDto Map(Customer x)=>new(x.Id,x.CustomerCode,x.CustomerName,x.ShortName,x.BillingAddress,x.ShippingAddress,x.City,x.State,x.Country,x.PostalCode,x.ContactPerson,x.Phone,x.Email,x.GSTNumber,x.PAN,x.PaymentTerms,x.CreditDays,x.IsActive,x.Remarks,x.CreatedBy,x.CreatedOn,x.ModifiedBy,x.ModifiedOn,Convert.ToBase64String(x.RowVersion));
}
