using DicomGenerator.Core.Enums;

namespace DicomGenerator.UI.Wpf.Configuration
{
    public class PacsConfig
    {
        public string Name { get; set; } = string.Empty;
        public string CallingAe { get; set; } = string.Empty;
        public string PacsHost { get; set; } = string.Empty;
        public string PacsPort { get; set; } = string.Empty;
        public string CalledAe { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public PacsLoadingMode LoadingMode { get; set; }
    }
}
