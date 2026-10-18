namespace Brix.Util.Profiling;

public interface IProfiler
{
	void BeginSample(string s);

	void EndSample();

	void PrintSamples();
}
