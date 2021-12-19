﻿using System;
using Toolbox.Tools;

namespace Toolbox.Pattern
{
    public static class PatternExtension
    {
        public static PatternSelect AddPattern(this PatternSelect subject, string name, string pattern, Func<PatternContext, PatternResult, PatternResult>? transform = null)
        {
            subject.VerifyNotNull(nameof(subject));
            name.VerifyNotEmpty(nameof(name));
            pattern.VerifyNotEmpty(nameof(pattern));

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

        public static PatternSelect AddTransform(this PatternSelect subject, string name, Func<PatternContext, PatternResult, PatternResult> select)
        {
            subject.VerifyNotNull(nameof(subject));
            name.VerifyNotEmpty(nameof(name));
            select.VerifyNotNull(nameof(select));

            subject.Add(name, select);

            return subject;
        }
    }
}