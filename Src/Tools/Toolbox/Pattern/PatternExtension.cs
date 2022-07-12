﻿using System;
using Toolbox.Tools;

namespace Toolbox.Pattern
{
    public static class PatternExtension
    {
        public static PatternCollection AddPattern(this PatternCollection subject, string name, string pattern, Func<PatternContext, PatternResult, PatternResult>? transform = null)
        {
            subject.NotNull();
            name.NotEmpty();
            pattern.NotEmpty();

            subject.Add(name, context =>
            {
                var search = new PatternSearch
                {
                    Name = name,
                    Pattern = pattern
                };

                if (new PatternTransform().TryMatch(search, context.Source, out PatternResult? pathPatternResult) == false) return null;

                return transform?.Invoke(context, pathPatternResult) ?? pathPatternResult;
            });

            return subject;
        }

        public static PatternCollection AddTransform(this PatternCollection subject, string name, Func<PatternContext, PatternResult, PatternResult> select)
        {
            subject.NotNull();
            name.NotEmpty();
            select.NotNull();

            subject.Add(name, select);

            return subject;
        }
    }
}