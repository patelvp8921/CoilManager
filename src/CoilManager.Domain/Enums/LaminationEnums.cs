namespace CoilManager.Domain.Enums;

public enum LaminationJobStatus { Draft, Allocated, Released, InProgress, Completed, Cancelled }
public enum LaminationDesignType { Simple, StepLap }
public enum StepLapOrientation { NotApplicable, Horizontal, Vertical, HorizontalAndVertical }
public enum LaminationPlateType { Side, Center, Top, Bottom }
