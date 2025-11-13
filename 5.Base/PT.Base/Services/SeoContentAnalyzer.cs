using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace PT.Base.Services
{
    /// <summary>
    /// Phân tích và đánh giá nội dung theo tiêu chuẩn SEO 2024-2025
    /// </summary>
    public class SeoContentAnalyzer
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static SeoContentAnalyzer()
        {
            // Set User-Agent để tránh bị block bởi một số website
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// Lấy toàn bộ nội dung HTML từ một URL
        /// Sử dụng để phân tích SEO cho trang web thực tế
        /// </summary>
        /// <param name="url">Đường dẫn URL của trang web cần phân tích</param>
        /// <returns>Nội dung HTML đầy đủ của trang</returns>
        public async Task<string> FetchWebContentAsync(string url)
        {
            try
            {
                // Kiểm tra URL hợp lệ
                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new ArgumentException("URL không được để trống", nameof(url));
                }

                // Đảm bảo URL có protocol
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                // Validate URL format
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
                {
                    throw new ArgumentException("URL không đúng định dạng", nameof(url));
                }

                // Gửi request và lấy response
                var response = await _httpClient.GetAsync(url);

                // Kiểm tra status code
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Không thể tải nội dung từ URL. Status code: {response.StatusCode}");
                }

                // Đọc nội dung HTML
                var htmlContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    throw new InvalidOperationException("Nội dung HTML trả về rỗng");
                }

                return htmlContent;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Lỗi kết nối đến URL: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Timeout khi tải nội dung từ URL. Vui lòng thử lại.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi không xác định khi lấy nội dung: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Phân tích SEO trực tiếp từ URL
        /// Kết hợp fetch content và analyze trong một method tiện lợi
        /// </summary>
        /// <param name="url">URL của trang web cần phân tích</param>
        /// <param name="focusKeyword">Từ khóa chính cần kiểm tra (tùy chọn)</param>
        /// <returns>Kết quả phân tích SEO chi tiết</returns>
        public async Task<SeoAnalysisResult> AnalyzeFromUrlAsync(string url, string focusKeyword = null)
        {
            try
            {
                // Lấy nội dung HTML từ URL
                var htmlContent = await FetchWebContentAsync(url);

                // Phân tích SEO
                var result = Analyze(htmlContent, focusKeyword);

                // Thêm thông tin URL vào kết quả
                result.AnalyzedUrl = url;

                return result;
            }
            catch (Exception ex)
            {
                var result = new SeoAnalysisResult
                {
                    AnalyzedUrl = url
                };
                result.Errors.Add($"Không thể phân tích URL: {ex.Message}");
                return result;
            }
        }

        public SeoAnalysisResult Analyze(string htmlContent, string focusKeyword = null)
        {
            var result = new SeoAnalysisResult();

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                result.Errors.Add("Nội dung trống");
                return result;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // 1. Kiểm tra thẻ H1
            CheckH1Tags(doc, result);

            // 2. Kiểm tra thẻ H2, H3
            CheckHeadingStructure(doc, result);

            // 3. Kiểm tra hình ảnh có alt text
            CheckImageAltText(doc, result);

            // 4. Kiểm tra độ dài nội dung
            CheckContentLength(doc, result);

            // 5. Kiểm tra internal links
            CheckLinks(doc, result);

            // 6. Kiểm tra focus keyword nếu có
            if (!string.IsNullOrWhiteSpace(focusKeyword))
            {
                CheckFocusKeyword(doc, focusKeyword, result);
            }

            // 7. Kiểm tra mật độ từ khóa
            CheckKeywordDensity(doc, result);

            // 8. Kiểm tra paragraphs quá dài
            CheckParagraphLength(doc, result);

            // 9. Kiểm tra Core Web Vitals - Structured Data (2024)
            CheckStructuredData(doc, result);

            // 10. Kiểm tra Mobile-Friendly - Responsive Images (2024)
            CheckResponsiveImages(doc, result);

            // 11. Kiểm tra E-E-A-T: Experience, Expertise, Authoritativeness, Trustworthiness (2024)
            CheckEEAT(doc, result);

            // 12. Kiểm tra User Intent & Search Intent Optimization (2024-2025)
            CheckContentDepth(doc, result);

            // 13. Kiểm tra Video Content & Multimedia (2024-2025)
            CheckMultimedia(doc, result);

            // 14. Kiểm tra Semantic SEO & LSI Keywords (2024-2025)
            CheckSemanticContent(doc, focusKeyword, result);

            // 15. Kiểm tra Readability Score - Flesch Reading Ease (2024)
            CheckReadability(doc, result);

            // 16. Kiểm tra External Links Authority (2024)
            CheckExternalLinksQuality(doc, result);

            // 17. Kiểm tra Table of Contents - TOC (2024-2025)
            CheckTableOfContents(doc, result);

            // 18. Kiểm tra FAQ Schema (2024-2025)
            CheckFAQSection(doc, result);

            // Tính điểm tổng thể
            result.OverallScore = CalculateScore(result);

            return result;
        }

        /// <summary>
        /// Tiêu chuẩn 2024: H1 phải duy nhất, độ dài 20-70 ký tự, chứa keyword chính
        /// Google ưu tiên H1 rõ ràng, súc tích và phản ánh chính xác nội dung
        /// </summary>
        private void CheckH1Tags(HtmlDocument doc, SeoAnalysisResult result)
        {
            var h1Tags = doc.DocumentNode.SelectNodes("//h1");

            if (h1Tags == null || h1Tags.Count == 0)
            {
                result.Errors.Add("Thiếu thẻ H1 - Cần có ít nhất 1 thẻ H1 trong nội dung");
                result.H1Count = 0;
            }
            else if (h1Tags.Count > 1)
            {
                result.Warnings.Add($"Có {h1Tags.Count} thẻ H1 - Nên chỉ có 1 thẻ H1 duy nhất");
                result.H1Count = h1Tags.Count;
            }
            else
            {
                result.Recommendations.Add("✓ Có đúng 1 thẻ H1");
                result.H1Count = 1;

                // Kiểm tra độ dài H1
                var h1Text = h1Tags[0].InnerText.Trim();
                if (h1Text.Length < 20)
                {
                    result.Warnings.Add($"Thẻ H1 quá ngắn ({h1Text.Length} ký tự) - Nên từ 20-70 ký tự");
                }
                else if (h1Text.Length > 70)
                {
                    result.Warnings.Add($"Thẻ H1 quá dài ({h1Text.Length} ký tự) - Nên từ 20-70 ký tự");
                }
                else
                {
                    result.Recommendations.Add($"✓ Độ dài H1 phù hợp ({h1Text.Length} ký tự)");
                }
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Cấu trúc heading phải tuần tự (H1 > H2 > H3...)
        /// 2-5 H2 tags là tối ưu, giúp Google hiểu rõ cấu trúc nội dung
        /// </summary>
        private void CheckHeadingStructure(HtmlDocument doc, SeoAnalysisResult result)
        {
            var h2Tags = doc.DocumentNode.SelectNodes("//h2");
            var h3Tags = doc.DocumentNode.SelectNodes("//h3");
            var h4Tags = doc.DocumentNode.SelectNodes("//h4");

            result.H2Count = h2Tags?.Count ?? 0;
            result.H3Count = h3Tags?.Count ?? 0;

            if (result.H2Count == 0)
            {
                result.Warnings.Add("Không có thẻ H2 - Nên có ít nhất 2-5 thẻ H2 để cấu trúc nội dung");
            }
            else if (result.H2Count > 10)
            {
                result.Warnings.Add($"Có quá nhiều thẻ H2 ({result.H2Count}) - Nên giới hạn từ 2-8 thẻ H2");
            }
            else
            {
                result.Recommendations.Add($"✓ Có {result.H2Count} thẻ H2 - Cấu trúc hợp lý");
            }

            if (result.H3Count > 20)
            {
                result.Warnings.Add($"Có quá nhiều thẻ H3 ({result.H3Count}) - Cân nhắc giảm bớt");
            }

            // Kiểm tra thứ tự heading
            var allHeadings = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
            if (allHeadings != null)
            {
                int prevLevel = 0;
                foreach (var heading in allHeadings)
                {
                    int currentLevel = int.Parse(heading.Name.Substring(1));
                    if (currentLevel - prevLevel > 1)
                    {
                        result.Warnings.Add($"Cấu trúc heading không tuần tự - Bỏ qua cấp từ H{prevLevel} đến H{currentLevel}");
                        break;
                    }
                    prevLevel = currentLevel;
                }
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: 100% hình ảnh phải có alt text mô tả, hỗ trợ accessibility
        /// Alt text nên chứa keyword tự nhiên, độ dài 80-125 ký tự
        /// Google Image Search ngày càng quan trọng cho traffic
        /// </summary>
        private void CheckImageAltText(HtmlDocument doc, SeoAnalysisResult result)
        {
            var images = doc.DocumentNode.SelectNodes("//img");

            if (images == null || images.Count == 0)
            {
                result.Warnings.Add("Không có hình ảnh - Nên thêm hình ảnh minh họa (tối thiểu 1-3 ảnh)");
                return;
            }

            result.ImageCount = images.Count;
            int imagesWithoutAlt = 0;
            int imagesWithEmptyAlt = 0;
            int imagesWithShortAlt = 0;

            foreach (var img in images)
            {
                var altAttr = img.GetAttributeValue("alt", null);
                if (altAttr == null)
                {
                    imagesWithoutAlt++;
                }
                else if (string.IsNullOrWhiteSpace(altAttr))
                {
                    imagesWithEmptyAlt++;
                }
                else if (altAttr.Length < 10)
                {
                    imagesWithShortAlt++;
                }
            }

            if (imagesWithoutAlt > 0)
            {
                result.Errors.Add($"{imagesWithoutAlt}/{result.ImageCount} hình ảnh thiếu thuộc tính alt - Cần thêm alt text cho tất cả hình ảnh");
            }
            else if (imagesWithEmptyAlt > 0)
            {
                result.Warnings.Add($"{imagesWithEmptyAlt}/{result.ImageCount} hình ảnh có alt trống - Nên mô tả nội dung hình ảnh");
            }
            else if (imagesWithShortAlt > 0)
            {
                result.Warnings.Add($"{imagesWithShortAlt}/{result.ImageCount} hình ảnh có alt text quá ngắn - Nên mô tả chi tiết hơn (80-125 ký tự)");
            }
            else
            {
                result.Recommendations.Add($"✓ Tất cả {result.ImageCount} hình ảnh đều có alt text phù hợp");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Nội dung dài (1500-2500 từ) được xếp hạng tốt hơn
        /// Long-form content (2000+ từ) chiếm ưu thế trong top 10 Google
        /// Nội dung phải có giá trị, không chỉ dài mà còn phải chất lượng
        /// </summary>
        private void CheckContentLength(HtmlDocument doc, SeoAnalysisResult result)
        {
            var textContent = doc.DocumentNode.InnerText;
            var words = Regex.Split(textContent, @"\s+").Where(w => !string.IsNullOrWhiteSpace(w)).ToList();
            result.WordCount = words.Count;

            if (result.WordCount < 300)
            {
                result.Errors.Add($"Nội dung quá ngắn ({result.WordCount} từ) - Google ưu tiên nội dung tối thiểu 800-1000 từ");
            }
            else if (result.WordCount < 800)
            {
                result.Warnings.Add($"Nội dung ngắn ({result.WordCount} từ) - Nên có ít nhất 800-1500 từ để cạnh tranh tốt");
            }
            else if (result.WordCount >= 1500 && result.WordCount <= 2500)
            {
                result.Recommendations.Add($"✓ Nội dung dài tối ưu ({result.WordCount} từ) - Phù hợp với tiêu chuẩn 2024");
            }
            else if (result.WordCount > 2500)
            {
                result.Recommendations.Add($"✓ Nội dung rất dài ({result.WordCount} từ) - Xuất sắc cho SEO, đảm bảo chất lượng");
            }
            else
            {
                result.Recommendations.Add($"✓ Độ dài nội dung khá ({result.WordCount} từ) - Có thể mở rộng thêm");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Internal linking giúp phân bổ Page Authority
        /// 3-5 internal links là tối ưu, sử dụng anchor text descriptive
        /// External links đến nguồn uy tín (authority sites) tăng độ tin cậy
        /// </summary>
        private void CheckLinks(HtmlDocument doc, SeoAnalysisResult result)
        {
            var links = doc.DocumentNode.SelectNodes("//a[@href]");

            if (links == null || links.Count == 0)
            {
                result.Warnings.Add("Không có link nào - Nên thêm 3-5 internal links và 1-2 external links đến nguồn uy tín");
                result.InternalLinksCount = 0;
                return;
            }

            var internalLinks = links.Where(l =>
            {
                var href = l.GetAttributeValue("href", "");
                return !href.StartsWith("http://") && !href.StartsWith("https://") && !string.IsNullOrWhiteSpace(href);
            }).ToList();

            result.InternalLinksCount = internalLinks.Count;
            result.ExternalLinksCount = links.Count - internalLinks.Count;

            if (result.InternalLinksCount == 0)
            {
                result.Errors.Add("Không có internal link - Cần có ít nhất 3-5 internal links để tăng Page Authority");
            }
            else if (result.InternalLinksCount < 3)
            {
                result.Warnings.Add($"Có {result.InternalLinksCount} internal link - Nên có 3-5 internal links");
            }
            else if (result.InternalLinksCount > 15)
            {
                result.Warnings.Add($"Có quá nhiều internal link ({result.InternalLinksCount}) - Có thể làm giảm giá trị link juice");
            }
            else
            {
                result.Recommendations.Add($"✓ Có {result.InternalLinksCount} internal links - Tối ưu");
            }

            if (result.ExternalLinksCount == 0)
            {
                result.Warnings.Add("Không có external link - Nên thêm 1-2 links đến nguồn uy tín (.gov, .edu, authority sites)");
            }
            else if (result.ExternalLinksCount >= 1 && result.ExternalLinksCount <= 3)
            {
                result.Recommendations.Add($"✓ Có {result.ExternalLinksCount} external links - Phù hợp");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Focus keyword nên xuất hiện tự nhiên
        /// Keyword density: 0.5-2% (không quá 3% để tránh keyword stuffing)
        /// Keyword phải có trong H1, H2 đầu tiên, 100 từ đầu, và đoạn kết
        /// </summary>
        private void CheckFocusKeyword(HtmlDocument doc, string keyword, SeoAnalysisResult result)
        {
            keyword = keyword.ToLower().Trim();
            var content = doc.DocumentNode.InnerText.ToLower();

            // Kiểm tra trong H1
            var h1 = doc.DocumentNode.SelectSingleNode("//h1");
            if (h1 != null && h1.InnerText.ToLower().Contains(keyword))
            {
                result.Recommendations.Add($"✓ Focus keyword xuất hiện trong H1");
            }
            else
            {
                result.Errors.Add($"Focus keyword '{keyword}' không xuất hiện trong H1 - Bắt buộc phải có");
            }

            // Kiểm tra trong H2 đầu tiên
            var firstH2 = doc.DocumentNode.SelectSingleNode("//h2");
            if (firstH2 != null && firstH2.InnerText.ToLower().Contains(keyword))
            {
                result.Recommendations.Add($"✓ Focus keyword xuất hiện trong H2 đầu tiên");
            }
            else
            {
                result.Warnings.Add($"Focus keyword '{keyword}' nên xuất hiện trong H2 đầu tiên");
            }

            // Kiểm tra trong 100 từ đầu
            var words = Regex.Split(content, @"\s+").Take(100).ToList();
            var first100Words = string.Join(" ", words);
            if (first100Words.Contains(keyword))
            {
                result.Recommendations.Add($"✓ Focus keyword xuất hiện trong 100 từ đầu");
            }
            else
            {
                result.Warnings.Add($"Focus keyword '{keyword}' nên xuất hiện trong 100 từ đầu tiên");
            }

            // Kiểm tra trong đoạn cuối
            var lastWords = Regex.Split(content, @"\s+").TakeLast(100).ToList();
            var last100Words = string.Join(" ", lastWords);
            if (last100Words.Contains(keyword))
            {
                result.Recommendations.Add($"✓ Focus keyword xuất hiện trong đoạn kết");
            }

            // Đếm số lần xuất hiện và tính mật độ
            int keywordCount = Regex.Matches(content, Regex.Escape(keyword)).Count;
            result.FocusKeywordCount = keywordCount;

            if (result.WordCount > 0)
            {
                double density = (double)keywordCount / result.WordCount * 100;
                result.KeywordDensity = Math.Round(density, 2);

                if (keywordCount == 0)
                {
                    result.Errors.Add($"Focus keyword '{keyword}' không xuất hiện trong nội dung");
                }
                else if (density < 0.5)
                {
                    result.Warnings.Add($"Mật độ keyword thấp ({density:F2}%) - Nên đạt 0.5-2%");
                }
                else if (density > 3)
                {
                    result.Errors.Add($"Mật độ keyword quá cao ({density:F2}%) - Nguy cơ keyword stuffing, giảm xuống dưới 2%");
                }
                else if (density >= 0.5 && density <= 2)
                {
                    result.Recommendations.Add($"✓ Mật độ keyword tối ưu ({density:F2}%) - {keywordCount} lần xuất hiện");
                }
                else
                {
                    result.Warnings.Add($"Mật độ keyword hơi cao ({density:F2}%) - Nên giữ trong khoảng 0.5-2%");
                }
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Keyword density phải tự nhiên, từ 0.5-2%
        /// Tránh keyword stuffing (>3%) sẽ bị Google phạt
        /// </summary>
        private void CheckKeywordDensity(HtmlDocument doc, SeoAnalysisResult result)
        {
            // Logic đã được tích hợp vào CheckFocusKeyword
            // Giữ lại method này để tương thích
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Đoạn văn nên ngắn 50-150 từ
        /// Paragraphs dài >150 từ khó đọc, ảnh hưởng User Experience và Dwell Time
        /// </summary>
        private void CheckParagraphLength(HtmlDocument doc, SeoAnalysisResult result)
        {
            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs == null) return;

            int longParagraphs = 0;
            int shortParagraphs = 0;

            foreach (var p in paragraphs)
            {
                var words = Regex.Split(p.InnerText, @"\s+").Where(w => !string.IsNullOrWhiteSpace(w)).Count();
                if (words > 150)
                {
                    longParagraphs++;
                }
                else if (words < 20 && words > 0)
                {
                    shortParagraphs++;
                }
            }

            if (longParagraphs > 0)
            {
                result.Warnings.Add($"{longParagraphs} đoạn văn quá dài (>150 từ) - Nên chia nhỏ thành 50-150 từ để tăng readability");
            }

            if (shortParagraphs > paragraphs.Count / 2)
            {
                result.Warnings.Add($"Có nhiều đoạn văn quá ngắn - Cân nhắc gộp lại để tạo nội dung mạch lạc hơn");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Structured Data (Schema Markup) là bắt buộc
        /// JSON-LD cho Article, BreadcrumbList, Organization giúp hiển thị Rich Snippets
        /// Tăng CTR từ 20-30% khi có Rich Results trên Google
        /// </summary>
        private void CheckStructuredData(HtmlDocument doc, SeoAnalysisResult result)
        {
            var jsonLdScripts = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");

            if (jsonLdScripts == null || jsonLdScripts.Count == 0)
            {
                result.Warnings.Add("Không có Schema Markup (JSON-LD) - Nên thêm Article Schema, BreadcrumbList để hiển thị Rich Snippets");
                return;
            }

            bool hasArticleSchema = false;
            bool hasBreadcrumbSchema = false;
            bool hasOrganizationSchema = false;

            foreach (var script in jsonLdScripts)
            {
                var content = script.InnerText.ToLower();
                if (content.Contains("\"@type\":\"article\"") || content.Contains("\"@type\":\"blogposting\""))
                    hasArticleSchema = true;
                if (content.Contains("\"@type\":\"breadcrumblist\""))
                    hasBreadcrumbSchema = true;
                if (content.Contains("\"@type\":\"organization\""))
                    hasOrganizationSchema = true;
            }

            if (hasArticleSchema)
                result.Recommendations.Add("✓ Có Article Schema - Giúp hiển thị Rich Snippets trên Google");
            else
                result.Warnings.Add("Thiếu Article Schema - Nên thêm để tối ưu hiển thị trên Google Search");

            if (!hasBreadcrumbSchema)
                result.Recommendations.Add("Nên thêm BreadcrumbList Schema để hiển thị breadcrumb navigation");

            result.HasStructuredData = hasArticleSchema || hasBreadcrumbSchema;
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Mobile-First Indexing - hình ảnh phải responsive
        /// Sử dụng srcset, loading="lazy", width/height attributes
        /// WebP format ưu tiên hơn JPEG/PNG (giảm 25-35% dung lượng)
        /// </summary>
        private void CheckResponsiveImages(HtmlDocument doc, SeoAnalysisResult result)
        {
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images == null || images.Count == 0) return;

            int imagesWithSrcset = 0;
            int imagesWithLazyLoad = 0;
            int imagesWithDimensions = 0;

            foreach (var img in images)
            {
                if (img.GetAttributeValue("srcset", null) != null)
                    imagesWithSrcset++;

                if (img.GetAttributeValue("loading", "") == "lazy")
                    imagesWithLazyLoad++;

                if (img.GetAttributeValue("width", null) != null && img.GetAttributeValue("height", null) != null)
                    imagesWithDimensions++;
            }

            if (imagesWithSrcset == 0)
            {
                result.Warnings.Add("Không có hình ảnh responsive (srcset) - Nên thêm srcset để tối ưu mobile");
            }
            else
            {
                result.Recommendations.Add($"✓ Có {imagesWithSrcset} hình ảnh responsive (srcset)");
            }

            if (imagesWithLazyLoad < images.Count)
            {
                result.Recommendations.Add($"Nên thêm loading='lazy' cho {images.Count - imagesWithLazyLoad} hình ảnh để cải thiện Core Web Vitals");
            }
            else
            {
                result.Recommendations.Add($"✓ Tất cả hình ảnh có lazy loading - Tối ưu tốc độ tải trang");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: E-E-A-T (Experience, Expertise, Authoritativeness, Trustworthiness)
        /// Google Quality Rater Guidelines yêu cầu author info, publish date, update date
        /// Nội dung phải thể hiện kinh nghiệm thực tế, không chỉ lý thuyết
        /// </summary>
        private void CheckEEAT(HtmlDocument doc, SeoAnalysisResult result)
        {
            // Kiểm tra author information
            var authorMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='author']") ??
                            doc.DocumentNode.SelectSingleNode("//meta[@property='article:author']");
            var authorDiv = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'author')]");

            if (authorMeta != null || authorDiv != null)
            {
                result.Recommendations.Add("✓ Có thông tin tác giả - Tăng độ tin cậy (E-E-A-T)");
            }
            else
            {
                result.Warnings.Add("Thiếu thông tin tác giả - Nên thêm author bio để tăng Expertise và Trust");
            }

            // Kiểm tra publish date
            var publishDate = doc.DocumentNode.SelectSingleNode("//meta[@property='article:published_time']") ??
                             doc.DocumentNode.SelectSingleNode("//time[@datetime]");

            if (publishDate != null)
            {
                result.Recommendations.Add("✓ Có ngày xuất bản - Giúp Google đánh giá freshness");
            }
            else
            {
                result.Warnings.Add("Thiếu ngày xuất bản - Nên thêm để Google hiểu content freshness");
            }

            // Kiểm tra modified date
            var modifiedDate = doc.DocumentNode.SelectSingleNode("//meta[@property='article:modified_time']");
            if (modifiedDate != null)
            {
                result.Recommendations.Add("✓ Có ngày cập nhật - Content được maintain thường xuyên");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Content Depth - nội dung phải đi sâu, đầy đủ
        /// Search Intent Optimization: Informational/Transactional/Navigational
        /// Cần có các section: Introduction, Main Points, Examples, Conclusion
        /// </summary>
        private void CheckContentDepth(HtmlDocument doc, SeoAnalysisResult result)
        {
            var h2Tags = doc.DocumentNode.SelectNodes("//h2");

            if (h2Tags != null && h2Tags.Count >= 4)
            {
                result.Recommendations.Add($"✓ Có {h2Tags.Count} sections chính - Nội dung có chiều sâu tốt");
            }
            else if (h2Tags != null && h2Tags.Count >= 2)
            {
                result.Warnings.Add($"Có {h2Tags.Count} sections - Nên mở rộng thêm 2-3 sections để tăng content depth");
            }
            else
            {
                result.Warnings.Add("Nội dung thiếu chiều sâu - Nên chia thành 4-6 sections chính với H2");
            }

            // Kiểm tra có phần kết luận
            var conclusion = doc.DocumentNode.SelectNodes("//h2|//h3")?.Any(h =>
                h.InnerText.ToLower().Contains("kết luận") ||
                h.InnerText.ToLower().Contains("tổng kết") ||
                h.InnerText.ToLower().Contains("conclusion"));

            if (conclusion == true)
            {
                result.Recommendations.Add("✓ Có phần kết luận - Cấu trúc nội dung hoàn chỉnh");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Video Content tăng Engagement và Dwell Time
        /// Video embedded từ YouTube, Vimeo được Google đánh giá cao
        /// Multimedia (infographics, charts) giúp tăng social shares
        /// </summary>
        private void CheckMultimedia(HtmlDocument doc, SeoAnalysisResult result)
        {
            var iframes = doc.DocumentNode.SelectNodes("//iframe");
            var videos = doc.DocumentNode.SelectNodes("//video");

            bool hasVideo = false;

            if (iframes != null)
            {
                foreach (var iframe in iframes)
                {
                    var src = iframe.GetAttributeValue("src", "").ToLower();
                    if (src.Contains("youtube") || src.Contains("vimeo") || src.Contains("video"))
                    {
                        hasVideo = true;
                        break;
                    }
                }
            }

            if (videos != null && videos.Count > 0)
                hasVideo = true;

            if (hasVideo)
            {
                result.Recommendations.Add("✓ Có video content - Tăng engagement và dwell time (SEO 2024-2025)");
            }
            else if (result.WordCount > 1000)
            {
                result.Recommendations.Add("Nên thêm video minh họa để tăng engagement (nội dung dài nên có multimedia)");
            }

            result.HasVideo = hasVideo;
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Semantic SEO - sử dụng LSI Keywords (Latent Semantic Indexing)
        /// Google hiểu ngữ cảnh qua NLP, BERT, MUM algorithms
        /// Nội dung cần có từ đồng nghĩa, từ liên quan thay vì lặp lại keyword chính
        /// </summary>
        private void CheckSemanticContent(HtmlDocument doc, string focusKeyword, SeoAnalysisResult result)
        {
            if (string.IsNullOrWhiteSpace(focusKeyword)) return;

            var content = doc.DocumentNode.InnerText.ToLower();

            // Kiểm tra sự đa dạng của từ vựng
            var words = Regex.Split(content, @"\s+")
                .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 3)
                .Select(w => w.ToLower())
                .ToList();

            var uniqueWords = words.Distinct().Count();
            var totalWords = words.Count;

            if (totalWords > 0)
            {
                double lexicalDiversity = (double)uniqueWords / totalWords;

                if (lexicalDiversity > 0.6)
                {
                    result.Recommendations.Add($"✓ Từ vựng đa dạng ({lexicalDiversity:P0}) - Tốt cho Semantic SEO");
                }
                else if (lexicalDiversity < 0.4)
                {
                    result.Warnings.Add($"Từ vựng thiếu đa dạng ({lexicalDiversity:P0}) - Nên sử dụng LSI keywords và từ đồng nghĩa");
                }
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: Readability Score - Flesch Reading Ease
        /// Nội dung dễ đọc (score 60-70) được xếp hạng tốt hơn
        /// Câu ngắn (10-20 từ), từ ngữ đơn giản tăng User Experience
        /// </summary>
        private void CheckReadability(HtmlDocument doc, SeoAnalysisResult result)
        {
            var textContent = doc.DocumentNode.InnerText;
            var sentences = Regex.Split(textContent, @"[.!?]+").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var words = Regex.Split(textContent, @"\s+").Where(w => !string.IsNullOrWhiteSpace(w)).ToList();

            if (sentences.Count == 0 || words.Count == 0) return;

            double avgWordsPerSentence = (double)words.Count / sentences.Count;

            if (avgWordsPerSentence > 25)
            {
                result.Warnings.Add($"Câu trung bình quá dài ({avgWordsPerSentence:F1} từ/câu) - Nên giảm xuống 15-20 từ để dễ đọc");
            }
            else if (avgWordsPerSentence >= 15 && avgWordsPerSentence <= 20)
            {
                result.Recommendations.Add($"✓ Độ dài câu phù hợp ({avgWordsPerSentence:F1} từ/câu) - Readability tốt");
            }
            else if (avgWordsPerSentence < 10)
            {
                result.Warnings.Add($"Câu quá ngắn ({avgWordsPerSentence:F1} từ/câu) - Nội dung có thể thiếu chiều sâu");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024: External Links phải đến authority sites
        /// Link đến .gov, .edu, authority domains (.com uy tín) tăng trust
        /// Nofollow cho sponsored/affiliate links
        /// </summary>
        private void CheckExternalLinksQuality(HtmlDocument doc, SeoAnalysisResult result)
        {
            var externalLinks = doc.DocumentNode.SelectNodes("//a[@href]")?.Where(l =>
            {
                var href = l.GetAttributeValue("href", "");
                return href.StartsWith("http://") || href.StartsWith("https://");
            }).ToList();

            if (externalLinks == null || externalLinks.Count == 0) return;

            int authorityLinks = 0;
            int nofollowLinks = 0;

            foreach (var link in externalLinks)
            {
                var href = link.GetAttributeValue("href", "").ToLower();
                var rel = link.GetAttributeValue("rel", "").ToLower();

                // Kiểm tra authority domains
                if (href.Contains(".gov") || href.Contains(".edu") ||
                    href.Contains("wikipedia.org") || href.Contains("google.com"))
                {
                    authorityLinks++;
                }

                if (rel.Contains("nofollow"))
                {
                    nofollowLinks++;
                }
            }

            if (authorityLinks > 0)
            {
                result.Recommendations.Add($"✓ Có {authorityLinks} external links đến authority sites - Tăng trustworthiness");
            }
            else if (externalLinks.Count > 2)
            {
                result.Recommendations.Add("Nên thêm 1-2 external links đến nguồn uy tín (.gov, .edu, Wikipedia)");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: Table of Contents giúp Featured Snippets
        /// TOC với anchor links cải thiện User Experience và Dwell Time
        /// Google ưu tiên nội dung có cấu trúc rõ ràng với jump links
        /// </summary>
        private void CheckTableOfContents(HtmlDocument doc, SeoAnalysisResult result)
        {
            // Kiểm tra có TOC
            var toc = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'toc')] | //*[contains(@id,'toc')] | //*[contains(@class,'table-of-contents')]");

            if (toc != null)
            {
                result.Recommendations.Add("✓ Có Table of Contents - Tăng khả năng xuất hiện Featured Snippets");
                result.HasTableOfContents = true;
            }
            else if (result.H2Count >= 5)
            {
                result.Recommendations.Add("Nên thêm Table of Contents (TOC) - Nội dung dài nên có mục lục để dễ navigation");
            }
        }

        /// <summary>
        /// Tiêu chuẩn 2024-2025: FAQ Schema tăng khả năng xuất hiện trong Rich Results
        /// FAQ section giúp chiếm nhiều SERP real estate
        /// Google ưu tiên hiển thị FAQ accordion trong search results
        /// </summary>
        private void CheckFAQSection(HtmlDocument doc, SeoAnalysisResult result)
        {
            // Kiểm tra có FAQ section
            var faqSection = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'faq')] | //*[contains(@id,'faq')]");

            // Kiểm tra FAQ Schema
            var faqSchema = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']")?.Any(s =>
                s.InnerText.ToLower().Contains("\"@type\":\"faqpage\""));

            if (faqSection != null || faqSchema == true)
            {
                result.Recommendations.Add("✓ Có FAQ section - Tăng khả năng xuất hiện Rich Results (2024-2025)");
                result.HasFAQ = true;

                if (faqSchema != true)
                {
                    result.Warnings.Add("Có FAQ nhưng thiếu FAQ Schema - Nên thêm JSON-LD để hiển thị accordion trên Google");
                }
            }
            else if (result.WordCount > 800)
            {
                result.Recommendations.Add("Nên thêm FAQ section với Schema - Giúp chiếm nhiều diện tích SERP hơn");
            }
        }

        /// <summary>
        /// Tính điểm tổng thể dựa trên tiêu chuẩn 2024-2025
        /// </summary>
        private int CalculateScore(SeoAnalysisResult result)
        {
            int score = 100;

            // Trừ điểm cho errors (nghiêm trọng)
            score -= result.Errors.Count * 12;

            // Trừ điểm cho warnings
            score -= result.Warnings.Count * 4;

            // Cộng điểm bonus cho các yếu tố 2024-2025
            if (result.HasStructuredData) score += 5;
            if (result.HasVideo) score += 5;
            if (result.HasTableOfContents) score += 3;
            if (result.HasFAQ) score += 5;
            if (result.WordCount >= 1500) score += 5;

            return Math.Max(0, Math.Min(100, score));
        }
    }

    /// <summary>
    /// Kết quả phân tích SEO theo tiêu chuẩn 2024-2025
    /// </summary>
    public class SeoAnalysisResult
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();

        public int H1Count { get; set; }
        public int H2Count { get; set; }
        public int H3Count { get; set; }
        public int ImageCount { get; set; }
        public int WordCount { get; set; }
        public int InternalLinksCount { get; set; }
        public int ExternalLinksCount { get; set; }
        public int FocusKeywordCount { get; set; }
        public double KeywordDensity { get; set; }

        // Các thuộc tính mới cho tiêu chuẩn 2024-2025
        public bool HasStructuredData { get; set; }
        public bool HasVideo { get; set; }
        public bool HasTableOfContents { get; set; }
        public bool HasFAQ { get; set; }
        public string AnalyzedUrl { get; set; }

        public int OverallScore { get; set; }

        public string GetScoreLabel()
        {
            if (OverallScore >= 90) return "Xuất sắc";
            if (OverallScore >= 75) return "Tốt";
            if (OverallScore >= 60) return "Trung bình";
            if (OverallScore >= 40) return "Cần cải thiện";
            return "Kém";
        }

        public string GetScoreColor()
        {
            if (OverallScore >= 90) return "success";
            if (OverallScore >= 75) return "info";
            if (OverallScore >= 60) return "warning";
            return "danger";
        }
    }
}