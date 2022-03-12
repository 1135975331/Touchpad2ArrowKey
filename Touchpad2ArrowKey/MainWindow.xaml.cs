using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Point = System.Drawing.Point;

namespace Touchpad2ArrowKey
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private Point lastPos;
		private bool lastPosHasValue;
		
		private Vector deltaVec;
		private double vecAngle;  //Degree
		private double vecLength;

		private double sensivity;
		private double minVecLength;
		private double angleSensiv;

		private bool isDebugging;
		
		private readonly InputSimulator inputSim;
		public MainWindow()
		{
			lastPos = new Point(0, 0);  //임시값
			lastPosHasValue = false;
			
			deltaVec = new Vector(0, 0);
			vecAngle = 0;
			vecLength = 0;

			sensivity = 10;
			minVecLength = Math.Max(1, 100 / sensivity);
			angleSensiv = 30;

			isDebugging = true;

			inputSim = new InputSimulator();

			InitializeComponent();

			Hook.KeyboardHook.KeyDown += KeyboardHook_KeyDown;
			Hook.KeyboardHook.KeyUp += KeyboardHook_KeyUp;
			Hook.KeyboardHook.HookStart();
		}
		
		~MainWindow() {
			Hook.KeyboardHook.HookEnd();
		}
		
		private bool KeyboardHook_KeyDown(int vkCode) {
			DebuggingLogTextBox.AppendText($"{vkCode}|");
			if(Keyboard.Modifiers == ModifierKeys.Shift)  //todo 토글 방식으로 변경하기
				TouchPadToArrowKey();
			
			return true;
		}
		
		private void TouchPadToArrowKey()
		{
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
			
			if(vecLength >= minVecLength) {
				if((0 - angleSensiv) <= vecAngle && vecAngle <= (0 + angleSensiv)) 
					inputSim.Keyboard.KeyDown(VirtualKeyCode.DOWN);
				else if((90 - angleSensiv) <= vecAngle && vecAngle <= (90 + angleSensiv)) 
					inputSim.Keyboard.KeyDown(VirtualKeyCode.RIGHT);
				else if(!((-180 + angleSensiv) <= vecAngle && vecAngle <= (180 - angleSensiv))) 
					inputSim.Keyboard.KeyDown(VirtualKeyCode.UP);
				else if((-90 - angleSensiv) <= vecAngle && vecAngle <= (-90 + angleSensiv)) 
					inputSim.Keyboard.KeyDown(VirtualKeyCode.LEFT);
			}

			if(isDebugging) {
				var direction = "NONE";
				
				if((0 - angleSensiv) <= vecAngle && vecAngle <= (0 + angleSensiv))
					direction = "DOWN";
				else if((90 - angleSensiv) <= vecAngle && vecAngle <= (90 + angleSensiv))
					direction = "RIGHT";
				else if(!((-180 + angleSensiv) <= vecAngle && vecAngle <= (180 - angleSensiv)))
					direction = "UP";
				else if((-90 - angleSensiv) <= vecAngle && vecAngle <= (-90 + angleSensiv))
					direction = "LEFT";

				DisplayDebuggingTexts(curPos, lastPos, deltaVec, vecAngle, direction);
			}
		}


		private void SetDeltaVector(Point curPosParam, Point lastPosParam)
		{
			deltaVec.X = curPosParam.X - lastPosParam.X;
			deltaVec.Y = curPosParam.Y - lastPosParam.Y;
		}


		private void DisplayDebuggingTexts(Point curPosParam, Point lastPosParam, Vector deltaVectorParam, double vecAngle, string vecDirection)
		{
			DebuggingLogTextBox.Text = $"Cursor Position  : {curPosParam.X}, {curPosParam.Y}\n" +
			                           $"Last Position    : {lastPosParam.X}, {lastPosParam.Y}\n" +
			                           $"Delta Vector     : {deltaVec.X}, {deltaVec.Y}, \n" +
			                           $"                   {deltaVec.Length}\n" +
			                           $"Vector Angle     : {vecAngle}\n" +
			                           $"Vector Direction : {vecDirection}\n" +
			                           $"minVecLength     : {minVecLength}\n" +
			                           $"Is Simulated     : {vecLength >= minVecLength}";
		}

		private static bool KeyboardHook_KeyUp(int vkCode) {
			return true;
		}
	}
}