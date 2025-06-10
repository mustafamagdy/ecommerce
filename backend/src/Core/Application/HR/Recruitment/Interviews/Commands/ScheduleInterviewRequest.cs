using FSH.WebApi.Domain.HR; // For InterviewType enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class ScheduleInterviewRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid ApplicantId { get; set; }
    public Guid InterviewerId { get; set; } // Employee Id
    public DateTime ScheduledTime { get; set; }
    public InterviewType Type { get; set; }
    public string? Location { get; set; } // New: Physical address or meeting link
    public string? Notes { get; set; } // Optional internal notes for the interview
}
