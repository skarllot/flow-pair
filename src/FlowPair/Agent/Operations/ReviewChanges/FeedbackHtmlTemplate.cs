﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Raiqub.LlmTools.FlowPair.Agent.Operations.ReviewChanges
{
    using System.Collections.Immutable;
    using System.Text;
    using Common;
    using v1;
    using T4Template;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class FeedbackHtmlTemplate : T4TemplateBase<ImmutableList<ReviewerFeedbackResponse>>
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("<!DOCTYPE html>\r\n<html>\r\n  <head>\r\n    <meta charset=\"utf-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n    <title>Code Review Feedback</title>\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/bulma/1.0.2/css/bulma.min.css\" integrity=\"sha512-RpeJZX3aH5oZN3U3JhE7Sd+HG8XQsqmP3clIbu4G28p668yNsRNj3zMASKe1ATjl/W80wuEtCx2dFA8xaebG5w==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\" />\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.0/styles/default.min.css\" integrity=\"sha512-hasIneQUHlh06VNBe7f6ZcHmeRTLIaQWFd43YriJ0UND19bvYRauxthDg8E4eVNPm9bRUhr5JGeqH7FRFXQu5g==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\" />\r\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.0/styles/github-dark.min.css\" integrity=\"sha512-rO+olRTkcf304DQBxSWxln8JXCzTHlKnIdnMUwYvQa9/Jd4cQaNkItIUj6Z4nvW1dqK0SKXLbn9h4KwZTNtAyw==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\" />\r\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.11.0/highlight.min.js\" integrity=\"sha512-6QBAC6Sxc4IF04SvIg0k78l5rP5YgVjmHX2NeArelbxM3JGj4imMqfNzEta3n+mi7iG3nupdLnl3QrbfjdXyTg==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\"></script>\r\n  </head>\r\n  <body>\r\n  <section class=\"section\">\r\n    <div class=\"container\">\r\n      <h1 class=\"title\">Code Review Feedback</h1>\r\n      <div class=\"content\">\r\n");

    foreach (var response in Model)
    {

            this.Write("        <div class=\"box\">\r\n            <h2 class=\"subtitle\">\r\n                <span class=\"tag");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(GetRiskLevelCssClass(response.RiskScore)));
            
            #line default
            #line hidden
            this.Write("\">Risk Score: ");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(response.RiskScore));
            
            #line default
            #line hidden
            this.Write("</span> ");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(response.Title));
            
            #line default
            #line hidden
            this.Write("\r\n            </h2>\r\n            <p><strong>File:</strong> ");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(response.Path));
            
            #line default
            #line hidden
            this.Write(":");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(response.LineRange));
            
            #line default
            #line hidden
            this.Write("</p>\r\n            <div class=\"notification\">\r\n                ");
            
            this.Write(this.ToStringHelper.ToStringWithCulture(HtmlFormatter.EncodeToHtml(response.Feedback)));
            
            #line default
            #line hidden
            this.Write("\r\n            </div>\r\n        </div>\r\n");

    }

            this.Write("      </div>\r\n    </div>\r\n  </section>\r\n  <script>hljs.highlightAll();</script>\r\n  </body>\r\n</html>\r\n");
            return this.GenerationEnvironment.ToString();
        }

    public FeedbackHtmlTemplate(ImmutableList<ReviewerFeedbackResponse> model) : base(new StringBuilder())
    {
        Model = model;
    }

    private static string GetRiskLevelCssClass(int number) => number switch{
        0 => " is-white",
        1 => " is-success",
        2 => " is-warning",
        3 => " is-danger",
        _ => ""};

    }
}
