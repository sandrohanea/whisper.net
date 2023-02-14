
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Demo;
using Whisper.net.Ggml;
using Whisper.net.Wave;


const int bufferSeconds = 1;
const int bufferMilliseconds = bufferSeconds * 1000;
const int maxSlidingBufferSize = 60 / bufferSeconds;
List<float[]> slidingBuffer = new(bufferMilliseconds);

await Parser.Default
	.ParseArguments<Options>(args)
	.WithParsedAsync(Demo);

async Task Demo(Options opt)
{
	Console.OutputEncoding = Encoding.UTF8;
	if (!File.Exists(opt.ModelName))
	{
		Console.WriteLine($"Downloading Model {opt.ModelName}");
		using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(opt.ModelType);
		using var fileWriter = File.OpenWrite(opt.ModelName);
		await modelStream.CopyToAsync(fileWriter);
	}

	switch (opt.Command)
	{
		case WhisperCommand.LanguageDetect:
			LanguageIdentification(opt);
			break;
		case WhisperCommand.Transcribe:
		case WhisperCommand.Translate:
			FullDetection(opt);
			break;
		default:
			Console.WriteLine("Unknown command");
			break;
	}
}

void LanguageIdentification(Options opt)
{
	var bufferedModel = File.ReadAllBytes(opt.ModelName);

	var builder = WhisperProcessorBuilder.Create()
	   .WithBufferedModel(bufferedModel)
	   .WithLanguage(opt.Language);

	using var processor = builder.Build();

	using var fileStream = File.OpenRead(opt.FileName);

	var wave = new WaveParser(fileStream);

	var samples = wave.GetAvgSamples();

	var language = processor.DetectLanguage(samples, speedUp: true);
	Console.WriteLine("Language is " + language);
}

void FullDetection(Options opt)
{
	var builder = WhisperProcessorBuilder.Create()
	   .WithFileModel(opt.ModelName)
	   .WithSegmentEventHandler(OnNewSegment)
	   .WithLanguage(opt.Language)
	   .WithLanguageDetection();

	if (opt.Command == WhisperCommand.Translate)
	{
		builder.WithTranslate();
	}

	using var processor = builder.Build();

	static void OnNewSegment(object sender, OnSegmentEventArgs e)
	{
		Console.WriteLine($"New Segment: {e.Start} ==> {e.End} : {e.Segment}");
	}

	if (opt.UseInputDevice)
	{
		FullDetectionFromInputDevice(processor);
	}
	else
	{
		FullDetectionFromFile(processor, opt);
	}
}

void FullDetectionFromFile(WhisperProcessor processor, Options opt)
{
	using FileStream fileStream = File.OpenRead(opt.FileName);
	processor.Process(fileStream);
	var language = processor.GetAutodetectedLanguage();
	Console.WriteLine("Language was " + language);
}

void FullDetectionFromInputDevice(WhisperProcessor processor)
{
	WaveInEvent waveIn = new()
	{
		DeviceNumber = 0, // indicates which microphone to use
		WaveFormat = new WaveFormat(rate: 16000, bits: 16, channels: 1),
		BufferMilliseconds = bufferMilliseconds
	};
	waveIn.DataAvailable += WaveInDataAvailable;
	waveIn.StartRecording();
	Console.WriteLine("Listening for speech");

	void WaveInDataAvailable(object sender, WaveInEventArgs e)
	{
		short[] values = new short[e.Buffer.Length / 2];
		Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
		float[] samples = values.Select(x => x / (short.MaxValue + 1f)).ToArray();

		int silenceCount = samples.Count(x => IsSilence(x, -40));

		if (silenceCount < values.Length - values.Length / 12)
		{
			slidingBuffer.Add(samples);

			if (slidingBuffer.Count > maxSlidingBufferSize)
			{
				slidingBuffer.RemoveAt(0);
			}
			processor.Process(slidingBuffer.SelectMany(x => x).ToArray());
		}
	}

	Console.WriteLine("Press any key to stop listening");
	Console.ReadLine();
}

static bool IsSilence(float amplitude, sbyte threshold) 
	=> GetDecibelsFromAmplitude(amplitude) < threshold;

static double GetDecibelsFromAmplitude(float amplitude)
	=> 20 * Math.Log10(Math.Abs(amplitude));

public class Options
{
	[Option('t', "command", Required = false, HelpText = "Command to run (lang-detect, transcribe or translate)", Default = WhisperCommand.Transcribe)]
	public WhisperCommand Command { get; set; }

	[Option('f', "file", Required = false, HelpText = "File to process")]
	public string FileName { get; set; }

	[Option('i', "inputDevice", Required = false, HelpText = "Use input device such as microphone as source to process", Default = true)]
	public bool UseInputDevice { get; set; }

	[Option('l', "lang", Required = false, HelpText = "Language", Default = "auto")]
	public string Language { get; set; }

	[Option('m', "modelFile", Required = false, HelpText = "Model to use (filename)", Default = "ggml-base.bin")]
	public string ModelName { get; set; }

	[Option('g', "ggml", Required = false, HelpText = "Ggml Model type to download (if not exists)", Default = GgmlType.Base)]
	public GgmlType ModelType { get; set; }
}
