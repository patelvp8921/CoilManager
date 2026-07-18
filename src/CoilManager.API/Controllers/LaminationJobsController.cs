using CoilManager.Application.DTOs.LaminationJobs;
using CoilManager.Application.Interfaces.Services;
using CoilManager.Shared.Pagination;
using CoilManager.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoilManager.API.Controllers;
[Route("api/lamination-jobs")]
public sealed class LaminationJobsController(ILaminationJobService service,IWebHostEnvironment environment) : BaseApiController
{
 const long MaxDrawingSize=15*1024*1024; static readonly HashSet<string> Allowed=[".pdf",".png",".jpg",".jpeg"];
 [HttpGet] public async Task<ActionResult<ApiPagedResponse<LaminationJobListItemDto>>> List([FromQuery]LaminationJobQueryRequest request,CancellationToken t){var r=await service.GetAsync(request,t);return Paged(r.Items,new PaginationResult(r.PageNumber,r.PageSize,r.TotalCount));}
 [HttpGet("next-number")] public async Task<ActionResult<ApiResponse<string>>> Next(CancellationToken t)=>Success(await service.GetNextNumberAsync(t));
 [HttpGet("by-number/{jobNumber}")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Number(string jobNumber,CancellationToken t)=>Success(await service.GetByNumberAsync(jobNumber,t));
 [HttpGet("{id:guid}")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Get(Guid id,CancellationToken t)=>Success(await service.GetAsync(id,t));
 [HttpPost] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Create(CreateLaminationJobRequest r,CancellationToken t){var x=await service.CreateAsync(r,t);return CreatedAtAction(nameof(Get),new{id=x.Id},ApiResponse<LaminationJobDetailsDto>.Ok(x,"Lamination Job created."));}
 [HttpPut("{id:guid}")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Update(Guid id,UpdateLaminationJobRequest r,CancellationToken t)=>Success(await service.UpdateAsync(id,r,t),"Lamination Job updated.");
 [HttpDelete("{id:guid}")] public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id,CancellationToken t){await service.DeleteAsync(id,t);return Success<object>(null,"Lamination Job deleted.");}
 [HttpGet("{id:guid}/requirements")] public async Task<ActionResult<ApiResponse<IReadOnlyList<LaminationMaterialRequirementDto>>>> Requirements(Guid id,CancellationToken t)=>Success(await service.RequirementsAsync(id,t));
 [HttpGet("{id:guid}/available-slit-coils")] public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableSlitCoilDto>>>> Available(Guid id,[FromQuery]AvailableSlitCoilQueryRequest r,CancellationToken t)=>Success(await service.AvailableAsync(id,r,t));
 [HttpGet("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<IReadOnlyList<LaminationJobMaterialAllocationDto>>>> Allocations(Guid id,CancellationToken t)=>Success(await service.AllocationsAsync(id,t));
 [HttpPost("{id:guid}/allocations")] public async Task<ActionResult<ApiResponse<LaminationJobMaterialAllocationDto>>> Allocate(Guid id,CreateLaminationAllocationRequest r,CancellationToken t)=>Success(await service.AllocateAsync(id,r,t),"Material allocated.");
 [HttpDelete("{id:guid}/allocations/{allocationId:guid}")] [HttpPost("{id:guid}/allocations/{allocationId:guid}/release")] public async Task<ActionResult<ApiResponse<object>>> ReleaseAllocation(Guid id,Guid allocationId,CancellationToken t){await service.ReleaseAllocationAsync(id,allocationId,t);return Success<object>(null,"Allocation released.");}
 [HttpPost("{id:guid}/confirm-allocation")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Confirm(Guid id,ConfirmLaminationAllocationRequest r,CancellationToken t)=>Success(await service.ConfirmAsync(id,r,t),"Material allocation confirmed.");
 [HttpPost("{id:guid}/release")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Release(Guid id,ReleaseLaminationJobRequest r,CancellationToken t)=>Success(await service.ReleaseAsync(id,r,t),"Lamination Job released.");
 [HttpPost("{id:guid}/complete")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Complete(Guid id,CompleteLaminationJobRequest r,CancellationToken t)=>Success(await service.CompleteAsync(id,r,t),"Lamination Job completed successfully.");
 [HttpGet("{id:guid}/completion")] public async Task<ActionResult<ApiResponse<LaminationCompletionDto>>> Completion(Guid id,CancellationToken t)=>Success(await service.CompletionAsync(id,t));
 [HttpPost("{id:guid}/cancel")] public async Task<ActionResult<ApiResponse<LaminationJobDetailsDto>>> Cancel(Guid id,CancellationToken t)=>Success(await service.CancelAsync(id,t),"Lamination Job cancelled.");
 [HttpGet("metrics")] public async Task<ActionResult<ApiResponse<LaminationMetricsDto>>> Metrics(CancellationToken t)=>Success(await service.MetricsAsync(t));
 [HttpPost("{id:guid}/drawing")] [RequestSizeLimit(MaxDrawingSize)] public async Task<ActionResult<ApiResponse<object>>> Upload(Guid id,IFormFile file,CancellationToken t){string ext=Path.GetExtension(file.FileName).ToLowerInvariant();if(file.Length<=0||file.Length>MaxDrawingSize||!Allowed.Contains(ext))return BadRequest(ApiResponse<object>.Fail("Drawing must be a PDF, PNG, JPG or JPEG up to 15 MB."));string dir=Path.Combine(environment.ContentRootPath,"App_Data","lamination-drawings");Directory.CreateDirectory(dir);string stored=$"{id:N}-{Guid.NewGuid():N}{ext}";string path=Path.Combine(dir,stored);await using(var stream=System.IO.File.Create(path))await file.CopyToAsync(stream,t);await service.SetDrawingAsync(id,Path.GetFileName(file.FileName),stored,t);return Success<object>(null,"Drawing attached.");}
 [HttpGet("{id:guid}/drawing")] public async Task<IActionResult> Drawing(Guid id,CancellationToken t){var d=await service.GetDrawingAsync(id,t);string path=Path.Combine(environment.ContentRootPath,"App_Data","lamination-drawings",Path.GetFileName(d.Reference));if(!System.IO.File.Exists(path))return NotFound();return PhysicalFile(path,ContentType(Path.GetExtension(path)),d.Name);}
 [HttpDelete("{id:guid}/drawing")] public async Task<ActionResult<ApiResponse<object>>> DeleteDrawing(Guid id,CancellationToken t){string? stored=await service.DeleteDrawingAsync(id,t);if(stored is not null){string path=Path.Combine(environment.ContentRootPath,"App_Data","lamination-drawings",Path.GetFileName(stored));if(System.IO.File.Exists(path))System.IO.File.Delete(path);}return Success<object>(null,"Drawing removed.");}
 static string ContentType(string ext)=>ext.ToLowerInvariant() switch{".pdf"=>"application/pdf",".png"=>"image/png",_=>"image/jpeg"};
}
