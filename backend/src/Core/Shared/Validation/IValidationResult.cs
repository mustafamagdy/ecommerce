﻿// namespace FSH.WebApi.Shared.Validation;
//
// public interface IValidationResult
// {
//   public static readonly Error ValidationError = new("ValidationError", "A validation error");
//   Error[] Errors { get; }
// }
//
// public sealed class ValidationResult : Result, IValidationResult
// {
//   private ValidationResult(Error[] errors)
//     : base(false, IValidationResult.ValidationError) =>
//     Errors = errors;
//
//   public Error[] Errors { get; }
//   public static ValidationResult WithErrors(Error[] errors) => new (errors);
// }
//
// public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
// {
//   private ValidationResult(Error[] errors)
//     : base(default, false, IValidationResult.ValidationError) =>
//     Errors = errors;
//
//   public Error[] Errors { get; }
//   public static ValidationResult WithErrors(Error[] errors) => new (errors);
// }