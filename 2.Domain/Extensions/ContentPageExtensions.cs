using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PT.Shared.Helpers;

namespace PT.Domain.Extensions
{
    /// <summary>
    /// Extension methods for ContentPage model
    /// </summary>
    public static class ContentPageExtensions
    {
        /// <summary>
        /// ===== SIMPLE STATIC HELPERS =====
        /// Render a single section from template HTML and JSON data
        /// Usage: var html = ContentPageExtensions.GetHtmlSection(input1, input2);
        /// </summary>
        /// <param name="templateHtml">Template HTML with [For]...[/For] and [fieldName]</param>
        /// <param name="dataJson">JSON data string</param>
        /// <param name="wrapInDiv">Whether to wrap in div. Default: false</param>
        /// <param name="sectionNumber">Optional section number for wrapper class. Default: 0</param>
        /// <returns>Rendered HTML string</returns>
        public static string GetHtmlSection(
            string templateHtml, 
            string dataJson, 
            bool wrapInDiv = false, 
            int sectionNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(templateHtml))
                return string.Empty;

            var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
            
            if (string.IsNullOrWhiteSpace(renderedHtml))
                return string.Empty;

            if (wrapInDiv && sectionNumber > 0)
            {
                return $"<div class=\"section section-{sectionNumber}\" data-section=\"{sectionNumber}\">{Environment.NewLine}{renderedHtml}{Environment.NewLine}</div>";
            }
            else if (wrapInDiv)
            {
                return $"<div class=\"section-content\">{Environment.NewLine}{renderedHtml}{Environment.NewLine}</div>";
            }

            return renderedHtml;
        }

