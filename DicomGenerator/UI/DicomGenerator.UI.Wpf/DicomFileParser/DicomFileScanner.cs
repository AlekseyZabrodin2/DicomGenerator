using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DicomGenerator.Core.LiteDbModels;
using NLog;

namespace DicomGenerator.Core.DicomFileParser
{
    public class DicomFileScanner
    {
        private readonly DicomParser _parser;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public List<PatientLiteDb> PreviewPatients { get; set; } = new();

        public int PatientsCount { get; private set; }
        public int StudiesCount { get; private set; }
        public int SeriesCount { get; private set; }
        public int ImagesCount { get; private set; }


        public DicomFileScanner()
        {
            _parser = new DicomParser();
        }


        public async Task<List<DicomFileInfo>> ScanFolderAsync(string folderPath,
            IProgress<(int Percent, string Message)> progress = null,
            CancellationToken cancellationToken = default)
        {
            PreviewPatients.Clear();
            PatientsCount = 0;
            StudiesCount = 0;
            SeriesCount = 0;
            ImagesCount = 0;

            var results = new List<DicomFileInfo>();

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

            if (files.Length == 0)
                return results;

            results = await Task.Run(() => ScanFiles(files, progress, cancellationToken));

            return results;
        }

        private List<DicomFileInfo> ScanFiles(string[] files,
            IProgress<(int Percent, string Message)> progress = null,
            CancellationToken cancellationToken = default)
        {
            var results = new List<DicomFileInfo>();

            var patientIds = new HashSet<string>();
            var studyUids = new HashSet<string>();
            var seriesUids = new HashSet<string>();
            var imageUids = new HashSet<string>();

            var parsedCount = 0;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var info = _parser.ParseFile(file);
                    if (info != null)
                    {
                        if (patientIds.Add(info.Patient.PatientID))
                        {
                            PreviewPatients.Add(info.Patient);
                            PatientsCount++;
                        }

                        if (!string.IsNullOrWhiteSpace(info.Study.StudyInstanceUid)
                            && studyUids.Add(info.Study.StudyInstanceUid))
                        {
                            StudiesCount++;
                        }

                        if (!string.IsNullOrWhiteSpace(info.Series.SeriesInstanceUid)
                            && seriesUids.Add(info.Series.SeriesInstanceUid))
                        {
                            SeriesCount++;
                        }

                        if (!string.IsNullOrWhiteSpace(info.Image.SopInstanceUid)
                            && imageUids.Add(info.Image.SopInstanceUid))
                        {
                            ImagesCount++;
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        results.Add(info);

                        parsedCount++;
                        var percent = (int)((double)parsedCount / files.Length * 100);
                        progress?.Report((percent, "Сканирование исследований ... "));
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Error parsing {file}: {ex.Message}");
                }
            }

            return results;
        }
    }
}
