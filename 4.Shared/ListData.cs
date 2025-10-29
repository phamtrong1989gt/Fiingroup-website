using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace PT.Shared
{
    public class ListData
    {
        public static string GetNameLanguage(string lan)
        {
            return ListLanguage.FirstOrDefault(x => x.Id == lan)?.Name ?? "";
        }

        public static List<LanguageModel> AllLanguage
        {
            get
            {
                return new List<LanguageModel>()
                {

                    new LanguageModel()
                    {
                        Id = "en",
                        Id2="en-US",
                        Name = "Tiếng Anh",
                        Name2 = "English",
                        Icon = "/Content/Admin/images/en.png",
                        Icon2 = "/Content/Admin/images/en.png"
                    },
                    new LanguageModel()
                    {
                        Id = "vi",
                        Id2 = "vi-VN",
                        Name = "Tiếng Việt",
                        Name2 = "Tiếng Việt",
                        Icon = "/Content/Admin/images/vi.png",
                        Icon2 = "/Content/Admin/images/vi.png"
                    }
                };
            }
        }

        public static List<LanguageModel> ListLanguage
        {
            get
            {
                return AllLanguage;
            }
        }
    }
    public class LanguageModel
    {
        public string Id { get; set; }
        public string Id2 { get; set; }

        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Icon { get; set; }
        public string Icon2 { get; set; }

    }
}
