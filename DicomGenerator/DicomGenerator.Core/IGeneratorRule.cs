namespace DicomGenerator.Core
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