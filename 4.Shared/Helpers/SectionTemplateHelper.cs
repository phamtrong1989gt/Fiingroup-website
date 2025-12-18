using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PT.Shared.Helpers
{
    /// <summary>
    /// Helper class for rendering section templates with JSON data
    /// </summary>
    public static class SectionTemplateHelper
    {
        /// <summary>
        /// Section configuration
        /// </summary>
        public class SectionConfig
        {
            public string TemplateHtml { get; set; }
            public string DataJson { get; set; }
            public int Order { get; set; }
            public bool IsEnabled { get; set; } = true;
        }

        /// <summary>
        /// Render all sections into a single HTML string
        /// </summary>
        /// <param name="sections">List of section configurations</param>
        /// <returns>Combined HTML string</returns>
        public static string RenderSections(List<SectionConfig> sections)
        {
            if (sections == null || !sections.Any())
                return string.Empty;

            var sb = new StringBuilder();
            
            // Sort by Order and process enabled sections only
            var enabledSections = sections
                .Where(s => s.IsEnabled && !string.IsNullOrWhiteSpace(s.TemplateHtml))
                .OrderBy(s => s.Order)
                .ToList();

            foreach (var section in enabledSections)
            {
                try
                {
                    var rendered = RenderSection(section.TemplateHtml, section.DataJson);
                    if (!string.IsNullOrWhiteSpace(rendered))
                    {
                        sb.AppendLine(rendered);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other sections
                    Console.WriteLine($"Error rendering section at order {section.Order}: {ex.Message}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render a single section template with JSON data
        /// </summary>
        /// <param name="templateHtml">Template HTML with placeholders like [fieldName] and [For]...[/For]</param>
        /// <param name="dataJson">JSON data string</param>
        /// <returns>Rendered HTML</returns>
        public static string RenderSection(string templateHtml, string dataJson)
        {
            if (string.IsNullOrWhiteSpace(templateHtml))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(dataJson))
                return templateHtml;

            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(dataJson);
                return RenderTemplate(templateHtml, data);
            }
            catch (JsonException ex)
            {
                // Invalid JSON, return template as-is
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                return templateHtml;
            }
        }

        /// <summary>
        /// Render template with JObject data
        /// </summary>
        private static string RenderTemplate(string template, JObject data)
        {
            if (data == null)
                return template;

            // Step 1: Process [For] loops (including custom named loops)
            template = ProcessForLoops(template, data);

            // Step 2: Replace simple placeholders [fieldName]
            template = ReplacePlaceholders(template, data);

            return template;
        }

        /// <summary>
        /// Process [For]...[/For] loops with support for custom names
        /// Supports: [For]...[/For], [ForDesktop]...[/ForDesktop], [ForMobile]...[/ForMobile], etc.
        /// </summary>
        private static string ProcessForLoops(string template, JObject data)
        {
            // Pattern to match [For...] or [ForXXX] with any alphanumeric name
            // Matches: [For], [ForDesktop], [ForMobile], [ForDesktopIndicator], etc.
            var forPattern = @"\[(For[^\]]*)\](.*?)\[/\1\]";
            var regex = new Regex(forPattern, RegexOptions.Singleline);

            // Keep processing until no more For loops found (to handle nested loops)
            int maxIterations = 10; // Prevent infinite loop
            int iteration = 0;
            
            while (regex.IsMatch(template) && iteration < maxIterations)
            {
                template = regex.Replace(template, match =>
                {
                    var forName = match.Groups[1].Value; // e.g., "For", "ForDesktop", "ForMobile"
                    var loopTemplate = match.Groups[2].Value;

                    // Try to get array data from JSON
                    // First try exact match (e.g., "ForDesktop")
                    // Then try without "For" prefix (e.g., "Desktop")
                    JArray array = null;
                    
                    if (data.TryGetValue(forName, out var token) && token is JArray)
                    {
                        array = token as JArray;
                    }
                    else if (forName.StartsWith("For", StringComparison.OrdinalIgnoreCase) && forName.Length > 3)
                    {
                        // Try without "For" prefix
                        var nameWithoutFor = forName.Substring(3);
                        if (data.TryGetValue(nameWithoutFor, out token) && token is JArray)
                        {
                            array = token as JArray;
                        }
                    }

                    if (array == null || array.Count == 0)
                    {
                        // No data found or empty array
                        return string.Empty;
                    }

                    // Process array items
                    var sb = new StringBuilder();
                    foreach (var item in array)
                    {
                        if (item is JObject itemObj)
                        {
                            // Replace placeholders in loop template with item data
                            var rendered = ReplacePlaceholders(loopTemplate, itemObj);
                            sb.Append(rendered);
                        }
                    }
                    
                    return sb.ToString();
                });
                
                iteration++;
            }

            return template;
        }

        /// <summary>
        /// Replace simple placeholders [fieldName] with values from data
        /// </summary>
        private static string ReplacePlaceholders(string template, JObject data)
        {
            // Pattern to match [fieldName] but NOT [For...] or [/For...]
            var placeholderPattern = @"\[([^\[\]\/]+)\]";
            var regex = new Regex(placeholderPattern);

            return regex.Replace(template, match =>
            {
                var fieldName = match.Groups[1].Value.Trim();

                // Skip if it's a For loop marker
                if (fieldName.StartsWith("For", StringComparison.OrdinalIgnoreCase))
                {
                    return match.Value;
                }

                // Get value from data
                if (data.TryGetValue(fieldName, out var token))
                {
                    // Handle different token types
                    switch (token.Type)
                    {
                        case JTokenType.String:
                            return token.ToString();
                        case JTokenType.Integer:
                        case JTokenType.Float:
                            return token.ToString();
                        case JTokenType.Boolean:
                            return token.ToString().ToLower();
                        case JTokenType.Null:
                            return string.Empty;
                        case JTokenType.Array:
                        case JTokenType.Object:
                            // Don't serialize complex objects in placeholders
                            return match.Value;
                        default:
                            return token.ToString();
                    }
                }

                // Field not found, return empty or keep placeholder for debugging
                // Change to match.Value to keep placeholder for debugging
                return string.Empty;
            });
        }

        /// <summary>
        /// Helper method to create section config from Input fields
        /// </summary>
        /// <param name="order">Display order</param>
        /// <param name="templateHtml">Template HTML (e.g., Input1, Input3, Input5)</param>
        /// <param name="dataJson">Data JSON (e.g., Input2, Input4, Input6)</param>
        /// <param name="isEnabled">Whether to render this section</param>
        /// <returns>SectionConfig object</returns>
        public static SectionConfig CreateSection(int order, string templateHtml, string dataJson, bool isEnabled = true)
        {
            return new SectionConfig
            {
                Order = order,
                TemplateHtml = templateHtml,
                DataJson = dataJson,
                IsEnabled = isEnabled
            };
        }

        /// <summary>
        /// Quick method to render from pairs of Input fields
        /// </summary>
        /// <param name="inputs">Pairs of (templateHtml, dataJson)</param>
        /// <returns>Combined HTML</returns>
        public static string RenderFromInputs(params (string templateHtml, string dataJson)[] inputs)
        {
            if (inputs == null || inputs.Length == 0)
                return string.Empty;

            var sections = inputs
                .Select((input, index) => CreateSection(index + 1, input.templateHtml, input.dataJson))
                .ToList();

            return RenderSections(sections);
        }

        /// <summary>
        /// Minify HTML (remove extra whitespace)
        /// </summary>
        public static string MinifyHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove comments
            html = Regex.Replace(html, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

            // Replace multiple whitespace with single space
            html = Regex.Replace(html, @"\s+", " ");

            // Remove whitespace between tags
            html = Regex.Replace(html, @">\s+<", "><");

            return html.Trim();
        }

        /// <summary>
        /// Validate JSON structure
        /// </summary>
        public static bool IsValidJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return false;

            try
            {
                JToken.Parse(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
