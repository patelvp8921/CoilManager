using CoilManager.Domain.Entities;using CoilManager.Domain.Enums;
namespace CoilManager.UnitTests.LaminationJobs;
public sealed class LaminationJobPlanningTests
{
 static LaminationJob Job(LaminationDesignType d=LaminationDesignType.Simple,StepLapOrientation o=StepLapOrientation.NotApplicable,int count=1)=>new("AE/C/2026/00001","D-1","Customer","100 KVA",d,o,count,Guid.NewGuid(),.23m,"M3",.85m,1000m,.90m,null,null,new DateOnly(2026,7,14),null,null,null,null);
 static LaminationJobStep Step(int n=1){var s=new LaminationJobStep(n,1,n,235,0);s.AddPlate(new LaminationJobPlate(LaminationPlateType.Side,235,500,10,20));return s;}
 [Fact]public void Job_number_format_is_year_scoped()=>Assert.Equal("AE/C/2026/00001",LaminationJob.FormatNumber(2026,1));
 [Fact]public void Simple_permits_one_step(){var j=Job();j.AddStep(Step());Assert.Single(j.Steps);}
 [Fact]public void Simple_rejects_multiple_steps(){var j=Job();j.AddStep(Step());Assert.Throws<InvalidOperationException>(()=>j.AddStep(Step(2)));}
 [Fact]public void Step_lap_requires_multiple_steps()=>Assert.Throws<InvalidOperationException>(()=>Job(LaminationDesignType.StepLap,StepLapOrientation.Horizontal,1));
 [Fact]public void Step_lap_orientation_is_required()=>Assert.Throws<InvalidOperationException>(()=>Job(LaminationDesignType.StepLap,StepLapOrientation.NotApplicable,2));
 [Fact]public void Step_lap_supports_both_orientations()=>Assert.Equal(StepLapOrientation.HorizontalAndVertical,Job(LaminationDesignType.StepLap,StepLapOrientation.HorizontalAndVertical,2).StepLapOrientation);
 [Fact]public void No_load_loss_includes_fifteen_percent_allowance()=>Assert.Equal(1035m,Job().NoLoadLossWatts);
 [Fact]public void Machine_is_fixed_for_ctl_planning()=>Assert.Equal("CTL-450-GLOBALSPS",Job().Machine);
 [Fact]public void Plate_type_is_unique_within_step(){var s=Step();Assert.Throws<InvalidOperationException>(()=>s.AddPlate(new LaminationJobPlate(LaminationPlateType.Side,235,null,1,0)));}
 [Fact]public void Flexible_OEM_dimensions_are_stored(){var p=new LaminationJobPlate(LaminationPlateType.Top,235,null,1,0);p.AddDimension(new("OEM-X",null,42,null,1));Assert.Equal("mm",p.Dimensions.Single().Unit);}
 [Fact]public void Released_schedule_is_locked(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);Assert.Throws<InvalidOperationException>(()=>j.AddStep(Step(2)));}
 [Fact]public void Draft_job_can_be_released(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);Assert.Equal(LaminationJobStatus.Released,j.Status);Assert.Empty(j.Allocations);}
 [Fact]public void Draft_job_cannot_confirm_allocation(){var j=Job();Assert.Throws<InvalidOperationException>(()=>j.ConfirmAllocation("planner",DateTimeOffset.UtcNow));}
 [Fact]public void Released_job_can_become_allocated(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);j.ConfirmAllocation("planner",DateTimeOffset.UtcNow);Assert.Equal(LaminationJobStatus.Allocated,j.Status);}
 [Fact]public void Released_job_cannot_complete_directly(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);Assert.Throws<InvalidOperationException>(()=>j.Complete(10,0,10,0,10,null,"planner",DateTimeOffset.UtcNow));}
 [Fact]public void Allocated_job_can_complete_only_once(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);j.ConfirmAllocation("planner",DateTimeOffset.UtcNow);j.Complete(10,0,10,0,10,null,"planner",DateTimeOffset.UtcNow);Assert.Equal(LaminationJobStatus.Completed,j.Status);Assert.Throws<InvalidOperationException>(()=>j.Complete(10,0,10,0,10,null,"planner",DateTimeOffset.UtcNow));}
 [Fact]public void Completed_job_cannot_be_cancelled(){var j=Job();j.AddStep(Step());j.Release("planner",DateTimeOffset.UtcNow);j.ConfirmAllocation("planner",DateTimeOffset.UtcNow);j.Complete(10,0,10,0,10,null,"planner",DateTimeOffset.UtcNow);Assert.Throws<InvalidOperationException>(()=>j.Cancel("planner",DateTimeOffset.UtcNow));}
  [Fact]public void Reserved_allocation_can_be_adjusted_without_changing_physical_coil_identity(){var a=new LaminationJobMaterialAllocation(Guid.NewGuid(),Guid.NewGuid(),"SC-009",235,100,200,"planner",DateTimeOffset.UtcNow,null);a.Adjust(150,150,"planner",DateTimeOffset.UtcNow,"Adjusted");Assert.Equal(150,a.AllocatedWeight);Assert.Equal(150,a.RemainingWeightAfterAllocation);Assert.Equal("SC-009",a.SlitCoilNumber);Assert.Equal(AllocationStatus.Reserved,a.Status);}
 [Fact]public void Partial_consumption_keeps_coil_number_and_releases_unused_weight(){var a=new LaminationJobMaterialAllocation(Guid.NewGuid(),Guid.NewGuid(),"SC-001",235,500,1000,"planner",DateTimeOffset.UtcNow,null);a.RecordConsumption(470,"planner",DateTimeOffset.UtcNow);Assert.Equal("SC-001",a.SlitCoilNumber);Assert.Equal(AllocationStatus.PartiallyConsumed,a.Status);Assert.Equal(470,a.ConsumedWeight);}}