        /// <summary>
        /// Render multiple sections from pairs of (templateHtml, dataJson)
        /// Usage: var html = ContentPageExtensions.GetHtmlSections((input1, input2), (input3, input4));
        /// </summary>
        /// <param name="sections">Pairs of (templateHtml, dataJson)</param>
        /// <returns>Combined HTML string</returns>
        public static string GetHtmlSections(params (string templateHtml, string dataJson)[] sections)
        {
            if (sections == null || sections.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();
            int sectionNumber = 1;

            foreach (var (templateHtml, dataJson) in sections)
            {
                if (!string.IsNullOrWhiteSpace(templateHtml))
                {
                    var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
                    
                    if (!string.IsNullOrWhiteSpace(renderedHtml))
                    {
                        sb.AppendLine($"<div class=\"section section-{sectionNumber}\" data-section=\"{sectionNumber}\">");
                        sb.AppendLine(renderedHtml);
                        sb.AppendLine("</div>");
                    }
                }
                
                sectionNumber++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render multiple sections with custom wrapper
        /// Usage: var html = ContentPageExtensions.GetHtmlSectionsWithWrapper("<section>{0}</section>", (input1, input2), (input3, input4));
        /// </summary>
        /// <param name="wrapperFormat">Wrapper format with {0} for content</param>
        /// <param name="sections">Pairs of (templateHtml, dataJson)</param>
        /// <returns>Combined HTML string</returns>
        public static string GetHtmlSectionsWithWrapper(
            string wrapperFormat, 
            params (string templateHtml, string dataJson)[] sections)
        {
            if (string.IsNullOrWhiteSpace(wrapperFormat) || sections == null || sections.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var (templateHtml, dataJson) in sections)
            {
                if (!string.IsNullOrWhiteSpace(templateHtml))
                {
                    var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
                    
                    if (!string.IsNullOrWhiteSpace(renderedHtml))
                    {
                        var wrapped = string.Format(wrapperFormat, renderedHtml);
                        sb.AppendLine(wrapped);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// ===== CONTENTPAGE EXTENSION METHODS =====
        /// Render all sections (Input1-10) into a single HTML string
        /// Each section is wrapped in a div with section-specific attributes
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionOrder">Optional: Custom section order (1-based). Default: [1,2,3,4,5]</param>
        /// <param name="wrapInDiv">Whether to wrap each section in a div. Default: true</param>
        /// <returns>Combined HTML string</returns>
        public static string RenderAllSections(
            this Model.ContentPage contentPage, 
            List<int> sectionOrder = null, 
            bool wrapInDiv = true)
        {
            if (contentPage == null)
                return string.Empty;

            // Default order: Section 1 to 5 (Input1+2, Input3+4, Input5+6, Input7+8, Input9+10)
            sectionOrder ??= new List<int> { 1, 2, 3, 4, 5 };

            var sb = new StringBuilder();

            foreach (var sectionNumber in sectionOrder)
            {
                var (templateHtml, dataJson) = GetSectionInputs(contentPage, sectionNumber);
                
                // Only render section if template is not empty
                if (!string.IsNullOrWhiteSpace(templateHtml))
                {
                    var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
                    
                    if (!string.IsNullOrWhiteSpace(renderedHtml))
                    {
                        if (wrapInDiv)
                        {
                            // Wrap in div with section metadata
                            sb.AppendLine($"<div class=\"section section-{sectionNumber}\" data-section=\"{sectionNumber}\">");
                            sb.AppendLine(renderedHtml);
                            sb.AppendLine("</div>");
                        }
                        else
                        {
                            sb.AppendLine(renderedHtml);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render specific sections only
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionNumbers">Section numbers to render (1-5)</param>
        /// <returns>Combined HTML string</returns>
        public static string RenderSections(this Model.ContentPage contentPage, params int[] sectionNumbers)
        {
            if (contentPage == null || sectionNumbers == null || sectionNumbers.Length == 0)
                return string.Empty;

            return RenderAllSections(contentPage, sectionNumbers.ToList(), wrapInDiv: true);
        }

        /// <summary>
        /// Render a single section (without wrapper div)
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionNumber">Section number (1-5)</param>
        /// <param name="wrapInDiv">Whether to wrap in div. Default: false for single section</param>
        /// <returns>Rendered HTML</returns>
        public static string RenderSection(
            this Model.ContentPage contentPage, 
            int sectionNumber, 
            bool wrapInDiv = false)
        {
            if (contentPage == null || sectionNumber < 1 || sectionNumber > 5)
                return string.Empty;

            var (templateHtml, dataJson) = GetSectionInputs(contentPage, sectionNumber);
            return GetHtmlSection(templateHtml, dataJson, wrapInDiv, sectionNumber);
        }

        /// <summary>
        /// Get template HTML and data JSON for a specific section
        /// </summary>
        private static (string templateHtml, string dataJson) GetSectionInputs(Model.ContentPage contentPage, int sectionNumber)
        {
            return sectionNumber switch
            {
                1 => (contentPage.Input1, contentPage.Input2),
                2 => (contentPage.Input3, contentPage.Input4),
                3 => (contentPage.Input5, contentPage.Input6),
                4 => (contentPage.Input7, contentPage.Input8),
                5 => (contentPage.Input9, contentPage.Input10),
                _ => (null, null)
            };
        }

        /// <summary>
        /// Check if a section has content
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionNumber">Section number (1-5)</param>
        /// <returns>True if section has template HTML</returns>
        public static bool HasSection(this Model.ContentPage contentPage, int sectionNumber)
        {
            if (contentPage == null || sectionNumber < 1 || sectionNumber > 5)
                return false;

            var (templateHtml, _) = GetSectionInputs(contentPage, sectionNumber);
            return !string.IsNullOrWhiteSpace(templateHtml);
        }

        /// <summary>
        /// Get list of section numbers that have content
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <returns>List of section numbers (1-5)</returns>
        public static List<int> GetActiveSections(this Model.ContentPage contentPage)
        {
            if (contentPage == null)
                return new List<int>();

            var activeSections = new List<int>();
            for (int i = 1; i <= 5; i++)
            {
                if (HasSection(contentPage, i))
                {
                    activeSections.Add(i);
                }
            }

            return activeSections;
        }

        /// <summary>
        /// Render with custom wrapper
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionOrder">Section order</param>
        /// <param name="wrapperFormat">Wrapper format with {0} for section number and {1} for content. Example: "&lt;section class='section-{0}'&gt;{1}&lt;/section&gt;"</param>
        /// <returns>Combined HTML string</returns>
        public static string RenderWithCustomWrapper(
            this Model.ContentPage contentPage, 
            List<int> sectionOrder, 
            string wrapperFormat)
        {
            if (contentPage == null || string.IsNullOrWhiteSpace(wrapperFormat))
                return string.Empty;

            sectionOrder ??= new List<int> { 1, 2, 3, 4, 5 };

            var sb = new StringBuilder();

            foreach (var sectionNumber in sectionOrder)
            {
                var (templateHtml, dataJson) = GetSectionInputs(contentPage, sectionNumber);
                
                if (!string.IsNullOrWhiteSpace(templateHtml))
                {
                    var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
                    
                    if (!string.IsNullOrWhiteSpace(renderedHtml))
                    {
                        // Apply custom wrapper
                        var wrapped = string.Format(wrapperFormat, sectionNumber, renderedHtml);
                        sb.AppendLine(wrapped);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render all sections and minify HTML
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionOrder">Optional: Custom section order</param>
        /// <param name="wrapInDiv">Whether to wrap each section in div</param>
        /// <returns>Minified HTML string</returns>
        public static string RenderAllSectionsMinified(
            this Model.ContentPage contentPage, 
            List<int> sectionOrder = null, 
            bool wrapInDiv = true)
        {
            var html = RenderAllSections(contentPage, sectionOrder, wrapInDiv);
            return SectionTemplateHelper.MinifyHtml(html);
        }

        /// <summary>
        /// Validate all section JSON data
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <returns>Dictionary of section number and validation result</returns>
        public static Dictionary<int, bool> ValidateSectionJsons(this Model.ContentPage contentPage)
        {
            var results = new Dictionary<int, bool>();

            if (contentPage == null)
                return results;

            for (int i = 1; i <= 5; i++)
            {
                var (_, dataJson) = GetSectionInputs(contentPage, i);
                results[i] = string.IsNullOrWhiteSpace(dataJson) || SectionTemplateHelper.IsValidJson(dataJson);
            }

            return results;
        }

        /// <summary>
        /// Render sections with custom CSS classes
        /// </summary>
        /// <param name="contentPage">ContentPage instance</param>
        /// <param name="sectionOrder">Section order</param>
        /// <param name="cssClasses">Dictionary of section number to CSS classes</param>
        /// <returns>Combined HTML string</returns>
        public static string RenderWithCssClasses(
            this Model.ContentPage contentPage, 
            List<int> sectionOrder, 
            Dictionary<int, string> cssClasses = null)
        {
            if (contentPage == null)
                return string.Empty;

            sectionOrder ??= new List<int> { 1, 2, 3, 4, 5 };
            cssClasses ??= new Dictionary<int, string>();

            var sb = new StringBuilder();

            foreach (var sectionNumber in sectionOrder)
            {
                var (templateHtml, dataJson) = GetSectionInputs(contentPage, sectionNumber);
                
                if (!string.IsNullOrWhiteSpace(templateHtml))
                {
                    var renderedHtml = SectionTemplateHelper.RenderSection(templateHtml, dataJson);
                    
                    if (!string.IsNullOrWhiteSpace(renderedHtml))
                    {
                        // Get custom CSS classes or use default
                        var classes = cssClasses.ContainsKey(sectionNumber) 
                            ? $"section section-{sectionNumber} {cssClasses[sectionNumber]}"
                            : $"section section-{sectionNumber}";

                        sb.AppendLine($"<div class=\"{classes}\" data-section=\"{sectionNumber}\">");
                        sb.AppendLine(renderedHtml);
                        sb.AppendLine("</div>");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
