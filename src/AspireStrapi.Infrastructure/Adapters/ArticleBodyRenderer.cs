using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AspireStrapi.Infrastructure.Adapters;

/// <summary>
/// Renders Strapi article dynamic-zone blocks into a single, safe HTML string.
/// Rich-text bodies are authored as Markdown; all raw text is HTML-encoded
/// before any inline markup is applied to avoid injection.
/// </summary>
internal static partial class ArticleBodyRenderer
{
    public static string? Render(
        IReadOnlyList<StrapiArticleBodyClient.Block> blocks,
        StrapiMediaUrlResolver media)
    {
        var html = new StringBuilder();

        foreach (StrapiArticleBodyClient.Block block in blocks)
        {
            switch (block.TypeName)
            {
                case "ComponentSharedRichText" when !string.IsNullOrWhiteSpace(block.Body):
                    html.Append("<div class=\"rich-text\">")
                        .Append(MarkdownToHtml(block.Body!))
                        .Append("</div>");
                    break;

                case "ComponentSharedQuote" when !string.IsNullOrWhiteSpace(block.Body):
                    html.Append("<blockquote class=\"article-quote\">");
                    if (!string.IsNullOrWhiteSpace(block.Title))
                    {
                        html.Append("<p class=\"article-quote__title\">")
                            .Append(WebUtility.HtmlEncode(block.Title))
                            .Append("</p>");
                    }

                    html.Append("<p>")
                        .Append(WebUtility.HtmlEncode(block.Body))
                        .Append("</p></blockquote>");
                    break;

                case "ComponentSharedMedia" when block.File?.Url is { } mediaUrl:
                    string src = media.Resolve(mediaUrl) ?? mediaUrl;
                    string alt = WebUtility.HtmlEncode(block.File.AlternativeText ?? string.Empty);
                    html.Append("<figure class=\"article-media\"><img src=\"")
                        .Append(WebUtility.HtmlEncode(src))
                        .Append("\" alt=\"")
                        .Append(alt)
                        .Append("\" loading=\"lazy\" /></figure>");
                    break;
            }
        }

        return html.Length == 0 ? null : html.ToString();
    }

    private static string MarkdownToHtml(string markdown)
    {
        string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
        var html = new StringBuilder();
        var paragraph = new StringBuilder();

        void FlushParagraph()
        {
            if (paragraph.Length == 0)
            {
                return;
            }

            html.Append("<p>").Append(InlineMarkdown(paragraph.ToString())).Append("</p>");
            paragraph.Clear();
        }

        foreach (string raw in lines)
        {
            string line = raw.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph();
                continue;
            }

            int heading = 0;
            while (heading < line.Length && line[heading] == '#')
            {
                heading++;
            }

            if (heading is >= 1 and <= 6 && heading < line.Length && line[heading] == ' ')
            {
                FlushParagraph();
                string text = InlineMarkdown(line[(heading + 1)..].Trim());
                html.Append("<h").Append(heading).Append('>')
                    .Append(text)
                    .Append("</h").Append(heading).Append('>');
                continue;
            }

            if (paragraph.Length > 0)
            {
                paragraph.Append("<br />");
            }

            paragraph.Append(line);
        }

        FlushParagraph();
        return html.ToString();
    }

    private static string InlineMarkdown(string text)
    {
        string encoded = WebUtility.HtmlEncode(text);

        encoded = BoldRegex().Replace(encoded, "<strong>$1</strong>");
        encoded = ItalicRegex().Replace(encoded, "<em>$1</em>");
        encoded = CodeRegex().Replace(encoded, "<code>$1</code>");
        encoded = LinkRegex().Replace(
            encoded, "<a href=\"$2\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");

        return encoded;
    }

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)")]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"`(.+?)`")]
    private static partial Regex CodeRegex();

    [GeneratedRegex(@"\[(.+?)\]\((https?://[^\s)]+)\)")]
    private static partial Regex LinkRegex();
}
