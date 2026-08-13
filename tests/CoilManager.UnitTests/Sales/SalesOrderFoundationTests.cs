using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;

namespace CoilManager.UnitTests.Sales;

public sealed class SalesOrderFoundationTests
{
    private static Customer ActiveCustomer() => new("CUS-00001","Vishal Transformers","Pune address","Pune","Maharashtra","India","411001","Vishal Patel","9999999999","sales@example.com");
    private static SalesOrder Order(Customer? customer=null) => new("SO/2026/00001",customer??ActiveCustomer(),"PO-100",DateOnly.FromDateTime(DateTime.Today),DateOnly.FromDateTime(DateTime.Today),DateOnly.FromDateTime(DateTime.Today.AddDays(14)),"INR",SalesOrderPriority.Normal);
    private static SalesOrderLine CoilLine(decimal quantity=100) => new(1,SalesOrderProductType.MotherCoil,"Mother coil",Guid.NewGuid(),"23HP85D",.23m,"M3",.85m,1000,null,null,null,null,null,quantity,QuantityUnit.Kg,100,null,null,null);
    private static void AddLines(SalesOrder order,Customer customer,params SalesOrderLine[] lines)=>order.Update(customer,"PO-100",null,DateOnly.FromDateTime(DateTime.Today),DateOnly.FromDateTime(DateTime.Today.AddDays(14)),"INR",SalesOrderPriority.Normal,null,"Billing","Shipping",null,null,null,0,null,null,lines);

    [Fact] public void Customer_code_matches_required_format()=>Assert.Matches("^CUS-[0-9]{5}$",ActiveCustomer().CustomerCode);
    [Fact] public void Sales_order_number_matches_yearly_format()=>Assert.Matches("^SO/2026/[0-9]{5}$",Order().SalesOrderNumber);
    [Fact] public void Draft_sales_order_can_be_created()=>Assert.Equal(SalesOrderStatus.Draft,Order().Status);
    [Fact] public void Quantity_must_be_positive()=>Assert.Throws<ArgumentException>(()=>CoilLine(0));
    [Fact] public void Lamination_supports_pieces_and_sets()
    {
        var pieces=new SalesOrderLine(1,SalesOrderProductType.Lamination,"Lamination",Guid.NewGuid(),"23HP85D",.23m,"M3",.85m,null,null,"DR-1",null,null,"100 kVA",10,QuantityUnit.Pieces,null,null,null,null);
        var sets=new SalesOrderLine(2,SalesOrderProductType.Lamination,"Lamination",Guid.NewGuid(),"23HP85D",.23m,"M3",.85m,null,null,null,null,"OEM-1","100 kVA",2,QuantityUnit.Sets,null,null,null,null);
        Assert.Equal(QuantityUnit.Pieces,pieces.QuantityUnit);Assert.Equal(QuantityUnit.Sets,sets.QuantityUnit);
    }
    [Fact] public void Confirmation_requires_at_least_one_line()=>Assert.Throws<InvalidOperationException>(()=>Order().Confirm("user",DateTimeOffset.UtcNow));
    [Fact] public void Confirm_changes_status_without_creating_fulfilment()
    {
        var customer=ActiveCustomer();var order=Order(customer);AddLines(order,customer,CoilLine());order.Confirm("user",DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Confirmed,order.Status);Assert.All(order.Lines,x=>{Assert.Equal(0,x.FulfilledQuantity);Assert.Null(x.WorkOrderStatus);});
    }
    [Fact] public void Confirmed_order_is_not_editable()
    {
        var c=ActiveCustomer();var o=Order(c);AddLines(o,c,CoilLine());o.Confirm("user",DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(()=>AddLines(o,c,CoilLine()));
    }
    [Fact] public void Cancellation_requires_reason()
    {
        var o=Order();Assert.Throws<ArgumentException>(()=>o.Cancel("", "user",DateTimeOffset.UtcNow));
    }
    [Fact] public void Cancelled_order_becomes_read_only()
    {
        var c=ActiveCustomer();var o=Order(c);AddLines(o,c,CoilLine());o.Cancel("Customer request","user",DateTimeOffset.UtcNow);
        Assert.Equal(SalesOrderStatus.Cancelled,o.Status);Assert.Throws<InvalidOperationException>(()=>AddLines(o,c,CoilLine()));
    }
    [Fact] public void Totals_do_not_combine_kg_pieces_and_sets()
    {
        var c=ActiveCustomer();var o=Order(c);var pieces=new SalesOrderLine(2,SalesOrderProductType.Lamination,"L",Guid.NewGuid(),"G",.23m,"M3",.85m,null,null,"D",null,null,"100",20,QuantityUnit.Pieces,null,null,null,null);var sets=new SalesOrderLine(3,SalesOrderProductType.Lamination,"L",Guid.NewGuid(),"G",.23m,"M3",.85m,null,null,"D",null,null,"100",3,QuantityUnit.Sets,null,null,null,null);AddLines(o,c,CoilLine(500),pieces,sets);
        Assert.Equal(500,o.TotalOrderWeight);Assert.Equal(23,o.TotalOrderPieces);
    }
    [Fact] public void Status_transitions_follow_sprint_rules()
    {
        var c=ActiveCustomer();var o=Order(c);AddLines(o,c,CoilLine());o.Confirm("user",DateTimeOffset.UtcNow);o.Hold();Assert.Equal(SalesOrderStatus.OnHold,o.Status);o.ReleaseHold();Assert.Equal(SalesOrderStatus.Confirmed,o.Status);
    }
}
