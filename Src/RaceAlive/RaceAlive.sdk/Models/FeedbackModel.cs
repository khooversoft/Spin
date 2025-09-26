using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace RaceAlive.sdk.Models;

public class FeedbackModel
{
    public string? Subject { get; set; }
    public string Text { get; set; } = null!;
    public string? ContactEmail { get; set; }

    public static IValidator<FeedbackModel> Validator { get; } = new Validator<FeedbackModel>()
        .RuleFor(x => x.Text).NotEmpty()
        .Build();
}

public static class FeedbackModelExtensions
{
    public static Option<IValidatorResult> Validate(this FeedbackModel model) => FeedbackModel.Validator.Validate(model);
}