using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Threading;
using Extensions;
using WindowsInput;
using WindowsInput.Native;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace Touchpad2ArrowKey
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private bool firstActivateAfterExecution;
		private bool isActivated;
		
		private Point lastPos;
		private bool lastPosHasValue;
		
		private Vector deltaVec;
		private double vecAngle;  //Degree
		private double vecLength;

		private double sensitivity;
		private int threadSleepTime;
		private double angleSensitivLR;
		private double angleSensitivUD;
		
		//Options
		private bool freezeCursor;
		private Point cursorFreezePos;

		private bool isDebugging;
		
		private readonly InputSimulator inputSim;
		private Thread t2AThread;
		
		private Timer activationTimer;
		
		public MainWindow()
		{
			firstActivateAfterExecution = false;
			isActivated = false;
			
			lastPos = new Point();  //임시값
			lastPosHasValue = false;
			
			deltaVec = new Vector();
			vecAngle = 0;
			vecLength = 0;

			sensitivity = 10;
			threadSleepTime = 3000;
			angleSensitivLR = 60;
			angleSensitivUD = 30;

			freezeCursor = false;
			cursorFreezePos = new Point();

			isDebugging = true;

			inputSim = new InputSimulator();

			activationTimer = new Timer {
				AutoReset = false, Interval = 750
			};


			InitializeComponent();
			
			DebugIsActivated.IsChecked = isActivated;
			DeactivateButton.IsEnabled = false;
			//todo Slider, ValueLabel 값 초기화 하기
			
				

			Hook.KeyboardHook.KeyDown += KeyboardHook_KeyDown;
			Hook.KeyboardHook.KeyUp += KeyboardHook_KeyUp;
			Hook.KeyboardHook.HookStart();
		}
		
		~MainWindow() {
			Hook.KeyboardHook.HookEnd();
		}
		
		private bool KeyboardHook_KeyDown(int vkCode) {
			DebuggingLogTextBox.AppendText($"{vkCode}|");

			if(vkCode.ToString() != "164") return true;
			
			if(activationTimer.Enabled) {
				activationTimer.Stop();

				if(isActivated)
					T2ADeactivate();
				else
					T2AActivate();
			}
			else { activationTimer.Start(); }

			
			return true;
		}

		private static bool KeyboardHook_KeyUp(int vkCode) {
			return true;
		}
		

		private void ActivateButton_Click(object sender, RoutedEventArgs e)
			=> T2AActivate();

		private void DeactivateButton_Click(object sender, RoutedEventArgs e) 
			=> T2ADeactivate();
		
		private void T2AActivate()
		{
			isActivated = true;
			DebugIsActivated.IsChecked = isActivated;
			ActivateButton.IsEnabled = false;
			DeactivateButton.IsEnabled = true;

			AngleSensitivitySlider.IsEnabled = false;
			SensitivitySlider.IsEnabled = false;
			//FreezeCursorCheckbox.IsEnabled = false;
			
			cursorFreezePos = System.Windows.Forms.Cursor.Position;
			
			t2AThread = new Thread(TouchPadToArrowKey) {
				IsBackground = true
			};
			t2AThread.Start();

			if(firstActivateAfterExecution) {
				Thread.Sleep(200);
				T2ADeactivate();
				T2AActivate();
				firstActivateAfterExecution = false;
			}
		}
		
		private void T2ADeactivate()
		{
			isActivated = false;
			DebugIsActivated.IsChecked = isActivated;
			DeactivateButton.IsEnabled = false;
			ActivateButton.IsEnabled = true;
			
			AngleSensitivitySlider.IsEnabled = true;
			SensitivitySlider.IsEnabled = true;
			//FreezeCursorCheckbox.IsEnabled = true;

			t2AThread.Join();
		}
		
		private void TouchPadToArrowKey()
		{
			while(isActivated) {
				var curPos = System.Windows.Forms.Cursor.Position;

				if(!lastPosHasValue) {
					lastPos = curPos;
					lastPosHasValue = true;
					return;
				}

				SetDeltaVector(curPos, lastPos);
				lastPos = curPos;

				vecAngle = Math.Atan2(deltaVec.X, deltaVec.Y) * (180 / Math.PI);
				vecLength = deltaVec.Length;

				if(deltaVec.Length == 0) continue;

				if(deltaVec.Length >= 2) {
					if((0 - angleSensitivUD) <= vecAngle && vecAngle <= (0 + angleSensitivUD))
						inputSim.Keyboard.KeyDown(VirtualKeyCode.DOWN);
					else if((90 - angleSensitivLR) <= vecAngle && vecAngle <= (90 + angleSensitivLR))
						inputSim.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
					else if(!((-180 + angleSensitivUD) <= vecAngle && vecAngle <= (180 - angleSensitivUD)))
						inputSim.Keyboard.KeyDown(VirtualKeyCode.UP);
					else if((-90 - angleSensitivLR) <= vecAngle && vecAngle <= (-90 + angleSensitivLR))
						inputSim.Keyboard.KeyDown(VirtualKeyCode.LEFT);
					
					Thread.Sleep((int) (threadSleepTime / (deltaVec.Length * sensitivity + 1)));
				}
				
				// if(freezeCursor)
				// 	System.Windows.Forms.Cursor.Position = cursorFreezePos;
			}
		}

		private void TouchpadToArrowKey_DebuggingLog()
		{
			if(isDebugging) {
				var direction = "NONE";

				if((0 - angleSensitivUD) <= vecAngle && vecAngle <= (0 + angleSensitivUD))
					direction = "DOWN";
				else if((90 - angleSensitivUD) <= vecAngle && vecAngle <= (90 + angleSensitivUD))
					direction = "RIGHT";
				else if(!((-180 + angleSensitivUD) <= vecAngle && vecAngle <= (180 - angleSensitivUD)))
					direction = "UP";
				else if((-90 - angleSensitivUD) <= vecAngle && vecAngle <= (-90 + angleSensitivUD))
					direction = "LEFT";

				DisplayDebuggingTexts(lastPos, deltaVec, vecAngle, direction);
			}
		}
		
		private void SetDeltaVector(Point curPosParam, Point lastPosParam)
		{
			deltaVec.X = curPosParam.X - lastPosParam.X;
			deltaVec.Y = curPosParam.Y - lastPosParam.Y;
		}


		private void DisplayDebuggingTexts(Point lastPosParam, Vector deltaVectorParam, double vecAngle, string vecDirection)
		{
			DebuggingLogTextBox.Text = $"Cursor Position  : ???, ???\n" +
			                           $"Last Position    : {lastPosParam.X}, {lastPosParam.Y}\n" +
			                           $"Delta Vector     : {deltaVec.X}, {deltaVec.Y}, \n" +
			                           $"                   {deltaVec.Length}\n" +
			                           $"Vector Angle     : {vecAngle}\n" +
			                           $"Vector Direction : {vecDirection}\n" +
			                           $"minVecLength     : {threadSleepTime}\n" +
			                           $"Is Simulated     : {vecLength >= threadSleepTime}";
		}

		private void AngleSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(AngleSensitivityValueLabel == null) return;
			
			angleSensitivUD = FloorRoundCeil.RoundFrom(AngleSensitivitySlider.Value, 2);
			AngleSensitivityValueLabel.Content = $"AngleSensitivity: {angleSensitivUD}";
		}

		private void SensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(SensitivityValueLabel == null) return;
			
			sensitivity = FloorRoundCeil.RoundFrom(SensitivitySlider.Value, 2);
			SensitivityValueLabel.Content = $"Sensitivity: {sensitivity}";
		}

		private void FreezeCursorCheckbox_Clicked(object sender, RoutedEventArgs e)
		{
			if(FreezeCursorCheckbox.IsChecked != null)
				freezeCursor = (bool)FreezeCursorCheckbox.IsChecked;
		}
	}
}