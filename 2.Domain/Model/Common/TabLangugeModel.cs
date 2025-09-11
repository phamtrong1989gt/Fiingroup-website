using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class TabLanguageModel
    {
        public string TagFroup { get; set; } = "[tab-languge-grid]";
        public string Language { get; set; } = "vi";
        public string LinkDefault { get; set; } = "#language";
        public string FuntionName { get; set; } = "reGetDataGrid(this)";
    }
}
