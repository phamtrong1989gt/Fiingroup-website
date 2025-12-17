namespace PT.Domain.Model
{
    public class SectionTemplateBuilderModel
    {
        public int SectionNumber { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; } = "view_carousel";
        public bool IsExpanded { get; set; } = false;
        
        // Input names (asp-for)
        public string InputTemplate { get; set; }  // e.g., "Input1"
        public string InputValue { get; set; }     // e.g., "Input2"
        
        // Labels
        public string TemplateLabel { get; set; } = "Template HTML";
        public string ValueLabel { get; set; } = "Values (JSON)";
        
        // Current values (for edit mode)
        public string TemplateValue { get; set; }
        public string ValueJson { get; set; }
        
        // Template Source - NEW!
        /// <summary>
        /// Tên bi?n global JavaScript ch?a templates (e.g., "SectionTemplates1", "SectionTemplates2")
        /// M?c ??nh: "SectionTemplates1"
        /// </summary>
        public string TemplateSource { get; set; } = "SectionTemplates1";
        
        // Custom button names - NEW!
        /// <summary>
        /// Tên button thêm field. M?c ??nh: "btnAddField{SectionNumber}"
        /// </summary>
        public string ButtonAddField { get; set; }
        
        /// <summary>
        /// Tên button thêm item. M?c ??nh: "btnAddItem{SectionNumber}"
        /// </summary>
        public string ButtonAddItem { get; set; }
        
        // Generated IDs (t? ??ng t?o t? SectionNumber ho?c custom)
        public string SectionId => $"headingSection_{SectionNumber}";
        public string CollapseId => $"collapseSection_{SectionNumber}";
        public string TemplateInputId => $"templateInput{SectionNumber}";
        public string ValueInputId => $"valuesInput{SectionNumber}";
        public string AddFieldBtnId => !string.IsNullOrEmpty(ButtonAddField) ? ButtonAddField : $"btnAddField{SectionNumber}";
        public string AddItemBtnId => !string.IsNullOrEmpty(ButtonAddItem) ? ButtonAddItem : $"btnAddItem{SectionNumber}";
        public string FieldNameInputId => $"quickFieldName{SectionNumber}";
    }
}
