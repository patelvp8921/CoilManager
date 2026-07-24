using CoilManager.Domain.Common;
using CoilManager.Domain.Enums;

namespace CoilManager.Domain.Entities;

public sealed class LaminationJob : SoftDeletableEntity
{
    public static string FormatNumber(int year, int sequence) => $"AE/C/{year}/{sequence:00000}";
    private LaminationJob() { }
    public LaminationJob(string number, string? jobOrDrawingNumber, string? customer, string rating,
        LaminationDesignType designType, StepLapOrientation orientation, int numberOfSteps, Guid gradeId,
        decimal thickness, string category, decimal coreLossPerKg, decimal totalWeight, decimal customerCoreLossPerKg,
        Guid? workOrderId, string? workOrderNumber, DateOnly plannedDate, DateOnly? requiredDate, string? shift,
        string? plannerName, string? remarks)
    {
        LaminationJobNumber = number; JobOrDrawingNumber = Clean(jobOrDrawingNumber); Customer = Clean(customer);
        Rating = Required(rating, "Rating");
        GradeId = gradeId; Thickness = thickness; Category = category; CoreLossPerKg = coreLossPerKg;
        SetCustomerLoss(totalWeight, customerCoreLossPerKg);
        WorkOrderId = workOrderId; WorkOrderNumber = Clean(workOrderNumber); PlannedDate = plannedDate; RequiredDate = requiredDate;
        Shift = Clean(shift); PlannerName = Clean(plannerName); Remarks = Clean(remarks);
        SetDesign(designType, orientation, numberOfSteps); Status = LaminationJobStatus.Draft;
    }
    public string LaminationJobNumber { get; private set; } = "";
    public string? JobOrDrawingNumber { get; private set; }
    public string? Customer { get; private set; }
    public string Rating { get; private set; } = "";
    public LaminationDesignType DesignType { get; private set; }
    public StepLapOrientation StepLapOrientation { get; private set; }
    public int NumberOfSteps { get; private set; }
    public Guid GradeId { get; private set; }
    public decimal Thickness { get; private set; }
    public string Category { get; private set; } = "";
    public decimal CoreLossPerKg { get; private set; }
    public decimal TotalWeight { get; private set; }
    public decimal CustomerCoreLossPerKg { get; private set; }
    public decimal NoLoadLossWatts { get; private set; }
    public Guid? WorkOrderId { get; private set; }
    public string? WorkOrderNumber { get; private set; }
    public DateOnly PlannedDate { get; private set; }
    public DateOnly? RequiredDate { get; private set; }
    public string Machine => "CTL-450-GLOBALSPS";
    public string? Shift { get; private set; }
    public string? PlannerName { get; private set; }
    public LaminationJobStatus Status { get; private set; }
    public int TotalPlannedPieces { get; private set; }
    public decimal TotalPlannedWeight { get; private set; }
    public decimal TotalAllocatedWeight { get; private set; }
    public string? Remarks { get; private set; }
    public string? DrawingAttachmentName { get; private set; }
    public string? DrawingAttachmentPath { get; private set; }
    public string? ReleasedBy { get; private set; } public DateTimeOffset? ReleasedOn { get; private set; }
    public string? AllocatedBy { get; private set; } public DateTimeOffset? AllocatedOn { get; private set; }
    public string? CancelledBy { get; private set; } public DateTimeOffset? CancelledOn { get; private set; }
    public int TotalGoodPieces { get; private set; } public int TotalRejectedPieces { get; private set; }
    public decimal TotalConsumedWeight { get; private set; } public decimal TotalScrapWeight { get; private set; } public decimal TotalJobWeight { get; private set; }
    public decimal TopPlateWeight { get; private set; } public decimal BottomPlateWeight { get; private set; } public decimal LeftSidePlateWeight { get; private set; } public decimal RightSidePlateWeight { get; private set; } public decimal CenterPlateWeight { get; private set; }
    public string? CompletedBy { get; private set; } public DateTimeOffset? CompletedOn { get; private set; } public string? CompletionRemarks { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public Grade? Grade { get; private set; }
    public ICollection<LaminationJobStep> Steps { get; private set; } = [];
    public ICollection<LaminationJobMaterialAllocation> Allocations { get; private set; } = [];

    public void AddStep(LaminationJobStep step) { EnsureEditable(); if (DesignType == LaminationDesignType.Simple && Steps.Count > 0) throw new InvalidOperationException("Simple design permits exactly one step."); if (Steps.Any(x => x.StepNumber == step.StepNumber)) throw new InvalidOperationException("Step Number must be unique within the job."); Steps.Add(step); Recalculate(); }
    public void UpdatePlanning(string? jobOrDrawingNumber, string? customer, string rating, LaminationDesignType designType, StepLapOrientation orientation, int numberOfSteps, Guid gradeId, decimal thickness, string category, decimal coreLossPerKg, decimal totalWeight, decimal customerCoreLossPerKg, Guid? workOrderId, string? workOrderNumber, DateOnly plannedDate, DateOnly? requiredDate, string? shift, string? plannerName, string? remarks) { EnsureEditable(); JobOrDrawingNumber=Clean(jobOrDrawingNumber); Customer=Clean(customer); Rating=Required(rating,"Rating"); GradeId=gradeId; Thickness=thickness; Category=category; CoreLossPerKg=coreLossPerKg; SetCustomerLoss(totalWeight,customerCoreLossPerKg); WorkOrderId=workOrderId; WorkOrderNumber=Clean(workOrderNumber); PlannedDate=plannedDate; RequiredDate=requiredDate; Shift=Clean(shift); PlannerName=Clean(plannerName); Remarks=Clean(remarks); SetDesign(designType,orientation,numberOfSteps); }
    public void ReplaceSchedule(IEnumerable<LaminationJobStep> steps) { EnsureEditable(); Steps.Clear(); foreach (var step in steps) AddStep(step); ValidateDesign(false); Recalculate(); }
    public void ConfirmAllocation(string actor, DateTimeOffset now) { if (Status != LaminationJobStatus.Released) throw new InvalidOperationException("Only a Released job can confirm material allocation."); Status = LaminationJobStatus.Allocated; AllocatedBy = actor; AllocatedOn = now; Recalculate(); }
    public void Release(string actor, DateTimeOffset now) { if (Status != LaminationJobStatus.Draft) throw new InvalidOperationException("Only a Draft job can be released."); if (string.IsNullOrWhiteSpace(JobOrDrawingNumber)) throw new InvalidOperationException("Job No / Drawing No is required before release."); ValidateDesign(true); Status = LaminationJobStatus.Released; ReleasedBy = actor; ReleasedOn = now; }
    public void Complete(int good, int rejected, decimal consumed, decimal scrap, decimal topPlateWeight, decimal bottomPlateWeight, decimal leftSidePlateWeight, decimal rightSidePlateWeight, decimal centerPlateWeight, string? remarks, string actor, DateTimeOffset now) { if(Status!=LaminationJobStatus.Allocated)throw new InvalidOperationException("Only an Allocated job can be completed."); if(good<0||rejected<0||consumed<0||scrap<0||topPlateWeight<0||bottomPlateWeight<0||leftSidePlateWeight<0||rightSidePlateWeight<0||centerPlateWeight<0)throw new InvalidOperationException("Completion values cannot be negative."); TotalGoodPieces=good; TotalRejectedPieces=rejected; TotalConsumedWeight=consumed; TotalScrapWeight=scrap; TopPlateWeight=topPlateWeight; BottomPlateWeight=bottomPlateWeight; LeftSidePlateWeight=leftSidePlateWeight; RightSidePlateWeight=rightSidePlateWeight; CenterPlateWeight=centerPlateWeight; TotalJobWeight=topPlateWeight+bottomPlateWeight+leftSidePlateWeight+rightSidePlateWeight+centerPlateWeight; CompletionRemarks=Clean(remarks); CompletedBy=actor; CompletedOn=now; Status=LaminationJobStatus.Completed; }
    public void Complete(int good, int rejected, decimal consumed, decimal scrap, decimal totalJobWeight, string? remarks, string actor, DateTimeOffset now) { decimal share=totalJobWeight/5m; Complete(good,rejected,consumed,scrap,share,share,share,share,totalJobWeight-share*4m,remarks,actor,now); }
    public void Cancel(string actor, DateTimeOffset now) { if (Status is LaminationJobStatus.Completed or LaminationJobStatus.Cancelled) throw new InvalidOperationException("This job cannot be cancelled."); Status = LaminationJobStatus.Cancelled; CancelledBy = actor; CancelledOn = now; }
    public void SetAttachment(string name, string reference) { DrawingAttachmentName = name; DrawingAttachmentPath = reference; }
    public void ClearAttachment() { DrawingAttachmentName = null; DrawingAttachmentPath = null; }
    public void Recalculate() { TotalPlannedPieces = Steps.SelectMany(x => x.Plates).Sum(x => x.Quantity); TotalPlannedWeight = Steps.SelectMany(x => x.Plates).Sum(x => x.PlannedWeight); TotalAllocatedWeight = Allocations.Where(x => x.Status == AllocationStatus.Reserved).Sum(x => x.AllocatedWeight); }
    private void ValidateDesign(bool release) { if (DesignType == LaminationDesignType.Simple && (NumberOfSteps != 1 || Steps.Count > 1)) throw new InvalidOperationException("Simple design permits exactly one step."); if (DesignType == LaminationDesignType.StepLap && (NumberOfSteps <= 1 || StepLapOrientation == StepLapOrientation.NotApplicable)) throw new InvalidOperationException("Step Lap requires multiple steps and an orientation."); if (release && Steps.Count != NumberOfSteps) throw new InvalidOperationException("Schedule step count must equal Number of Steps."); }
    private void SetDesign(LaminationDesignType design, StepLapOrientation orientation, int count) { DesignType = design; StepLapOrientation = design == LaminationDesignType.Simple ? StepLapOrientation.NotApplicable : orientation; NumberOfSteps = design == LaminationDesignType.Simple ? 1 : count; ValidateDesign(false); }
    private void EnsureEditable() { if (Status != LaminationJobStatus.Draft) throw new InvalidOperationException("The released cutting schedule is locked."); }
    private void SetCustomerLoss(decimal totalWeight, decimal customerCoreLossPerKg) { if (totalWeight <= 0) throw new ArgumentOutOfRangeException(nameof(totalWeight), "Total Weight must be greater than zero."); if (customerCoreLossPerKg <= 0) throw new ArgumentOutOfRangeException(nameof(customerCoreLossPerKg), "Core Loss (W/kg) must be greater than zero."); TotalWeight=totalWeight; CustomerCoreLossPerKg=customerCoreLossPerKg; NoLoadLossWatts=decimal.Round(totalWeight*customerCoreLossPerKg*1.15m,2); }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.") : value.Trim();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class LaminationJobStep : BaseEntity
{
    private LaminationJobStep() { }
    public LaminationJobStep(int stepNumber, int stackQuantity, int sequence, decimal width, decimal plannedWeight, string? remarks = null) { if (stepNumber <= 0 || stackQuantity <= 0 || width <= 0 || plannedWeight < 0) throw new ArgumentOutOfRangeException(nameof(stepNumber), "Step values are invalid."); StepNumber=stepNumber; StackQuantity=stackQuantity; Sequence=sequence; Width=width; PlannedWeight=plannedWeight; Remarks=remarks; }
    public Guid LaminationJobId { get; private set; } public int StepNumber { get; private set; } public int StackQuantity { get; private set; } public int Sequence { get; private set; } public decimal Width { get; private set; } public decimal PlannedWeight { get; private set; } public string? Remarks { get; private set; }
    public ICollection<LaminationJobPlate> Plates { get; private set; } = [];
    public void AddPlate(LaminationJobPlate plate) { if (Plates.Any(x => x.PlateType == plate.PlateType)) throw new InvalidOperationException("Plate Type must be unique within each step."); Plates.Add(plate); }
}

public sealed class LaminationJobPlate : BaseEntity
{
    private LaminationJobPlate() { }
    public LaminationJobPlate(LaminationPlateType plateType, decimal width, decimal? length, int quantity, decimal plannedWeight, string? remarks=null) { if(width<=0 || quantity<=0 || plannedWeight<0) throw new ArgumentOutOfRangeException(nameof(width), "Plate values are invalid."); PlateType=plateType; Width=width; Length=length; Quantity=quantity; PlannedWeight=plannedWeight; Remarks=remarks; }
    public Guid LaminationJobStepId { get; private set; } public LaminationPlateType PlateType { get; private set; } public decimal Width { get; private set; } public decimal? Length { get; private set; } public int Quantity { get; private set; } public decimal PlannedWeight { get; private set; } public string? Remarks { get; private set; }
    public ICollection<LaminationPlateDimension> Dimensions { get; private set; } = [];
    public void AddDimension(LaminationPlateDimension dimension) { if(Dimensions.Any(x=>x.DimensionCode.Equals(dimension.DimensionCode,StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Dimension Code must be unique within a plate."); Dimensions.Add(dimension); }
}

public sealed class LaminationPlateDimension : BaseEntity
{
    private LaminationPlateDimension() { }
    public LaminationPlateDimension(string code,string? displayName,decimal value,string? unit,int sequence,string? remarks=null) { if(string.IsNullOrWhiteSpace(code)||value<0) throw new ArgumentException("Dimension Code is required and value cannot be negative."); DimensionCode=code.Trim(); DisplayName=displayName; DimensionValue=value; Unit=string.IsNullOrWhiteSpace(unit)?"mm":unit.Trim(); Sequence=sequence; Remarks=remarks; }
    public Guid LaminationJobPlateId { get; private set; } public string DimensionCode { get; private set; }=""; public string? DisplayName { get; private set; } public decimal DimensionValue { get; private set; } public string Unit { get; private set; }="mm"; public int Sequence { get; private set; } public string? Remarks { get; private set; }
}

public sealed class LaminationJobMaterialAllocation : BaseEntity
{
    private LaminationJobMaterialAllocation() { }
    public LaminationJobMaterialAllocation(Guid jobId, Guid coilId, string number, decimal requiredWidth, decimal allocated, decimal remaining, string actor, DateTimeOffset now, string? remarks) { if(allocated<=0) throw new ArgumentOutOfRangeException(nameof(allocated)); LaminationJobId=jobId; SlitCoilId=coilId; SlitCoilNumber=number; RequiredWidth=requiredWidth; AllocatedWeight=allocated; RemainingWeightAfterAllocation=remaining; Status=AllocationStatus.Reserved; ReservedBy=actor; ReservedOn=now; CreatedBy=actor; CreatedOn=now; Remarks=remarks; }
    public Guid LaminationJobId { get; private set; } public Guid SlitCoilId { get; private set; } public string SlitCoilNumber { get; private set; }=""; public decimal RequiredWidth { get; private set; } public decimal AllocatedWeight { get; private set; } public decimal? IssuedWeight { get; private set; } public decimal? ConsumedWeight { get; private set; } public decimal RemainingWeightAfterAllocation { get; private set; } public AllocationStatus Status { get; private set; } public string ReservedBy { get; private set; }=""; public DateTimeOffset ReservedOn { get; private set; } public string? ReleasedBy { get; private set; } public DateTimeOffset? ReleasedOn { get; private set; } public string? Remarks { get; private set; } public string? CreatedBy { get; private set; } public DateTimeOffset CreatedOn { get; private set; }
    public void Adjust(decimal allocated, decimal remaining, string actor, DateTimeOffset now, string? remarks) { if(Status != AllocationStatus.Reserved) throw new InvalidOperationException("Only reserved allocations can be adjusted."); if(allocated <= 0) throw new ArgumentOutOfRangeException(nameof(allocated)); AllocatedWeight=allocated; RemainingWeightAfterAllocation=remaining; if(!string.IsNullOrWhiteSpace(remarks))Remarks=remarks.Trim(); }
    public void Release(string actor, DateTimeOffset now) { if(Status != AllocationStatus.Reserved) throw new InvalidOperationException("Only reserved allocations can be released."); Status=AllocationStatus.Released; ReleasedBy=actor; ReleasedOn=now; }
    public void RecordConsumption(decimal consumed,string actor,DateTimeOffset now,string? remarks=null) { if(Status!=AllocationStatus.Reserved)throw new InvalidOperationException("Only reserved allocations can be consumed."); if(consumed<0||consumed>AllocatedWeight)throw new InvalidOperationException("Actual consumption cannot exceed allocated weight."); ConsumedWeight=consumed; ReleasedBy=actor; ReleasedOn=now; if(!string.IsNullOrWhiteSpace(remarks))Remarks=remarks.Trim(); Status=consumed==0?AllocationStatus.Released:consumed==AllocatedWeight?AllocationStatus.Consumed:AllocationStatus.PartiallyConsumed; }
}
