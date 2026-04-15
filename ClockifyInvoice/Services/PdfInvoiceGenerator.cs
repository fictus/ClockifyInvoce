using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using ClockifyInvoice.Models;

namespace ClockifyInvoice.Services
{
    public class PdfInvoiceGenerator
    {
        public void Generate(string outputPath, InvoiceProfile profile, List<InvoiceLineItem> items, string invoiceNumber, DateTime invoiceDate)
        {
            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf, iText.Kernel.Geom.PageSize.A4);
            doc.SetMargins(40, 50, 40, 50);

            // Parse accent color
            var accentColor = ParseColor(profile.AccentColorHex);
            var lightGray = new DeviceRgb(245, 247, 250);
            var mediumGray = new DeviceRgb(180, 185, 195);
            var darkText = new DeviceRgb(30, 35, 45);
            var mutedText = new DeviceRgb(100, 110, 125);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // ─── HEADER ───────────────────────────────────────────────────────────
            var header = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                .UseAllAvailableWidth();

            // Left: Freelancer name + label
            var leftCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(20);
            leftCell.Add(new Paragraph("INVOICE")
                .SetFont(fontBold).SetFontSize(28).SetFontColor(accentColor));
            leftCell.Add(new Paragraph(profile.FreelancerName)
                .SetFont(fontBold).SetFontSize(14).SetFontColor(darkText).SetMarginTop(4));
            if (!string.IsNullOrWhiteSpace(profile.FreelancerEmail))
                leftCell.Add(new Paragraph(profile.FreelancerEmail)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.FreelancerPhone))
                leftCell.Add(new Paragraph(profile.FreelancerPhone)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.FreelancerWebsite))
                leftCell.Add(new Paragraph(profile.FreelancerWebsite)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.FreelancerAddress))
                leftCell.Add(new Paragraph(profile.FreelancerAddress)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.FreelancerCity))
                leftCell.Add(new Paragraph(profile.FreelancerCity)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            header.AddCell(leftCell);

            // Right: Invoice meta
            var rightCell = new Cell().SetBorder(Border.NO_BORDER).SetPaddingBottom(20)
                .SetTextAlignment(TextAlignment.RIGHT);
            var dueDate = invoiceDate.AddDays(profile.PaymentTermsDays);
            rightCell.Add(new Paragraph($"# {invoiceNumber}")
                .SetFont(fontBold).SetFontSize(13).SetFontColor(accentColor));
            rightCell.Add(new Paragraph($"Date: {invoiceDate:MMMM dd, yyyy}")
                .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText).SetMarginTop(8));
            rightCell.Add(new Paragraph($"Due: {dueDate:MMMM dd, yyyy}")
                .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            header.AddCell(rightCell);

            doc.Add(header);

            // Accent rule
            var solidLine = new iText.Kernel.Pdf.Canvas.Draw.SolidLine(2f);
            solidLine.SetColor(accentColor);
            var separator = new LineSeparator(solidLine);
            separator.SetWidth(UnitValue.CreatePercentValue(100));
            separator.SetMarginBottom(20);

            doc.Add(separator);

            // ─── BILL TO ──────────────────────────────────────────────────────────
            var billingTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .UseAllAvailableWidth().SetMarginBottom(24);

            var billToCell = new Cell().SetBorder(Border.NO_BORDER);
            billToCell.Add(new Paragraph("BILL TO")
                .SetFont(fontBold).SetFontSize(8).SetFontColor(accentColor).SetCharacterSpacing(1.5f));
            billToCell.Add(new Paragraph(profile.ClientName)
                .SetFont(fontBold).SetFontSize(11).SetFontColor(darkText).SetMarginTop(4));
            if (!string.IsNullOrWhiteSpace(profile.ClientEmail))
                billToCell.Add(new Paragraph(profile.ClientEmail)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.ClientAddress))
                billToCell.Add(new Paragraph(profile.ClientAddress)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            if (!string.IsNullOrWhiteSpace(profile.ClientCity))
                billToCell.Add(new Paragraph(profile.ClientCity)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            billingTable.AddCell(billToCell);
            billingTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)); // spacer
            doc.Add(billingTable);

            // ─── LINE ITEMS TABLE ─────────────────────────────────────────────────
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 12, 22, 36, 10, 10, 10 }))
                .UseAllAvailableWidth().SetMarginBottom(0);

            // Column headers
            string[] headers = { "Date", "Project", "Description", "Hours", "Rate", "Amount" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell()
                    .SetBackgroundColor(accentColor)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(8)
                    .Add(new Paragraph(h)
                        .SetFont(fontBold).SetFontSize(8.5f)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetCharacterSpacing(0.5f)));
            }

            bool isOdd = true;
            foreach (var item in items)
            {
                var rowBg = isOdd ? ColorConstants.WHITE : lightGray;
                isOdd = !isOdd;

                var borderColor = new DeviceRgb(230, 232, 236);
                var bottomBorder = new SolidBorder(borderColor, 0.5f);

                Cell MakeCell(string text, bool bold = false, TextAlignment align = TextAlignment.LEFT) =>
                    new Cell()
                        .SetBackgroundColor(rowBg)
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetBorderRight(Border.NO_BORDER)
                        .SetBorderTop(Border.NO_BORDER)
                        .SetBorderBottom(bottomBorder)
                        .SetPaddingTop(7).SetPaddingBottom(7).SetPaddingLeft(8).SetPaddingRight(8)
                        .Add(new Paragraph(text)
                            .SetFont(bold ? fontBold : fontRegular)
                            .SetFontSize(8.5f)
                            .SetFontColor(darkText)
                            .SetTextAlignment(align));

                table.AddCell(MakeCell(item.Date));
                table.AddCell(MakeCell(item.Project));
                table.AddCell(MakeCell(item.Description));
                // For custom fixed-amount entries (Hours==0 and Rate==0) show "—" instead of 0.00
                bool isFixed = item.PrecomputedAmount > 0 && item.Hours == 0 && item.Rate == 0;
                table.AddCell(MakeCell(isFixed ? "—" : item.Hours.ToString("F2"), align: TextAlignment.RIGHT));
                table.AddCell(MakeCell(isFixed ? "—" : $"{profile.CurrencySymbol}{item.Rate:F2}", align: TextAlignment.RIGHT));
                table.AddCell(MakeCell($"{profile.CurrencySymbol}{item.Amount:F2}", bold: true, align: TextAlignment.RIGHT));
            }

            doc.Add(table);

            // ─── TOTALS ───────────────────────────────────────────────────────────
            var subtotal = items.Sum(i => i.Amount);
            var tax = subtotal * (profile.TaxRate / 100m);
            var total = subtotal + tax;

            var totalsTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 20, 20 }))
                .UseAllAvailableWidth().SetMarginTop(0).SetMarginBottom(24);

            Cell TotalCell(string text, bool bold = false, bool highlight = false, TextAlignment align = TextAlignment.RIGHT) =>
                new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(highlight ? accentColor : (bold ? lightGray : ColorConstants.WHITE))
                    .SetPaddingTop(6).SetPaddingBottom(6).SetPaddingLeft(8).SetPaddingRight(8)
                    .Add(new Paragraph(text)
                        .SetFont(bold ? fontBold : fontRegular)
                        .SetFontSize(bold ? 10f : 9f)
                        .SetFontColor(highlight ? ColorConstants.WHITE : darkText)
                        .SetTextAlignment(align));

            // Subtotal row
            totalsTable.AddCell(TotalCell("", align: TextAlignment.LEFT));
            totalsTable.AddCell(TotalCell("Subtotal"));
            totalsTable.AddCell(TotalCell($"{profile.CurrencySymbol}{subtotal:F2}"));

            // Tax row (if applicable)
            if (profile.TaxRate > 0)
            {
                totalsTable.AddCell(TotalCell("", align: TextAlignment.LEFT));
                totalsTable.AddCell(TotalCell($"{profile.TaxLabel} ({profile.TaxRate}%)"));
                totalsTable.AddCell(TotalCell($"{profile.CurrencySymbol}{tax:F2}"));
            }

            // Total row
            totalsTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
            totalsTable.AddCell(TotalCell($"TOTAL ({profile.Currency})", bold: true, highlight: true));
            totalsTable.AddCell(TotalCell($"{profile.CurrencySymbol}{total:F2}", bold: true, highlight: true));

            doc.Add(totalsTable);

            // ─── PAYMENT NOTES ────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(profile.PaymentNotes))
            {
                var dottedLine = new iText.Kernel.Pdf.Canvas.Draw.DottedLine();
                dottedLine.SetColor(mediumGray);
                var dottedSeparator = new LineSeparator(dottedLine);
                dottedSeparator.SetWidth(UnitValue.CreatePercentValue(100));
                dottedSeparator.SetMarginBottom(12);

                doc.Add(dottedSeparator);

                doc.Add(new Paragraph("PAYMENT NOTES")
                    .SetFont(fontBold).SetFontSize(8).SetFontColor(accentColor).SetCharacterSpacing(1.5f).SetMarginBottom(4));
                doc.Add(new Paragraph(profile.PaymentNotes)
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText));
            }

            // ─── FOOTER ───────────────────────────────────────────────────────────
            doc.Add(new Paragraph($"Thank you for your business, {profile.ClientName}!")
                .SetFont(fontRegular).SetFontSize(9).SetFontColor(mutedText)
                .SetTextAlignment(TextAlignment.LEFT).SetMarginTop(0));

            doc.Close();
        }

        private DeviceRgb ParseColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return new DeviceRgb(r, g, b);
                }
            }
            catch { }
            return new DeviceRgb(44, 95, 138);
        }
    }
}
