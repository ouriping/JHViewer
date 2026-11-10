using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Syntax;
using MdTable = Markdig.Extensions.Tables.Table;
using MdTableCell = Markdig.Extensions.Tables.TableCell;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using Markdig.Syntax.Inlines;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace MarkdownPreview.Shell;

/// <summary>把 Markdown 写成带本地图片的 Word (.docx) 文档。</summary>
public sealed class MarkdownDocxExporter
{
    private const int MaxImageWidthPx = 620;
    private const long EmuPerPixel = 9525;

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly Dictionary<string, string> _imageRelIds = new(StringComparer.OrdinalIgnoreCase);
    private uint _drawingId = 1;

    public void Export(string markdown, string? baseDirectory, string outputPath)
    {
        var outputFull = Path.GetFullPath(outputPath);
        var outputDir = Path.GetDirectoryName(outputFull);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        _imageRelIds.Clear();
        _drawingId = 1;

        using var document = WordprocessingDocument.Create(outputFull, WordprocessingDocumentType.Document);
        var main = document.AddMainDocumentPart();
        main.Document = new Document(new Body());
        AddStyles(main);
        var body = main.Document.Body ?? throw new InvalidOperationException("无法创建 Word 文档正文。");
        var parsed = Markdown.Parse(markdown ?? string.Empty, Pipeline);
        foreach (var block in parsed)
        {
            AddBlock(body, block, 0, main, baseDirectory);
        }

        if (!body.Elements<Paragraph>().Any() && !body.Elements<Table>().Any())
        {
            body.Append(new Paragraph(new Run(new Text(string.Empty))));
        }

        body.Append(new SectionProperties(
            new PageSize { Width = 11906, Height = 16838 },
            new PageMargin { Top = 1134, Bottom = 1134, Left = 1134, Right = 1134, Header = 720, Footer = 720 }));
        main.Document.Save();
    }

