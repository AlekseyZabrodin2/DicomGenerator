using System;
using System.Collections.Generic;
using System.IO;
using FellowOakDicom;
using FellowOakDicom.StructuredReport;

namespace DicomGenerator.Core.GeneratorRules.Sop
{
    public class SopClassFactory : ISopClassFactory
    {
        private readonly IDictionary<SopClassUid, Func<DicomDataset>> _factoryMethods;
        private readonly IDictionary<SopClassUid, IList<FileInfo>> _fileInfoDictionary;

        public SopClassFactory(string dicomDirectory)
        {
            _factoryMethods =
                new Dictionary<SopClassUid, Func<DicomDataset>>
                {
                    {SopClassUid.DxPresentation, CreateDxPresentation},
                    {SopClassUid.MgPresentation, CreateMgPresentation},
                    {SopClassUid.SrBasic, CreateSrBasic}
                };

            _fileInfoDictionary = AnalizeDirectory(dicomDirectory);
        }

        private IDictionary<SopClassUid, IList<FileInfo>> AnalizeDirectory(string dicomDirectory)
        {
            var dictionary = new Dictionary<SopClassUid, IList<FileInfo>>();

            if (Directory.Exists(dicomDirectory))
            {
                foreach (var file in Directory.EnumerateFiles(dicomDirectory, "*", SearchOption.AllDirectories))
                {
                    var fileInfo = new FileInfo(file);
                    if (DicomFile.HasValidHeader(fileInfo.FullName))
                    {
                        var dicomFile = DicomFile.Open(fileInfo.FullName);
                        if (dicomFile.Dataset.TryGetString(DicomTag.SOPClassUID, out var uid))
                        {
                            var sopClassUID = SopClassUid.FromUidString(uid);

                            if (sopClassUID != SopClassUid.Unknown)
                            {
                                if (!dictionary.TryGetValue(sopClassUID, out var list))
                                {
                                    dictionary.Add(sopClassUID,new List<FileInfo>{fileInfo});
                                }
                                else
                                {
                                    list.Add(fileInfo);
                                }
                                
                            }
                        }
                    }
                }
            }


            return dictionary;
        }

        public DicomDataset GenerateDataset(SopClassUid sopClassUid)
        {
            if (_factoryMethods.TryGetValue(sopClassUid, out var creationFunc))
            {
                return creationFunc.Invoke();
            }

            throw new ArgumentException($"The {sopClassUid.Name} with uid {sopClassUid.Uid} is not supported");
        }

        private DicomDataset CreateDxPresentation()
        {
            var dataset = new DicomDataset();
            dataset.AddOrUpdate(DicomTag.SOPClassUID, SopClassUid.DxPresentation.Uid);
            
            if (_fileInfoDictionary.TryGetValue(SopClassUid.DxPresentation, out var files))
            {
                var random = new Random();
                var file = files[random.Next(0, files.Count - 1)];

                CopyImageDatasetFromFile(file, dataset);
            }

            return dataset;
        }

        private DicomDataset CreateMgPresentation()
        {
            var dataset = new DicomDataset();
            dataset.AddOrUpdate(DicomTag.SOPClassUID, SopClassUid.MgPresentation.Uid);
          
            if (_fileInfoDictionary.TryGetValue(SopClassUid.MgPresentation, out var files))
            {
                var random = new Random();
                var file = files[random.Next(0, files.Count - 1)];

                CopyImageDatasetFromFile(file, dataset);
            }

            return dataset;
        }

        private DicomDataset CreateSrBasic()
        {
            var dataset = new DicomDataset();
            dataset.AddOrUpdate(DicomTag.SOPClassUID, SopClassUid.SrBasic.Uid);
            
            if (_fileInfoDictionary.TryGetValue(SopClassUid.SrBasic, out var files))
            {
                var random = new Random();
                var file = files[random.Next(0, files.Count - 1)];

                CopyTextDatasetFromFile(file,dataset);
            }

            return dataset;
        }

        private static void CopyImageDatasetFromFile(FileInfo file, DicomDataset dataset)
        {
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, UidGenerator.GenerateImageUid());

            var dicomFile = DicomFile.Open(file.FullName, FileReadOption.ReadAll);

            dicomFile.Dataset.CopyTo(dataset, DicomTag.PixelData);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.Columns);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.Rows);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.SamplesPerPixel);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.PhotometricInterpretation);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.BitsStored);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.HighBit);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.PixelRepresentation);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.BitsAllocated);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ContentDate);

        }

        private static void CopyTextDatasetFromFile(FileInfo file, DicomDataset dataset)
        {
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, UidGenerator.GenerateImageUid());

            var dicomFile = DicomFile.Open(file.FullName, FileReadOption.ReadAll);
            
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ContentDate);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ContentTime);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.InstanceNumber);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.PerformedProcedureCodeSequence);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.CompletionFlag);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ReferencedSOPSequence);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ValueType);
            dicomFile.Dataset.CopyTo(dataset, DicomTag.ContinuityOfContent);
        }
    }
}