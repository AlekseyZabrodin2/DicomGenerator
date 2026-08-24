using System.Collections.Generic;

namespace DicomGenerator.UI.Wpf.Configuration
{
    public class SourcesConfig
    {
        public string DataBasePath { get; set; } = string.Empty;
        public List<FolderConfig> Folders { get; set; } = [];
        public List<PacsConfig> Pacs { get; set; } = [];
    }
}
