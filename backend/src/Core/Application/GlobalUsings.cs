global using Ardalis.Specification;
global using FluentValidation;
global using FSH.WebApi.Application.Common.Events;
global using FSH.WebApi.Application.Common.Exceptions;
global using FSH.WebApi.Application.Common.FileStorage;
global using FSH.WebApi.Application.Common.Interfaces;
global using FSH.WebApi.Application.Common.Models;
global using FSH.WebApi.Application.Common.Persistence;
global using FSH.WebApi.Application.Common.Specification;
global using FSH.WebApi.Application.Common.Validation;
global using FSH.WebApi.Application.HR.Attendance.Commands;
global using FSH.WebApi.Application.HR.Attendance.Dtos;
global using FSH.WebApi.Application.HR.Attendance.Queries;
global using FSH.WebApi.Application.HR.Benefits.Commands; // Added
global using FSH.WebApi.Application.HR.Benefits.Dtos;   // Added
global using FSH.WebApi.Application.HR.Benefits.Queries;  // Added
global using FSH.WebApi.Application.HR.Employees.Commands;
global using FSH.WebApi.Application.HR.Employees.Queries;
global using FSH.WebApi.Application.HR.Leaves.Commands;
global using FSH.WebApi.Application.HR.Leaves.Queries;
global using FSH.WebApi.Application.HR.LeaveTypes.Commands;
global using FSH.WebApi.Application.HR.LeaveTypes.Queries;
global using FSH.WebApi.Application.HR.Payroll.Commands;
global using FSH.WebApi.Application.HR.Payroll.Queries;
global using FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;  // Added
global using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos;    // Added
global using FSH.WebApi.Application.HR.Recruitment.Applicants.Queries;
global using FSH.WebApi.Application.HR.Recruitment.Interviews.Commands; // Added
global using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;   // Added
global using FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;  // Added
global using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;
global using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos;
global using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries;
global using FSH.WebApi.Domain.Catalog;
global using FSH.WebApi.Domain.Common;
global using FSH.WebApi.Domain.Common.Contracts;
global using FSH.WebApi.Shared.Notifications;
global using MediatR;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;