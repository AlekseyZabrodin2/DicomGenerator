using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DicomGenerator.Core.DicomFileParser;
using DicomGenerator.Core.LiteDbModels;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

namespace DicomGenerator.UI.Wpf.LocalPacsModules
{
    public partial class LocalPacsSource : ObservableObject
    {
        private readonly DicomParser _parser;

        [ObservableProperty]
        public partial ObservableCollection<PatientLiteDb> PreviewPatients { get; set; }

        [ObservableProperty]
        public partial int PatientsCount { get; set; }

        [ObservableProperty]
        public partial int StudiesCount { get; set; }

        [ObservableProperty]
        public partial int SeriesCount { get; set; }

        [ObservableProperty]
        public partial int ImagesCount { get; set; }



        public LocalPacsSource()
        {
            _parser = new DicomParser();
        }

        

        public async Task<List<DicomFileInfo>> LoadDicomMetadataAsync(IDicomClient client, 
            IProgress<int> progress,
            CancellationToken cancellationToken = default)
        {
            PreviewPatients = new();
            PatientsCount = 0;
            StudiesCount = 0;
            SeriesCount = 0;
            ImagesCount = 0;

            var results = new List<DicomFileInfo>();

            var studies = await LoadStudiesAsync(client, cancellationToken);
            StudiesCount = studies.Count;

            var processed = 0;

            foreach (var studyDataset in studies)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var studyInfo = _parser.ParseDataset(studyDataset);

                if (studyInfo == null)
                    continue;

                if (!PreviewPatients.Any(x => x.PatientID == studyInfo.Patient.PatientID))
                {
                    PreviewPatients.Add(studyInfo.Patient);
                    PatientsCount++;
                }

                var studyUid = studyDataset.GetSingleValueOrDefault<string>(DicomTag.StudyInstanceUID, string.Empty);

                if (string.IsNullOrEmpty(studyUid))
                    continue;

                var studyFiles = await LoadSeriesAsync(client, studyUid, studyInfo.Patient, studyInfo.Study, cancellationToken);                

                results.AddRange(studyFiles);

                processed++;

                progress?.Report((int)((double)processed / studies.Count * 100));
            }

            return results;
        }

        private async Task<List<DicomDataset>> LoadStudiesAsync(IDicomClient client, CancellationToken cancellationToken)
        {
            var result = new List<DicomDataset>();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var request = DicomCFindRequest.CreateStudyQuery();

            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, string.Empty);

            request.OnResponseReceived += (req, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    if (response.Dataset != null)
                    {
                        result.Add(response.Dataset);
                    }
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            };

            cancellationToken.ThrowIfCancellationRequested();

            await client.AddRequestAsync(request);

            await client.SendAsync();

            await tcs.Task;

            return result;
        }

        private async Task<List<DicomFileInfo>> LoadSeriesAsync(IDicomClient client, string studyUid, PatientLiteDb patient, StudyLiteDb study, CancellationToken cancellationToken)
        {
            var results = new List<DicomFileInfo>();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var seriesDatasets = new List<DicomDataset>();

            var request = DicomCFindRequest.CreateSeriesQuery(studyUid);

            request.OnResponseReceived += (req, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    if (response.Dataset != null)
                    {
                        seriesDatasets.Add(response.Dataset);
                    }                        
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            };

            cancellationToken.ThrowIfCancellationRequested();

            await client.AddRequestAsync(request);
            await client.SendAsync();

            await tcs.Task;


            foreach (var seriesDataset in seriesDatasets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var seriesInfo = _parser.ParseDataset(seriesDataset);

                if (seriesInfo == null)
                    continue;

                SeriesCount++;

                var seriesUid = seriesDataset.GetSingleValueOrDefault<string>(DicomTag.SeriesInstanceUID, string.Empty);

                if (string.IsNullOrWhiteSpace(seriesUid))
                    continue;

                var images = await LoadImagesAsync(client, studyUid, seriesUid, cancellationToken);

                foreach (var image in images)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    results.Add(new DicomFileInfo
                    {
                        Patient = patient,
                        Study = study,
                        Series = seriesInfo.Series,
                        Image = image
                    });
                }
            }

            return results;
        }

        private async Task<List<ImageLiteDb>> LoadImagesAsync(IDicomClient client, string studyUid, string seriesUid, CancellationToken cancellationToken)
        {
            var results = new List<ImageLiteDb>();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var images = new List<DicomDataset>();

            var request = DicomCFindRequest.CreateImageQuery(studyUid, seriesUid);

            request.OnResponseReceived += (req, response) =>
            {
                if (response.Status == DicomStatus.Pending)
                {
                    if (response.Dataset != null)
                        images.Add(response.Dataset);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            };

            cancellationToken.ThrowIfCancellationRequested();

            await client.AddRequestAsync(request);
            await client.SendAsync();

            await tcs.Task;

            foreach (var dataset in images)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var imageInfo = _parser.ParseDataset(dataset);

                if (imageInfo == null)
                    continue;

                ImagesCount++;

                results.Add(imageInfo.Image);
            }

            return results;
        }
    }
}
