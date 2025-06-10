using FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;
using FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;

namespace FSH.WebApi.Host.Controllers.HR;

public class InterviewsController : VersionedApiController
{
    [HttpPost("schedule")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Schedule, FSHResource.HRInterviews)]
    [OpenApiOperation("Schedule a new interview.", "")]
    public async Task<ActionResult<Guid>> ScheduleAsync(ScheduleInterviewRequest request)
    {
        return Ok(await Mediator.Send(request));
    }

    [HttpPut("{id:guid}/reschedule")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Update, FSHResource.HRInterviews)]
    [OpenApiOperation("Reschedule an existing interview.", "")]
    public async Task<ActionResult<Guid>> RescheduleAsync(Guid id, RescheduleInterviewRequest request)
    {
        if (id != request.InterviewId)
        {
            // It's good practice to also have the ID in the request body for validation,
            // but if the request object is designed to get ID from route, ensure it's set.
            // For this example, assuming RescheduleInterviewRequest has InterviewId property.
            // If request.InterviewId is not meant to be in body, then it should be:
            // var command = new RescheduleInterviewRequest { InterviewId = id, ... (map other props from request DTO if body DTO is different) }
            // For now, assuming request DTO has InterviewId and we check consistency.
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("{id:guid}/cancel")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.Cancel, FSHResource.HRInterviews)]
    [OpenApiOperation("Cancel an interview.", "")]
    public async Task<ActionResult<Guid>> CancelAsync(Guid id, CancelInterviewRequest request)
    {
        // Similar to Reschedule, ensure ID consistency or construct command with ID from route.
        // Assuming CancelInterviewRequest takes ID in constructor or has settable property.
        if (id != request.InterviewId)
        {
             return BadRequest();
        }
        // If request is just { Reason: "..." } in body, then:
        // return Ok(await Mediator.Send(new CancelInterviewRequest(id, request.Reason)));
        return Ok(await Mediator.Send(request));
    }

    [HttpPost("{id:guid}/feedback")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.RecordFeedback, FSHResource.HRInterviews)]
    [OpenApiOperation("Record feedback for a completed interview.", "")]
    public async Task<ActionResult<Guid>> RecordFeedbackAsync(Guid id, RecordInterviewFeedbackRequest request)
    {
        if (id != request.Id) // RecordInterviewFeedbackRequest uses 'Id' for InterviewId
        {
            return BadRequest();
        }
        return Ok(await Mediator.Send(request));
    }

    [HttpGet("{id:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRInterviews)]
    [OpenApiOperation("Get interview details by ID.", "")]
    public async Task<ActionResult<InterviewDto>> GetByIdAsync(Guid id)
    {
        var interview = await Mediator.Send(new GetInterviewByIdRequest(id));
        return Ok(interview);
    }

    [HttpGet("applicant/{applicantId:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRInterviews)]
    [OpenApiOperation("Get all interviews for an applicant.", "")]
    public async Task<ActionResult<List<InterviewDto>>> GetByApplicantAsync(Guid applicantId)
    {
        var interviews = await Mediator.Send(new GetInterviewsByApplicantRequest(applicantId));
        return Ok(interviews);
    }

    [HttpGet("jobopening/{jobOpeningId:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRInterviews)]
    [OpenApiOperation("Get all interviews for a job opening.", "")]
    public async Task<ActionResult<List<InterviewDto>>> GetByJobOpeningAsync(Guid jobOpeningId)
    {
        var interviews = await Mediator.Send(new GetInterviewsByJobOpeningRequest(jobOpeningId));
        return Ok(interviews);
    }

    [HttpGet("interviewer/{interviewerId:guid}")]
    // TODO: Add Permissions - e.g., [MustHavePermission(FSHAction.View, FSHResource.HRInterviews)] (Interviewer viewing their own)
    [OpenApiOperation("Get all interviews for an interviewer.", "")]
    public async Task<ActionResult<List<InterviewDto>>> GetByInterviewerAsync(Guid interviewerId)
    {
        var interviews = await Mediator.Send(new GetInterviewsByInterviewerRequest(interviewerId));
        return Ok(interviews);
    }
}
