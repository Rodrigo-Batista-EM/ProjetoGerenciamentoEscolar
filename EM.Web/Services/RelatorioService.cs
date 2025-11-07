using System.Linq;
using EM.Domain;
using EM.Web.Interfaces;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace EM.Web.Services
{
    public class RelatorioService : IRelatorioService
    {
        public byte[] GerarRelatorioAlunosPDF(IEnumerable<Aluno> alunos)
        {
            try
            {
                using var ms = new MemoryStream();
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var doc = new Document(pdf, PageSize.A4);
                doc.SetMargins(36, 36, 54, 36);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Cabeçalho
            var headerTable = new Table(1).UseAllAvailableWidth();
            var headerCell = new Cell()
                .Add(new Paragraph("ESCOLAR MANAGER\nRELATÓRIO DE ALUNOS").SetFont(fontBold).SetFontSize(14))
                .SetBackgroundColor(new DeviceRgb(220, 53, 69))
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(10)
                .SetBorder(Border.NO_BORDER);
            headerTable.AddCell(headerCell);
            doc.Add(headerTable);

            // Data/Hora
            doc.Add(new Paragraph($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));

            // Tabela de alunos
            var table = new Table(new float[] { 12, 28, 18, 14, 10, 18, 10 })
                .UseAllAvailableWidth();

            // Cabeçalhos
            AddHeader(table, "Matrícula", fontBold);
            AddHeader(table, "Nome", fontBold);
            AddHeader(table, "CPF", fontBold);
            AddHeader(table, "Nascimento", fontBold);
            AddHeader(table, "Sexo", fontBold);
            AddHeader(table, "Cidade", fontBold);
            AddHeader(table, "UF", fontBold);

            var alunosList = alunos?.ToList() ?? new List<Aluno>();
            
            if (!alunosList.Any())
            {
                var noDataCell = new Cell(1, 7)
                    .Add(new Paragraph("Nenhum aluno encontrado para os filtros aplicados.")
                        .SetFont(font).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(10);
                table.AddCell(noDataCell);
            }
            else
            {
                foreach (var a in alunosList)
                {
                    AddCell(table, a.Matricula.ToString(), font);
                    AddCell(table, a.Nome ?? "—", font);
                    AddCell(table, FormatarCpf(a.CPF), font);
                    var nascimentoText = (a.Nascimento == default ? "—" : a.Nascimento.ToString("dd/MM/yyyy"));
                    AddCell(table, nascimentoText, font);
                    AddCell(table, a.Sexo.ToString(), font);
                    AddCell(table, a.CidadeNome ?? "—", font);
                    AddCell(table, a.UF ?? "—", font);
                }
            }

            doc.Add(table);

            // Rodapé simples com numeração de páginas
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                var pageSize = page.GetPageSize();
                var canvas = new PdfCanvas(page);
                var footer = new Paragraph($"Página {i} de {pdf.GetNumberOfPages()}")
                    .SetFont(font).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER);
                new Canvas(canvas, pageSize)
                    .ShowTextAligned(footer, pageSize.GetWidth() / 2, 20, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
            }

                doc.Close();
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna um PDF simples com a mensagem de erro
                using var errorMs = new MemoryStream();
                var errorWriter = new PdfWriter(errorMs);
                var errorPdf = new PdfDocument(errorWriter);
                var errorDoc = new Document(errorPdf, PageSize.A4);
                
                var errorFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                errorDoc.Add(new Paragraph("Erro ao gerar relatório")
                    .SetFont(errorFont).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                errorDoc.Add(new Paragraph($"Mensagem: {ex.Message}")
                    .SetFont(errorFont).SetFontSize(12));
                errorDoc.Add(new Paragraph($"Stack: {ex.StackTrace}")
                    .SetFont(errorFont).SetFontSize(8));
                
                errorDoc.Close();
                return errorMs.ToArray();
            }
        }

        public byte[] GerarRelatorioCidadesPDF(IEnumerable<Cidade> cidades)
        {
            // Placeholder para futuros relatórios de cidades
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf, PageSize.A4);
            doc.Add(new Paragraph("Relatório de Cidades - Em breve"));
            doc.Close();
            return ms.ToArray();
        }

        private void AddHeader(Table table, string text, PdfFont font)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
        }

        private void AddCell(Table table, string text, PdfFont font)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetPadding(5));
        }

        private string FormatarCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) || cpf.Length != 11) return "—";
            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }
    }
}