namespace Albatross.Config {
	public interface IApplicationPath {
		bool IsSystemPath { get; }
		string DataRoot { get; }
		string ConfigRoot { get; }
		string LogRoot { get; }
		void Init();
	}
}