using System.Windows;

namespace Touchpad2ArrowKey
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public App() : base() {
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
		}

		static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			MessageBox.Show("Unhandled exception occurred: \n" + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}