using System;
using System.Text.Json.Serialization;

namespace InvoiceSystem.Wpf.Models;

public class InvoiceDetailDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonPropertyName("statusName")]
    public string StatusName { get; set; } = string.Empty;

    [JsonPropertyName("invoiceDate")]
    public DateTime InvoiceDate { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("paidAmount")]
    public decimal PaidAmount { get; set; }

    [JsonPropertyName("remainingAmount")]
    public decimal RemainingAmount { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("pdfUrl")]
    public string? PdfUrl { get; set; }

    [JsonPropertyName("pdfPath")]
    public string? PdfPath { get; set; }
}
