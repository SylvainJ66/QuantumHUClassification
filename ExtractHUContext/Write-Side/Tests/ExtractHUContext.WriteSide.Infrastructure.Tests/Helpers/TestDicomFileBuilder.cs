using FellowOakDicom;
using FellowOakDicom.IO.Buffer;
using System.IO.Compression;

namespace ExtractHUContext.WriteSide.Infrastructure.Tests.Helpers;

/// <summary>
/// Helper class to create test DICOM files with pixel data for integration testing.
/// </summary>
public static class TestDicomFileBuilder
{
    public static Stream CreateSimpleCtDicomStream(
        int width = 512,
        int height = 512,
        double rescaleSlope = 1.0,
        double rescaleIntercept = -1024.0)
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.CTImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.StudyInstanceUID, DicomUID.Generate() },
            { DicomTag.SeriesInstanceUID, DicomUID.Generate() },
            { DicomTag.Modality, "CT" },
            { DicomTag.PatientID, "TEST001" },
            { DicomTag.PatientName, "Test^Patient" },
            { DicomTag.RescaleSlope, rescaleSlope },
            { DicomTag.RescaleIntercept, rescaleIntercept },
            { DicomTag.Rows, (ushort)height },
            { DicomTag.Columns, (ushort)width },
            { DicomTag.BitsAllocated, (ushort)16 },
            { DicomTag.BitsStored, (ushort)16 },
            { DicomTag.HighBit, (ushort)15 },
            { DicomTag.PixelRepresentation, (ushort)1 }, // Signed
            { DicomTag.SamplesPerPixel, (ushort)1 },
            { DicomTag.PhotometricInterpretation, "MONOCHROME2" }
        };

        // Create simple pixel data with known HU values
        // Using pattern: soft tissue (50 HU), water (0 HU), bone (1000 HU)
        var pixelCount = width * height;
        var pixelData = new short[pixelCount];

        for (var i = 0; i < pixelCount; i++)
        {
            pixelData[i] = (i % 3) switch
            {
                // Create a pattern with different HU values
                0 => (short)((50 - rescaleIntercept) / rescaleSlope),
                1 => (short)((0 - rescaleIntercept) / rescaleSlope),
                _ => (short)((1000 - rescaleIntercept) / rescaleSlope)
            };
        }

        // Convert to bytes
        var pixelBytes = new byte[pixelData.Length * 2];
        Buffer.BlockCopy(pixelData, 0, pixelBytes, 0, pixelBytes.Length);

        var pixelDataElement = new DicomOtherWord(DicomTag.PixelData, new MemoryByteBuffer(pixelBytes));
        dataset.Add(pixelDataElement);

        var dicomFile = new DicomFile(dataset);
        var stream = new MemoryStream();
        dicomFile.Save(stream);
        stream.Position = 0;

        return stream;
    }

    public static Stream CreateZipWithMultipleDicomFiles(int fileCount = 3)
    {
        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            for (var i = 0; i < fileCount; i++)
            {
                var entry = archive.CreateEntry($"CT_IMAGE_{i:D3}.dcm");
                using var entryStream = entry.Open();
                using var dicomStream = CreateSimpleCtDicomStream(
                    width: 256,
                    height: 256,
                    rescaleSlope: 1.0,
                    rescaleIntercept: -1024.0);
                dicomStream.CopyTo(entryStream);
            }
        }

        zipStream.Position = 0;
        return zipStream;
    }

    public static Stream CreateNonCtDicomStream()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.MRImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.StudyInstanceUID, DicomUID.Generate() },
            { DicomTag.SeriesInstanceUID, DicomUID.Generate() },
            { DicomTag.Modality, "MR" }, // MRI, not CT
            { DicomTag.PatientID, "TEST002" },
            { DicomTag.PatientName, "Test^MRI" },
            { DicomTag.Rows, (ushort)256 },
            { DicomTag.Columns, (ushort)256 },
            { DicomTag.BitsAllocated, (ushort)16 },
            { DicomTag.BitsStored, (ushort)16 },
            { DicomTag.HighBit, (ushort)15 },
            { DicomTag.PixelRepresentation, (ushort)0 },
            { DicomTag.SamplesPerPixel, (ushort)1 },
            { DicomTag.PhotometricInterpretation, "MONOCHROME2" }
        };

        var pixelData = new byte[256 * 256 * 2];
        var pixelDataElement = new DicomOtherWord(DicomTag.PixelData, new MemoryByteBuffer(pixelData));
        dataset.Add(pixelDataElement);

        var dicomFile = new DicomFile(dataset);
        var stream = new MemoryStream();
        dicomFile.Save(stream);
        stream.Position = 0;

        return stream;
    }
}
