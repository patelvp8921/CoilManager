using System.Diagnostics;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Domain.Entities;
using CoilManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoilManager.Persistence.DemoData;

public sealed class DemoDataSeeder(ApplicationDbContext db, ILogger<DemoDataSeeder> log) : IDemoDataSeeder
{
    private const int Seed = 2026;
    private static readonly decimal[] MotherWidths=[950,980,1000,1020,1045];
    private static readonly decimal[] SlitWidths=[120,150,180,210,225,235,250,265,280,300,320,400,470];
    private static readonly string[] Warehouses=["WH-A / A01","WH-A / A02","WH-B / B01","WH-C / C01","WH-D / D01"];

    public async Task<DemoDataSummary> GenerateAsync(GenerateDemoDataCommand command,CancellationToken token=default)
    {
        var watch=Stopwatch.StartNew(); var random=new Random(Seed);
        await using var transaction=await db.Database.BeginTransactionAsync(token);
        if(command.ClearExistingData){log.LogInformation("Clearing existing production demo data...");await ClearAsync(token);if(string.Equals(command.Stage,"Clear",StringComparison.OrdinalIgnoreCase)){await transaction.CommitAsync(token);watch.Stop();return new(0,0,0,0,0,0,watch.ElapsedMilliseconds,"Development demo data cleared successfully.");}}
        if(await db.RawCoils.AnyAsync(x=>x.RawCoilNumber.StartsWith("MC-2026-"),token)&&!command.ClearExistingData)
            return new(0,0,0,0,0,0,watch.ElapsedMilliseconds,"Demo data already exists. Use Clear Existing Data to regenerate deterministically.");

        log.LogInformation("Generating master data..."); var (suppliers,manufacturers,grades)=await EnsureMasterDataAsync(token);
        log.LogInformation("Generating Mother Coils..."); var mothers=CreateMotherCoils(random,suppliers,manufacturers,grades);
        await db.RawCoils.AddRangeAsync(mothers,token); await db.SaveChangesAsync(token);

        if(string.Equals(command.Stage,"MotherCoils",StringComparison.OrdinalIgnoreCase))
        {
            await transaction.CommitAsync(token);watch.Stop();
            log.LogInformation("Mother Coil demo data completed successfully in {Elapsed} ms",watch.ElapsedMilliseconds);
            return new(mothers.Count,0,0,0,0,0,watch.ElapsedMilliseconds,"Mother Coil demo data generated successfully.");
        }

        log.LogInformation("Generating Slitting Jobs and Slit Coils..."); var jobs=new List<SlittingJob>();var slits=new List<SlitCoil>();var inventory=new List<InventoryTransaction>();
        for(int i=0;i<30;i++)
        {
            var mother=mothers[i%mothers.Count];var date=DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(10,365)));
            var job=new SlittingJob($"SJ-2026-{i+1:00000}",date,$"Planner-{i%6+1}",mother.Id,null,i%2==0?"A":"B",.2m,2,2,"Deterministic development demo data");
            int count=i<10?10:9;var items=new List<SlittingJobItem>();
            for(int n=0;n<count;n++)
            {
                decimal width=SlitWidths[(i*3+n)%SlitWidths.Length];decimal weight=Math.Round(40+(decimal)random.NextDouble()*610,3);
                string number=$"SC-2026-{slits.Count+1:00000}";items.Add(new(n+1,number,width,weight,"Demo slit"));
                var coil=new SlitCoil(number,mother.Id,mother.Id,mother.Id,job.Id,n+1,1,mother.GradeId,mother.SupplierId,mother.ManufacturerId,mother.HeatNumber,mother.Thickness,mother.Category,mother.CoreLossPerKg,width,weight,Warehouses[(i+n)%Warehouses.Length],"1");
                slits.Add(coil);inventory.Add(new(InventoryTransactionType.SlitCoilGeneration,CoilType.SlitCoil,coil.Id,number,job.Id,job.SlittingJobNo,null,CoilStatus.Available,weight,date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc),"Demo slit coil generated"));
            }
            job.RebuildItems(items);int bucket=i%10;if(bucket>=2){job.Release("Demo Planner",date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(8));inventory.Add(new(InventoryTransactionType.SlittingJobRelease,CoilType.MotherCoil,mother.Id,mother.RawCoilNumber,job.Id,job.SlittingJobNo,CoilStatus.Available,CoilStatus.Reserved,0,date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(8),"Demo slitting job released"));}if(bucket>=4){job.Start("Demo Operator",date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(10),null,"A",null);job.Complete("Demo Operator",date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(18));foreach(var item in job.Items)item.Complete(item.Width,item.EstimatedWeight,null);inventory.Add(new(InventoryTransactionType.SlittingJobComplete,CoilType.MotherCoil,mother.Id,mother.RawCoilNumber,job.Id,job.SlittingJobNo,CoilStatus.Reserved,CoilStatus.Available,0,date.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(18),"Demo slitting completed"));}
            jobs.Add(job);
        }
        var slittingTransactionCount=inventory.Count;await db.SlittingJobs.AddRangeAsync(jobs,token);await db.SlitCoils.AddRangeAsync(slits,token);await db.InventoryTransactions.AddRangeAsync(inventory,token);await db.SaveChangesAsync(token);

        log.LogInformation("Generating Lamination Jobs and Material Allocation...");var laminationJobs=new List<LaminationJob>();int allocationCount=0;var reserved=new Dictionary<Guid,decimal>();
        for(int i=0;i<30;i++)
        {
            bool stepLap=i%10>=4;int stepCount=stepLap?3+random.Next(6):1;var candidate=slits[(i*9)%slits.Count];var grade=grades.Single(x=>x.Id==candidate.GradeId);var planned=DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(1,330)));
            var job=new LaminationJob($"LJ-2026-{i+1:00000}",$"DR-{2026}-{i+1:0000}",new[]{"ABB","Siemens","Hitachi","CG","GE","BHEL","Voltamp","Kirloskar","Schneider"}[i%9],$"{new[]{100,160,250,315,500,630,1000,1250}[i%8]} kVA",stepLap?LaminationDesignType.StepLap:LaminationDesignType.Simple,stepLap?StepLapOrientation.HorizontalAndVertical:StepLapOrientation.NotApplicable,stepCount,grade.Id,grade.ThicknessMm,grade.Category,grade.CoreLossPerKg,1000+random.Next(2000),grade.CoreLossPerKg,null,null,planned,planned.AddDays(random.Next(7,30)),i%2==0?"A":"B",$"Planner-{i%6+1}","Development demo Lamination Job");
            for(int step=1;step<=stepCount;step++)
            {
                int stack=10+random.Next(31);decimal length=650+random.Next(500);decimal unit=Math.Round(candidate.Width*length*grade.ThicknessMm*7650m/1_000_000_000m,3);var schedule=new LaminationJobStep(step,stack,step,candidate.Width,unit*stack*5,"Demo step");
                schedule.AddPlate(new(LaminationPlateType.Top,candidate.Width,length,stack,unit*stack,"Demo Top"));schedule.AddPlate(new(LaminationPlateType.Bottom,candidate.Width,length,stack,unit*stack,"Demo Bottom"));schedule.AddPlate(new(LaminationPlateType.Side,candidate.Width,length,stack*2,unit*stack*2,"Demo Left/Right"));schedule.AddPlate(new(LaminationPlateType.Center,candidate.Width,length,stack,unit*stack,"Demo Center"));job.AddStep(schedule);
            }
            int status=i%10;if(status>=2){job.Release("Demo Planner",planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(8));}
            if(status>=5)
            {
                decimal required=job.TotalPlannedWeight;var matching=slits.Where(x=>x.GradeId==job.GradeId&&x.Thickness==job.Thickness&&x.Width==candidate.Width&&x.Weight-reserved.GetValueOrDefault(x.Id)>0).Take(3).ToArray();decimal left=status<7?required*.55m:required;
                foreach(var coil in matching)
                {
                    if(left<=0)break;decimal available=coil.Weight-reserved.GetValueOrDefault(coil.Id);decimal amount=Math.Round(Math.Min(left,available),3);if(amount<=0)continue;var allocation=new LaminationJobMaterialAllocation(job.Id,coil.Id,coil.CoilNumber,candidate.Width,amount,available-amount,"Demo Planner",planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(1),"Demo reservation");job.Allocations.Add(allocation);reserved[coil.Id]=reserved.GetValueOrDefault(coil.Id)+amount;left-=amount;allocationCount++;inventory.Add(new(InventoryTransactionType.LaminationAllocationReserved,CoilType.SlitCoil,coil.Id,coil.CoilNumber,job.Id,job.LaminationJobNumber,CoilStatus.Available,CoilStatus.Available,amount,planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(1),"Demo Lamination reservation"));
                }
                job.Recalculate();if(status>=7&&left<=.001m){job.ConfirmAllocation("Demo Planner",planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(2));}
                if(status>=8&&job.Status==LaminationJobStatus.Allocated)
                {
                    foreach(var allocation in job.Allocations.Where(x=>x.Status==AllocationStatus.Reserved).ToArray()){var coil=slits.Single(x=>x.Id==allocation.SlitCoilId);decimal consumed=Math.Round(allocation.AllocatedWeight*.9m,3);allocation.RecordConsumption(consumed,"Demo Operator",planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(3));coil.ConsumeForLamination(consumed);inventory.Add(new(InventoryTransactionType.LaminationConsumption,CoilType.SlitCoil,coil.Id,coil.CoilNumber,job.Id,job.LaminationJobNumber,CoilStatus.Available,coil.Status,consumed,planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(3),"Demo Lamination consumption"));}job.Complete(job.TotalPlannedPieces,0,job.Allocations.Sum(x=>x.ConsumedWeight??0),0,job.TotalWeight,"Demo completion","Demo Operator",planned.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddDays(3));
                }
            }
            laminationJobs.Add(job);
        }
        await db.LaminationJobs.AddRangeAsync(laminationJobs,token);await db.InventoryTransactions.AddRangeAsync(inventory.Skip(slittingTransactionCount),token);await db.SaveChangesAsync(token);await transaction.CommitAsync(token);watch.Stop();
        log.LogInformation("Demo data completed successfully in {Elapsed} ms",watch.ElapsedMilliseconds);
        return new(mothers.Count,jobs.Count,slits.Count,laminationJobs.Count,allocationCount,inventory.Count,watch.ElapsedMilliseconds,"Development demo data generated successfully.");
    }

    private async Task ClearAsync(CancellationToken token)
    {
        await db.WorkOrderMaterialAllocations.ExecuteDeleteAsync(token);
        await db.LaminationJobMaterialAllocations.ExecuteDeleteAsync(token);await db.LaminationPlateDimensions.ExecuteDeleteAsync(token);await db.LaminationJobPlates.ExecuteDeleteAsync(token);await db.LaminationJobSteps.ExecuteDeleteAsync(token);await db.LaminationJobs.IgnoreQueryFilters().ExecuteDeleteAsync(token);
        await db.SlitCoilLabelPrintHistories.ExecuteDeleteAsync(token);await db.InventoryTransactions.ExecuteDeleteAsync(token);await db.SlitCoils.IgnoreQueryFilters().ExecuteDeleteAsync(token);await db.SlittingJobItems.ExecuteDeleteAsync(token);await db.SlittingJobs.ExecuteDeleteAsync(token);await db.RawCoils.IgnoreQueryFilters().ExecuteDeleteAsync(token);db.ChangeTracker.Clear();
    }

    private async Task<(List<Supplier>,List<Manufacturer>,List<Grade>)> EnsureMasterDataAsync(CancellationToken token)
    {
        string[] supplierNames=["Prime Steel","National Coil","Apex Metals","Bharat Steel","Eastern Metals","Global CRGO","Precision Steel","Shakti Metals","Unity Coils","Zenith Steel"];
        string[] manufacturerNames=["Tata Steel","JSW Steel","SAIL","Baosteel","Nippon Steel","POSCO","ThyssenKrupp","JFE Steel","ArcelorMittal","Voestalpine"];
        for(int i=0;i<10;i++){string code=$"DS{i+1:00}";if(!await db.Suppliers.AnyAsync(x=>x.Code==code,token))db.Suppliers.Add(new(supplierNames[i],code,"Development demo supplier"));string mcode=$"DM{i+1:00}";if(!await db.Manufacturers.AnyAsync(x=>x.Code==mcode,token))db.Manufacturers.Add(new(manufacturerNames[i],mcode,"Development demo manufacturer",true,i<6?"India":"International"));}
        var gradeSeeds=new[]{("23HP85",.23m,.85m),("23HP90",.23m,.90m),("27M4",.27m,1.00m),("30M5",.30m,1.10m),("35M6",.35m,1.25m)};foreach(var g in gradeSeeds)if(!await db.Grades.AnyAsync(x=>x.Code==g.Item1,token))db.Grades.Add(new Grade(g.Item1,g.Item2,g.Item3));await db.SaveChangesAsync(token);
        return(await db.Suppliers.Where(x=>x.Code.StartsWith("DS")).OrderBy(x=>x.Code).Take(10).ToListAsync(token),await db.Manufacturers.Where(x=>x.Code.StartsWith("DM")).OrderBy(x=>x.Code).Take(10).ToListAsync(token),await db.Grades.Where(x=>gradeSeeds.Select(g=>g.Item1).Contains(x.Code)).ToListAsync(token));
    }

    private static List<RawCoil> CreateMotherCoils(Random random,List<Supplier> suppliers,List<Manufacturer> manufacturers,List<Grade> grades)
    {
        var rows=new List<RawCoil>();for(int i=0;i<50;i++){var grade=grades[i%grades.Count];decimal weight=random.Next(3800,6201);var status=i<15?CoilStatus.Available:i<40?CoilStatus.Reserved:CoilStatus.Consumed;var row=new RawCoil($"MC-2026-{i+1:00000}",$"MILL-{i+1:00000}",$"HEAT-{2026}-{i+1:0000}",$"PO-{i+1:0000}",$"INV-{i+1:0000}",$"TC-{i+1:0000}",null,suppliers[i%suppliers.Count].Id,manufacturers[i%manufacturers.Count].Id,grade.Id,grade.ThicknessMm,grade.Category,grade.CoreLossPerKg,MotherWidths[i%MotherWidths.Length],weight,Math.Round(weight*1000/(MotherWidths[i%MotherWidths.Length]*grade.ThicknessMm*7.65m),2),Warehouses[i%Warehouses.Length],DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(1,365))),status);rows.Add(row);}return rows;
    }
}