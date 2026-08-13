namespace CoilManager.Domain.Enums;

public enum SalesOrderStatus { Draft, Confirmed, InPlanning, InProduction, PartiallyReady, ReadyForDispatch, PartiallyDispatched, Completed, OnHold, Cancelled }
public enum SalesOrderPriority { Low, Normal, High, Urgent }
public enum SalesOrderProductType { MotherCoil, SlitCoil, Lamination, CoreFrameAssembly }
public enum QuantityUnit { Kg, Pieces, Sets }
