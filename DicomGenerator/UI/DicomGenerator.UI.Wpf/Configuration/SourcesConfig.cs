using System.Collections.Generic;

namespace DicomGenerator.UI.Wpf.Configuration
{
    public class SourcesConfig
    {
        public List<FolderConfig> Folders { get; set; } = [];
        public List<PacsConfig> Pacs { get; set; } = [];
    }
}
