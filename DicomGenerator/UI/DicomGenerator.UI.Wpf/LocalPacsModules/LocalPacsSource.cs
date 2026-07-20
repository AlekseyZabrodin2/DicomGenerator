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
            IProgress<(int Percent, string Message)> progress,
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
                var percent = (int)((double)processed / studies.Count * 100);
                progress?.Report((percent, "Сканирование исследований ... "));
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

        private async Task<List<DicomFileInfo>> LoadSeriesAsync(IDicomClient client, 
            string studyUid, PatientLiteDb patient, 
            StudyLiteDb study, CancellationToken cancellationToken)
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

        private async Task<List<ImageLiteDb>> LoadImagesAsync(IDicomClient client, 
            string studyUid, string seriesUid, 
            CancellationToken cancellationToken)
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

        public async Task<List<DicomFileInfo>> LoadDicomMetadataFastAsync(IDicomClient client,
            IProgress<(int Percent, string Message)>? progress,
            CancellationToken cancellationToken = default)
        {
            PreviewPatients = new();

            PatientsCount = 0;
            StudiesCount = 0;
            SeriesCount = 0;
            ImagesCount = 0;

            progress?.Report((0, "Загрузка исследований ..."));

            var datasets = await LoadAllImagesAsync(client, progress, cancellationToken);

            progress?.Report((0, $"Сканирование исследований ..."));

            var result = new List<DicomFileInfo>();

            var patients = new Dictionary<string, PatientLiteDb>();
            var studies = new Dictionary<string, StudyLiteDb>();
            var series = new Dictionary<string, SeriesLiteDb>();

            var total = datasets.Count;
            var processed = 0;

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var dataset in datasets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var imageInfo = _parser.ParseDataset(dataset);

                    if (imageInfo != null)
                    {
                        var patientId = imageInfo.Patient.PatientID;
                        var studyUid = imageInfo.Study.StudyInstanceUid;
                        var seriesUid = imageInfo.Series.SeriesInstanceUid;

                        if (!patients.TryGetValue(patientId, out var patient))
                        {
                            patient = imageInfo.Patient;
                            patients.Add(patientId, patient);
                            PreviewPatients.Add(patient);
                            PatientsCount++;
                        }

                        if (!studies.TryGetValue(studyUid, out var study))
                        {
                            study = imageInfo.Study;
                            studies.Add(studyUid, study);
                            StudiesCount++;
                        }

                        if (!series.TryGetValue(seriesUid, out var serie))
                        {
                            serie = imageInfo.Series;
                            series.Add(seriesUid, serie);
                            SeriesCount++;
                        }

                        ImagesCount++;

                        result.Add(new DicomFileInfo
                        {
                            Patient = patient,
                            Study = study,
                            Series = serie,
                            Image = imageInfo.Image
                        });
                    }
                }
                finally
                {
                    processed++;
                    var percent = (int)((double)processed * 100 / total);
                    progress?.Report((percent, $"Parsing {processed}/{total}..."));
                }
            }

            return result;
        }

        private async Task<List<DicomDataset>> LoadAllImagesAsync(IDicomClient client,
            IProgress<(int Percent, string Message)>? progress,
            CancellationToken cancellationToken = default)
        {
            var images = new List<DicomDataset>();

            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);

            foreach (var tag in ImageQueryTags)
            {
                request.Dataset.AddOrUpdate(tag, "");
            }

            Exception? pacsException = null;
            void Handler(DicomCFindRequest _, DicomCFindResponse response)
            {
                if (response.Status == DicomStatus.Pending && response.Dataset != null)
                {
                    images.Add(response.Dataset);
                    return;
                }
                if (response.Status.State == DicomState.Failure)
                {
                    pacsException = new InvalidOperationException($"PACS C-FIND error: {response.Status}");
                }
            }

            request.OnResponseReceived += Handler;

            try
            {
                await client.AddRequestAsync(request);
                await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyAbortAssociation);

                if (pacsException != null)
                    throw pacsException;

                return images;
            }
            catch (OperationCanceledException)
            {
                progress?.Report((0, "Запрос отменен"));
                throw;
            }
            finally
            {
                request.OnResponseReceived -= Handler;
            }
        }

        private static readonly DicomTag[] ImageQueryTags =
        {
            DicomTag.PatientID,
            DicomTag.PatientName,
            DicomTag.PatientBirthDate,
            DicomTag.PatientSex,
            DicomTag.PatientTelephoneNumbers,
            DicomTag.PatientAddress,
            DicomTag.PatientComments,

            DicomTag.StudyInstanceUID,
            DicomTag.StudyID,
            DicomTag.StudyDate,
            DicomTag.StudyTime,
            DicomTag.AccessionNumber,
            DicomTag.StudyDescription,

            DicomTag.SeriesInstanceUID,
            DicomTag.SeriesDescription,
            DicomTag.SeriesNumber,
            DicomTag.Modality,
            DicomTag.BodyPartExamined,
            DicomTag.OperatorsName,

            DicomTag.SOPInstanceUID,
            DicomTag.InstanceNumber,
            DicomTag.ImageLaterality,
            DicomTag.AcquisitionTime,
            DicomTag.EntranceDose
        };
    }
}
