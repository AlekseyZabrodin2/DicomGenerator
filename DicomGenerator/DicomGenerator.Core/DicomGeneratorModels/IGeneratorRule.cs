namespace DicomGenerator.Core.DicomGeneratorModels
{
    public interface IGeneratorRule<T>
    {
        T Generate();
    }

    public interface IGeneratorRule<T,Tin>
    {
        T Generate(Tin parameter);
    }
}