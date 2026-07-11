namespace CoilManager.Domain.Enums;

public enum CoilStatus
{
    Draft = 0,
    Available = 1,
    Reserved = 2,
    OnHold = 2,
    Rejected = 3,
    Scrapped = 3,
    Consumed = 4,
    Dispatched = 5,
    UnderInspection = 6,
    InProcess = 7
}
