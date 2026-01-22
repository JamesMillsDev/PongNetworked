using System.Runtime.InteropServices;
using DaisNET.Utility;
using Raylib_cs;

namespace Pong.Client
{
	public static unsafe class PongTraceLog
	{
		public static void SetupLog() => Raylib.SetTraceLogCallback(&TraceCallback);

		[UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
		private static void TraceCallback(int logLevel, sbyte* text, sbyte* args)
		{
			ConsoleLog.Log(Logging.GetLogMessage(new IntPtr(text), new IntPtr(args)), (LogLevel)logLevel);
		}
	}
}