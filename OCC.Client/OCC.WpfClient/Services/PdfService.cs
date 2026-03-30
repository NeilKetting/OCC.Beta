using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OCC.Shared.Models;
using OCC.WpfClient.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace OCC.WpfClient.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // Initializing QuestPDF with the Community License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateOrderPdfAsync(Order order, bool isPrintVersion = false)
        {
            // Path to save the PDF
            var fileName = $"Order_{order.OrderNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                        // Header (Ported from Avalonia ComposePremium)
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("PURCHASE ORDER").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                                col.Item().Text($"# {order.OrderNumber}").FontSize(14);
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("Onsite Construction Care").FontSize(14).SemiBold();
                                col.Item().Text("Vat No: 4210204780");
                                col.Item().Text("Tel: 011 024 1234");
                                col.Item().Text("Email: accounts@onsitecare.co.za");
                            });
                        });

                        // Content
                        page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                        {
                            x.Spacing(20);

                            // Vendor & Shipping Info
                            x.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).Padding(5).Column(c =>
                                {
                                    c.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("VENDOR").SemiBold();
                                    c.Item().Text(order.SupplierName ?? "Unknown Supplier");
                                    c.Item().Text(order.EntityAddress ?? "");
                                    c.Item().Text(order.EntityTel ?? "");
                                });

                                row.ConstantItem(20);

                                row.RelativeItem().Border(1).Padding(5).Column(c =>
                                {
                                    c.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("SHIP TO").SemiBold();
                                    c.Item().Text(order.ProjectName ?? "N/A");
                                    c.Item().Text(order.Attention ?? "");
                                });
                            });

                            // Items Table
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // Qty
                                    columns.RelativeColumn();   // Item
                                    columns.ConstantColumn(80); // Rate
                                    columns.ConstantColumn(80); // Amount
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("QTY");
                                    header.Cell().Element(CellStyle).Text("ITEM / DESCRIPTION");
                                    header.Cell().Element(CellStyle).Text("RATE");
                                    header.Cell().Element(CellStyle).Text("AMOUNT");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.DefaultTextStyle(x => x.SemiBold())
                                                        .PaddingVertical(5)
                                                        .BorderBottom(1)
                                                        .BorderColor(Colors.Black);
                                    }
                                });

                                // Rows
                                foreach (var line in order.Lines.Where(l => l.QuantityOrdered > 0))
                                {
                                    table.Cell().Element(RowStyle).Text(line.QuantityOrdered.ToString());
                                    table.Cell().Element(RowStyle).Column(c => {
                                        c.Item().Text(line.ItemCode).Bold();
                                        if (!string.IsNullOrEmpty(line.Description))
                                            c.Item().Text(line.Description).FontSize(9).FontColor(Colors.Grey.Darken2);
                                    });
                                    table.Cell().Element(RowStyle).AlignRight().Text($"{line.UnitPrice:N2}");
                                    table.Cell().Element(RowStyle).AlignRight().Text($"{line.LineTotal:N2}");

                                    static IContainer RowStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1)
                                                        .BorderColor(Colors.Grey.Lighten2)
                                                        .PaddingVertical(5);
                                    }
                                }
                            });

                            // Totals
                            x.Item().AlignRight().Column(c =>
                            {
                                c.Spacing(5);
                                c.Item().Text(t =>
                                {
                                    t.Span("Subtotal: ").SemiBold();
                                    t.Span($"{order.SubTotal:N2}");
                                });
                                c.Item().Text(t =>
                                {
                                    t.Span("VAT (15%): ").SemiBold();
                                    t.Span($"{order.VatTotal:N2}");
                                });
                                c.Item().Text(t =>
                                {
                                    t.Span("Total: ").FontSize(14).ExtraBold();
                                    t.Span($"{order.TotalAmount:N2}").FontSize(14).ExtraBold();
                                });
                            });

                            // Remarks
                            if (!string.IsNullOrEmpty(order.ReferenceNo))
                            {
                                x.Item().Column(c =>
                                {
                                    c.Item().Text("REMARKS / NOTES").SemiBold();
                                    c.Item().PaddingTop(5).Text(order.ReferenceNo);
                                });
                            }
                        });

                        // Footer
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(filePath);
            });

            return filePath;
        }
    }
}
