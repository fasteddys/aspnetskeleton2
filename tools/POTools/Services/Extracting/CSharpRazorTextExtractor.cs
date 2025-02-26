﻿using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;

namespace POTools.Services.Extracting
{
    public class CSharpRazorTextExtractor : CSharpTextExtractor
    {
        private readonly RazorProjectEngine _projectEngine;

        public CSharpRazorTextExtractor() : this(null) { }

        public CSharpRazorTextExtractor(CSharpTextExtractorSettings? settings) : base(settings)
        {
            _projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, RazorProjectFileSystem.Create(@"\"), builder =>
            {
                if (!builder.Features.OfType<SectionDirectivePass>().Any())
                    SectionDirective.Register(builder);

                IRazorTargetExtensionFeature razorTargetExtensionFeature = builder.Features.OfType<IRazorTargetExtensionFeature>().FirstOrDefault();
                if (razorTargetExtensionFeature == null || !razorTargetExtensionFeature.TargetExtensions.OfType<TemplateTargetExtension>().Any())
                {
                    // required for successfully parsing documents containing Templated Razor Delegates
                    builder.AddTargetExtension(new TemplateTargetExtension
                    {
                        TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
                    });
                }
            });
        }

        protected override string GetCode(string content, CancellationToken cancellationToken)
        {
            var sourceDocument = RazorSourceDocument.Create(content, "_");
            var codeDocument = _projectEngine.Process(sourceDocument, fileKind: null, importSources: null, tagHelpers: null);
            var parsedDocument = codeDocument.GetCSharpDocument();
            var errorDiagnostic = parsedDocument.Diagnostics.OfType<RazorDiagnostic>().FirstOrDefault(d => d.Severity == RazorDiagnosticSeverity.Error);
            if (errorDiagnostic != null)
                throw new ArgumentException($"Razor code has errors: {errorDiagnostic}.", nameof(content));

            return parsedDocument.GeneratedCode;
        }
    }
}