    private void AddBlock(Body body, Block block, int listLevel, MainDocumentPart main, string? baseDirectory)
    {
        switch (block)
        {
            case HeadingBlock heading:
                body.Append(CreateParagraph(heading.Inline, main, baseDirectory, listLevel, bullet: null, headingLevel: heading.Level));
                break;
            case ParagraphBlock paragraph:
                body.Append(CreateParagraph(paragraph.Inline, main, baseDirectory, listLevel, bullet: null, headingLevel: 0));
                break;
            case ListBlock list:
                AddList(body, list, listLevel, main, baseDirectory);
                break;
            case FencedCodeBlock or CodeBlock:
                AddCode(body, ((CodeBlock)block).Lines.ToString(), listLevel);
                break;
            case QuoteBlock quote:
                foreach (var child in quote)
                {
                    AddBlock(body, child, listLevel + 1, main, baseDirectory);
                }
                break;
            case ThematicBreakBlock:
                body.Append(new Paragraph(
                    new ParagraphProperties(new ParagraphBorders(
                        new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "D0D7DE" })),
                    new Run(new Text(" ") { Space = SpaceProcessingModeValues.Preserve })));
                break;
            case MdTable table:
                body.Append(CreateTable(table, main, baseDirectory));
                break;
            case HtmlBlock html:
                AddCode(body, html.Lines.ToString(), listLevel);
                break;
        }
    }

    private void AddList(Body body, ListBlock list, int level, MainDocumentPart main, string? baseDirectory)
    {
        var index = 1;
        if (list.IsOrdered && int.TryParse(list.OrderedStart, out var start))
        {
            index = start;
        }

        foreach (var item in list)
        {
            if (item is not ListItemBlock listItem)
            {
                continue;
            }

            var bullet = list.IsOrdered ? index + ". " : "• ";
            index++;
            var first = true;
            foreach (var child in listItem)
            {
                if (child is ListBlock nested)
                {
                    AddList(body, nested, level + 1, main, baseDirectory);
                    first = false;
                    continue;
                }

                if (child is ParagraphBlock paragraph)
                {
                    body.Append(CreateParagraph(paragraph.Inline, main, baseDirectory, level, first ? bullet : null, headingLevel: 0));
                    first = false;
                    continue;
                }

                AddBlock(body, child, level + 1, main, baseDirectory);
                first = false;
            }
        }
    }

    private Paragraph CreateParagraph(
        ContainerInline? inlines,
        MainDocumentPart main,
        string? baseDirectory,
        int listLevel,
        string? bullet,
        int headingLevel)
    {
        var paragraph = new Paragraph();
        var properties = new ParagraphProperties();
        if (headingLevel is >= 1 and <= 6)
        {
            properties.Append(new ParagraphStyleId { Val = "Heading" + headingLevel });
        }

        var indent = listLevel * 420;
        if (indent > 0)
        {
            properties.Append(new Indentation { Left = indent.ToString() });
        }

        if (properties.HasChildren)
        {
            paragraph.Append(properties);
        }

        if (!string.IsNullOrEmpty(bullet))
        {
            paragraph.Append(CreateRun(bullet, TextStyle.Plain));
        }

        AppendInlines(paragraph, inlines, TextStyle.Plain, main, baseDirectory);
        if (!paragraph.Elements<Run>().Any())
        {
            paragraph.Append(new Run(new Text(string.Empty)));
        }

        return paragraph;
    }

    private void AppendInlines(
        Paragraph paragraph,
        Inline? inline,
        TextStyle style,
        MainDocumentPart main,
        string? baseDirectory)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case LiteralInline literal:
                    AppendText(paragraph, literal.Content.ToString(), style);
                    break;
                case CodeInline code:
                    AppendText(paragraph, code.Content, style with { Code = true });
                    break;
                case EmphasisInline emphasis:
                    AppendInlines(paragraph, emphasis.FirstChild, style.WithEmphasis(emphasis), main, baseDirectory);
                    break;
                case LinkInline { IsImage: true } image:
                    AppendImage(paragraph, image, main, baseDirectory);
                    break;
                case LinkInline link:
                    AppendLink(paragraph, link, style, main, baseDirectory);
                    break;
                case LineBreakInline:
                    paragraph.Append(new Run(new Break()));
                    break;
                case HtmlEntityInline entity:
                    AppendText(paragraph, entity.Transcoded.ToString(), style);
                    break;
                case AutolinkInline autolink:
                    AppendText(paragraph, autolink.Url ?? string.Empty, style);
                    break;
                case ContainerInline container:
                    AppendInlines(paragraph, container.FirstChild, style, main, baseDirectory);
                    break;
            }
        }
    }

    private static void AppendText(Paragraph paragraph, string? text, TextStyle style)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        paragraph.Append(CreateRun(text, style));
    }

    private void AppendLink(
        Paragraph paragraph,
        LinkInline link,
        TextStyle style,
        MainDocumentPart main,
        string? baseDirectory)
    {
        var url = link.Url ?? string.Empty;
        var label = InlineText(link) is { Length: > 0 } text ? text : url;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var absolute) || absolute.Scheme is not ("http" or "https" or "mailto"))
        {
            AppendText(paragraph, label, style);
            return;
        }

        var relation = main.AddHyperlinkRelationship(absolute, isExternal: true);
        var hyperlink = new Hyperlink(CreateRun(label, style with { Link = true })) { Id = relation.Id };
        paragraph.Append(hyperlink);
    }

    private void AppendImage(Paragraph paragraph, LinkInline image, MainDocumentPart main, string? baseDirectory)
    {
        var alt = InlineText(image);
        if (!MarkdownAssetPath.TryResolveLocalFile(image.Url, baseDirectory, out var fullPath))
        {
            AppendText(paragraph, string.IsNullOrWhiteSpace(alt) ? "[图片缺失]" : "[图片缺失: " + alt + "]", TextStyle.Plain with { Italic = true });
            return;
        }

        if (!_imageRelIds.TryGetValue(fullPath, out var relationId))
        {
            var part = main.AddImagePart(ImageContentType(fullPath));
            using (var stream = File.OpenRead(fullPath))
            {
                part.FeedData(stream);
            }

            relationId = main.GetIdOfPart(part);
            _imageRelIds[fullPath] = relationId;
        }

        var (width, height) = FitImage(fullPath);
        var id = _drawingId++;
        paragraph.Append(new Run(CreateImage(relationId, width * EmuPerPixel, height * EmuPerPixel, id, Path.GetFileName(fullPath))));
    }

    private Table CreateTable(MdTable markdownTable, MainDocumentPart main, string? baseDirectory)
    {
        var table = new Table();
        table.Append(new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "D0D7DE" })));

        foreach (var rowBlock in markdownTable)
        {
            if (rowBlock is not MdTableRow row)
            {
                continue;
            }

            var wordRow = new TableRow();
            foreach (var cellBlock in row)
            {
                if (cellBlock is not MdTableCell cell)
                {
                    continue;
                }

                var wordCell = new TableCell();
                var wrote = false;
                foreach (var child in cell)
                {
                    if (child is not ParagraphBlock paragraph)
                    {
                        continue;
                    }

                    var created = CreateParagraph(paragraph.Inline, main, baseDirectory, 0, bullet: null, headingLevel: 0);
                    if (row.IsHeader)
                    {
                        foreach (var run in created.Descendants<Run>())
                        {
                            var props = run.RunProperties ?? new RunProperties();
                            if (run.RunProperties is null)
                            {
                                run.PrependChild(props);
                            }

                            if (props.GetFirstChild<Bold>() is null)
                            {
                                props.Append(new Bold());
                            }
                        }
                    }

                    wordCell.Append(created);
                    wrote = true;
                }

                if (!wrote)
                {
                    wordCell.Append(new Paragraph(new Run(new Text(string.Empty))));
                }

                wordRow.Append(wordCell);
            }

            table.Append(wordRow);
        }

        return table;
    }

    private static void AddCode(Body body, string code, int listLevel)
    {
        var lines = (code ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
        {
            var properties = new ParagraphProperties(
                new Shading { Val = ShadingPatternValues.Clear, Fill = "F6F8FA" });
            if (listLevel > 0)
            {
                properties.Append(new Indentation { Left = (listLevel * 420).ToString() });
            }

            body.Append(new Paragraph(properties, CreateRun(line.Length == 0 ? " " : line, TextStyle.Plain with { Code = true })));
        }
    }

    private static Run CreateRun(string text, TextStyle style)
    {
        var properties = new RunProperties();
        properties.Append(new RunFonts
        {
            Ascii = style.Code ? "Consolas" : "Calibri",
            HighAnsi = style.Code ? "Consolas" : "Calibri",
            EastAsia = "Microsoft YaHei"
        });
        properties.Append(new FontSize { Val = style.Code ? "20" : "21" });
        if (style.Bold)
        {
            properties.Append(new Bold());
        }

        if (style.Italic)
        {
            properties.Append(new Italic());
        }

        if (style.Strike)
        {
            properties.Append(new Strike());
        }

        if (style.Code)
        {
            properties.Append(new Shading { Val = ShadingPatternValues.Clear, Fill = "F0F0F0" });
        }

        if (style.Link)
        {
            properties.Append(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = "0969DA" });
            properties.Append(new Underline { Val = UnderlineValues.Single });
        }

        return new Run(properties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
    }

    private static Drawing CreateImage(string relationId, long widthEmu, long heightEmu, uint id, string name)
    {
        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = id, Name = name },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = id, Name = name },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })))
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
            {
                DistanceFromTop = 0,
                DistanceFromBottom = 0,
                DistanceFromLeft = 0,
                DistanceFromRight = 0
            });
    }

    private static (int Width, int Height) FitImage(string path)
    {
        var (width, height) = ReadImageSize(path);
        if (width <= 0 || height <= 0)
        {
            width = MaxImageWidthPx;
            height = 360;
        }

        if (width > MaxImageWidthPx)
        {
            height = Math.Max(1, (int)Math.Round(height * (MaxImageWidthPx / (double)width)));
            width = MaxImageWidthPx;
        }

        return (width, height);
    }

    private static (int Width, int Height) ReadImageSize(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".png" && bytes.Length >= 24)
            {
                return (ReadInt32BE(bytes, 16), ReadInt32BE(bytes, 20));
            }

            if (ext is ".jpg" or ".jpeg")
            {
                return ReadJpegSize(bytes);
            }

            if (ext == ".gif" && bytes.Length >= 10)
            {
                return (BitConverter.ToUInt16(bytes, 6), BitConverter.ToUInt16(bytes, 8));
            }

            if (ext == ".bmp" && bytes.Length >= 26)
            {
                return (BitConverter.ToInt32(bytes, 18), Math.Abs(BitConverter.ToInt32(bytes, 22)));
            }
        }
        catch
        {
            // 读不到尺寸时使用默认宽度。
        }

        return (0, 0);
    }

    private static (int Width, int Height) ReadJpegSize(byte[] bytes)
    {
        var index = 2;
        while (index + 9 < bytes.Length)
        {
            if (bytes[index] != 0xFF)
            {
                index++;
                continue;
            }

            var marker = bytes[index + 1];
            if (marker is 0xC0 or 0xC1 or 0xC2)
            {
                var height = (bytes[index + 5] << 8) | bytes[index + 6];
                var width = (bytes[index + 7] << 8) | bytes[index + 8];
                return (width, height);
            }

            if (marker == 0xD8 || marker == 0xD9 || marker is >= 0xD0 and <= 0xD7)
            {
                index += 2;
                continue;
            }

            if (index + 3 >= bytes.Length)
            {
                break;
            }

            var length = (bytes[index + 2] << 8) | bytes[index + 3];
            index += 2 + Math.Max(length, 2);
        }

        return (0, 0);
    }

    private static int ReadInt32BE(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static PartTypeInfo ImageContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            ".tif" or ".tiff" => ImagePartType.Tiff,
            _ => ImagePartType.Png
        };
    }

    private static string InlineText(ContainerInline inline)
    {
        var buffer = new System.Text.StringBuilder();
        AppendInlineText(inline.FirstChild, buffer);
        return buffer.ToString();
    }

    private static void AppendInlineText(Inline? inline, System.Text.StringBuilder buffer)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case LiteralInline literal:
                    buffer.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    buffer.Append(code.Content);
                    break;
                case ContainerInline container:
                    AppendInlineText(container.FirstChild, buffer);
                    break;
            }
        }
    }

    private static void AddStyles(MainDocumentPart main)
    {
        var styles = new Styles();
        var normal = new Style { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
        normal.Append(new StyleName { Val = "Normal" });
        normal.Append(new StyleRunProperties(
            new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", EastAsia = "Microsoft YaHei" },
            new FontSize { Val = "21" }));
        styles.Append(normal);

        for (var level = 1; level <= 6; level++)
        {
            var size = Math.Max(21, 36 - (level - 1) * 3).ToString();
            var heading = new Style { Type = StyleValues.Paragraph, StyleId = "Heading" + level };
            heading.Append(new StyleName { Val = "heading " + level });
            heading.Append(new BasedOn { Val = "Normal" });
            heading.Append(new StyleRunProperties(
                new Bold(),
                new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri", EastAsia = "Microsoft YaHei" },
                new FontSize { Val = size }));
            styles.Append(heading);
        }

        var part = main.AddNewPart<StyleDefinitionsPart>();
        part.Styles = styles;
        styles.Save();
    }

    private readonly record struct TextStyle(bool Bold, bool Italic, bool Strike, bool Code, bool Link)
    {
        public static TextStyle Plain => new(false, false, false, false, false);

        public TextStyle WithEmphasis(EmphasisInline emphasis)
        {
            var bold = Bold || emphasis.DelimiterCount >= 2 && emphasis.DelimiterChar is '*' or '_';
            var italic = Italic || emphasis.DelimiterCount == 1 && emphasis.DelimiterChar is '*' or '_';
            var strike = Strike || emphasis.DelimiterChar == '~';
            return new TextStyle(bold, italic, strike, Code, Link);
        }
    }
}
