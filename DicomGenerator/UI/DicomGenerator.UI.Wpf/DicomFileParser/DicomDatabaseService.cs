using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DicomGenerator.Core.Extensions;
using DicomGenerator.Core.LiteDbModels;
using DicomGenerator.UI.Wpf.LiteDbCore;
using Windows.UI;

namespace DicomGenerator.UI.Wpf.DicomFileParser
{    
    public class DicomDatabaseService : IDisposable
    {
        private readonly LiteDbContext _dbContext;
        private readonly int _batchSize = 10000;

        public DicomDatabaseService(LiteDbContext dbContext)
        {
            _dbContext = dbContext;
        }



        public void SaveToDatabase(List<DicomFileInfo> dicomFiles, 
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            var patientsToInsert = new List<LitePatient>(_batchSize);
            var studiesToInsert = new List<LiteStudy>(_batchSize);
            var seriesToInsert = new List<LiteSeries>(_batchSize);
            var imagesToInsert = new List<LiteImage>(_batchSize);
            var patientIds = new HashSet<string>();
            var studyUids = new HashSet<string>();
            var seriesUids = new HashSet<string>();
            var imageUids = new HashSet<string>();

            cancellationToken.ThrowIfCancellationRequested();

            if (dicomFiles == null || dicomFiles.Count == 0)
                return;

            var total = dicomFiles.Count;
            var processed = 0;            

            var patients = dicomFiles
                .Where(x =>
                x.Patient != null &&
                x.Study != null &&
                x.Series != null &&
                x.Image != null)
                .GroupBy(x => x.Patient.PatientID);

            foreach (var patientGroup in patients)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SavePatient(patientGroup.First().Patient, patientsToInsert, patientIds);

                var studies = patientGroup
                    .GroupBy(x => x.Study.StudyInstanceUid);

                foreach (var studyGroup in studies)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    SaveStudy(studyGroup.First().Study, studiesToInsert, studyUids);

                    var series = studyGroup
                        .GroupBy(x => x.Series.SeriesInstanceUid);

                    foreach (var seriesGroup in series)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        SaveSeries(seriesGroup.First().Series, seriesToInsert, seriesUids);

                        foreach (var dicomFile in seriesGroup)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            SaveImage(dicomFile.Image, imagesToInsert, imageUids);

                            processed++;

                            if (processed % 10 == 0 || processed == total)
                            {
                                var percent = (int)((double)processed / total * 100);
                                progress?.Report(percent);
                            }
                        }
                    }
                }
            }

            if (patientsToInsert.Count > 0)
                _dbContext.Patients.InsertBulk(patientsToInsert);

            if (studiesToInsert.Count > 0)
                _dbContext.Studies.InsertBulk(studiesToInsert);

            if (seriesToInsert.Count > 0)
                _dbContext.Series.InsertBulk(seriesToInsert);

            if (imagesToInsert.Count > 0)
                _dbContext.Images.InsertBulk(imagesToInsert);
        }

        private void SavePatient(PatientLiteDb patient, List<LitePatient> patientsToInsert, HashSet<string> patientIds)
        {
            if (patient == null || string.IsNullOrWhiteSpace(patient.PatientID))
                return;

            var exist = _dbContext.Patients.FindOne(x => x.PatientId == patient.PatientID);

            if (exist != null)
            {
                patient.Id = exist.Id.ToString();
                _dbContext.Patients.Update(patient.ToLitePatient());
            }
            else
            {
                if (patientIds.Add(patient.PatientID))
                {
                    patientsToInsert.Add(patient.ToLitePatient());
                }
            }
        }

        private void SaveStudy(StudyLiteDb study, List<LiteStudy> studiesToInsert, HashSet<string> studyUids)
        {
            if (study == null || string.IsNullOrWhiteSpace(study.StudyInstanceUid))
                return;

            var exist = _dbContext.Studies
                .FindOne(x => x.StudyInstanceUid == study.StudyInstanceUid);

            if (exist != null)
            {
                study.Id = exist.Id;
                _dbContext.Studies.Update(study.ToLiteStudy());
            }
            else
            {
                if (studyUids.Add(study.StudyInstanceUid))
                {
                    studiesToInsert.Add(study.ToLiteStudy());
                }
            }
        }

        private void SaveSeries(SeriesLiteDb series, List<LiteSeries> seriesToInsert, HashSet<string> seriesUids)
        {
            if (series == null || string.IsNullOrWhiteSpace(series.SeriesInstanceUid))
                return;

            var exist = _dbContext.Series
                .FindOne(x => x.SeriesInstanceUid == series.SeriesInstanceUid);

            if (exist != null)
            {
                series.Id = exist.Id;
                _dbContext.Series.Update(series.ToLiteSeries());
            }
            else
            {
                if (seriesUids.Add(series.SeriesInstanceUid))
                {
                    seriesToInsert.Add(series.ToLiteSeries());
                }
            }
        }

        private void SaveImage(ImageLiteDb image, List<LiteImage> imagesToInsert, HashSet<string> imageUids)
        {            
            if (image == null || string.IsNullOrWhiteSpace(image.SopInstanceUid))
                return;

            var exist = _dbContext.Images
                .FindOne(x => x.SopInstanceUid == image.SopInstanceUid);

            if (exist != null)
            {
                image.Id = exist.Id;
                _dbContext.Images.Update(image.ToLiteImage());
            }
            else
            {
                if (imageUids.Add(image.SopInstanceUid))
                {
                    imagesToInsert.Add(image.ToLiteImage());
                }
            }
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
