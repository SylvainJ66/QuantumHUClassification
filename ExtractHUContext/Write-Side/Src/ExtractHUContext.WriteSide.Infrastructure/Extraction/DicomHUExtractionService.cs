using System.IO.Compression;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Domain.Ports;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Infrastructure.Extraction;

/// <summary>
/// Service for extracting Hounsfield Unit statistics from DICOM medical study files.
/// Uses fo-dicom library to parse DICOM files and extract HU values from pixel data.
/// </summary>
public class DicomHuExtractionService : IHuExtractionService
{
    public async Task<Result<HuStatistics>> ExtractHuStatisticsAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!fileStream.CanRead)
                return Result.Failure<HuStatistics>("Invalid file stream");

            if (fileStream.Length == 0)
                return Result.Failure<HuStatistics>("File stream is empty");

            var allHuValues = new List<double>();

            // Check if file is a ZIP archive (common for medical studies)
            fileStream.Position = 0;
            if (IsZipFile(fileStream))
            {
                fileStream.Position = 0;
                await ExtractHuFromZipArchive(fileStream, allHuValues, cancellationToken);
            }
            else
            {
                // Single DICOM file
                fileStream.Position = 0;
                await ExtractHuFromDicomFile(fileStream, allHuValues, cancellationToken);
            }

            if (allHuValues.Count == 0)
                return Result.Failure<HuStatistics>("No valid DICOM files with pixel data found");

            var statistics = CalculateStatistics(allHuValues);
            return statistics;
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<HuStatistics>("Extraction was cancelled");
        }
        catch (Exception ex)
        {
            return Result.Failure<HuStatistics>($"Failed to extract HU statistics: {ex.Message}");
        }
    }

    private static bool IsZipFile(Stream stream)
    {
        try
        {
            var buffer = new byte[4];
            var bytesRead = stream.Read(buffer, 0, 4);
            if (bytesRead < 4)
                return false;
            // ZIP file magic number: 0x50 0x4B 0x03 0x04
            return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
        }
        catch
        {
            return false;
        }
    }

    private static async Task ExtractHuFromZipArchive(
        Stream zipStream,
        List<double> allHuValues,
        CancellationToken cancellationToken)
    {
        await using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Skip directories and non-DICOM files
            if (entry.Length == 0 || entry.FullName.EndsWith("/"))
                continue;

            // Common DICOM file extensions
            var extension = Path.GetExtension(entry.Name).ToLowerInvariant();
            if (extension != ".dcm" && extension != ".dicom" && !string.IsNullOrEmpty(extension))
                continue;

            try
            {
                await using var entryStream = await entry.OpenAsync(cancellationToken);
                using var memoryStream = new MemoryStream();
                await entryStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                await ExtractHuFromDicomFile(memoryStream, allHuValues, cancellationToken);
            }
            catch
            {
                // Skip invalid DICOM files
            }
        }
    }

    private static Task ExtractHuFromDicomFile(
        Stream dicomStream,
        List<double> allHuValues,
        CancellationToken cancellationToken)
    {
        try
        {
            var dicomFile = DicomFile.Open(dicomStream);
            var dataset = dicomFile.Dataset;

            // Check if this is a CT image (required for HU values)
            var modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);
            if (modality != "CT")
                return Task.CompletedTask;

            // Get rescale parameters for HU conversion
            var rescaleSlope = dataset.GetSingleValueOrDefault(DicomTag.RescaleSlope, 1.0);
            var rescaleIntercept = dataset.GetSingleValueOrDefault(DicomTag.RescaleIntercept, 0.0);

            // Extract pixel data
            var pixelData = DicomPixelData.Create(dataset);
            var frame = pixelData.GetFrame(0);

            // Convert pixel values to HU
            var huValues = ConvertPixelDataToHu(frame.Data, rescaleSlope, rescaleIntercept, pixelData.BitsStored);

            if (cancellationToken.IsCancellationRequested)
                return Task.CompletedTask;

            allHuValues.AddRange(huValues);
        }
        catch
        {
            // Skip invalid or unsupported DICOM files
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<double> ConvertPixelDataToHu(
        byte[] pixelData,
        double rescaleSlope,
        double rescaleIntercept,
        int bitsStored)
    {
        var huValues = new List<double>();

        // Handle different pixel data formats
        if (bitsStored <= 8)
        {
            // 8-bit data
            foreach (var pixelValue in pixelData)
            {
                var hu = pixelValue * rescaleSlope + rescaleIntercept;
                huValues.Add(hu);
            }
        }
        else
        {
            // 16-bit data (most common for CT)
            for (var i = 0; i < pixelData.Length - 1; i += 2)
            {
                var pixelValue = BitConverter.ToInt16(pixelData, i);
                var hu = (pixelValue * rescaleSlope) + rescaleIntercept;
                huValues.Add(hu);
            }
        }

        return huValues;
    }

    private static Result<HuStatistics> CalculateStatistics(List<double> huValues)
    {
        if (huValues.Count == 0)
            return Result.Failure<HuStatistics>("No HU values to calculate statistics");

        var mean = huValues.Average();
        var min = huValues.Min();
        var max = huValues.Max();

        // Calculate standard deviation
        var sumOfSquares = huValues.Sum(hu => Math.Pow(hu - mean, 2));
        var variance = sumOfSquares / huValues.Count;
        var stdDev = Math.Sqrt(variance);

        var voxelCount = huValues.Count;

        return HuStatistics.Create(mean, min, max, stdDev, voxelCount);
    }
}
