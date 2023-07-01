using System.ComponentModel.DataAnnotations;
using Toolbox.Types;

namespace SpinPortal.Application;

public class ObjectIdPathValidationAttribute : ValidationAttribute
{
    public string GetErrorMessage() => $"Id must be alpha, numeric, [-._]";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        string id = (string)validationContext.ObjectInstance;

        if (!ObjectId.IsPathValid(id)) return new ValidationResult(GetErrorMessage());

        return ValidationResult.Success!;
    }
}
