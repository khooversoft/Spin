﻿//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace SpinCluster.sdk.Models;

//[GenerateSerializer, Immutable]
//public sealed record UserPhoneModel
//{
//    [Id(0)] public string Type { get; init; } = null!;
//    [Id(1)] public string Number { get; init; } = null!;
//}

//public static class UserPhoneModelValidator
//{
//    public static IValidator<UserPhoneModel> Validator { get; } = new Validator<UserPhoneModel>()
//        .RuleFor(x => x.Type).NotEmpty()
//        .RuleFor(x => x.Number).NotEmpty()
//        .Build();

//    public static Option Validate(this UserPhoneModel subject) => Validator.Validate(subject).ToOptionStatus();
//}